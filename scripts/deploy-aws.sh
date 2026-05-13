#!/usr/bin/env bash
# scripts/deploy-aws.sh
#
# End-to-end deployment for the SavePoint app to AWS (Learner Lab friendly).
# Re-runnable: safe to run multiple times in the same Learner Lab session.
#
# Run from the repository root:  bash scripts/deploy-aws.sh

set -euo pipefail

# ----- Paths --------------------------------------------------------------
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TF_DIR="${REPO_ROOT}/infra/terraform"
K8S_DIR="${REPO_ROOT}/infra/k8s-aws"

# ----- Output helpers -----------------------------------------------------
log()  { printf "\n\033[1;34m==>\033[0m %s\n" "$*"; }
ok()   { printf "\033[1;32m[OK]\033[0m %s\n" "$*"; }
warn() { printf "\033[1;33m[WARN]\033[0m %s\n" "$*"; }
fail() { printf "\033[1;31m[FAIL]\033[0m %s\n" "$*" >&2; exit 1; }

# ----- 1. Tool checks -----------------------------------------------------
log "Checking required tools"
for tool in docker kubectl helm terraform aws kustomize; do
  if ! command -v "${tool}" >/dev/null 2>&1; then
    fail "Missing required tool: ${tool}. Install it and re-run."
  fi
done
ok "docker, kubectl, helm, terraform, aws, kustomize found"

# ----- 2. Region / account env --------------------------------------------
log "Checking AWS_REGION"
if [[ -z "${AWS_REGION:-}" ]]; then
  warn "AWS_REGION not set — defaulting to us-east-1"
  export AWS_REGION="us-east-1"
fi
ok "AWS_REGION=${AWS_REGION}"

# ----- 3. Credentials check -----------------------------------------------
log "Verifying AWS credentials (aws sts get-caller-identity)"
if ! aws sts get-caller-identity >/dev/null 2>&1; then
  cat <<EOF >&2
[FAIL] AWS credentials are missing or expired.

Refresh them from the AWS Learner Lab portal:
  1. Open the Learner Lab in your browser and click "Start Lab".
  2. Wait for the green dot, then click "AWS Details".
  3. Click "Show" next to "AWS CLI" and copy the three lines.
  4. Paste them into this terminal exactly as shown, e.g.:

       export AWS_ACCESS_KEY_ID="ASIA..."
       export AWS_SECRET_ACCESS_KEY="..."
       export AWS_SESSION_TOKEN="..."

  5. Re-run:  bash scripts/deploy-aws.sh
EOF
  exit 1
fi
AWS_ACCOUNT_ID="$(aws sts get-caller-identity --query Account --output text)"
export AWS_ACCOUNT_ID
ok "AWS account ${AWS_ACCOUNT_ID}, region ${AWS_REGION}"

# ----- 4. Terraform apply -------------------------------------------------
log "Running terraform init / plan / apply (this can take 15–20 min on first run)"
cd "${TF_DIR}"

if [[ ! -f .terraform.lock.hcl || ! -d .terraform ]]; then
  warn "Terraform not initialised. Run 'terraform init' yourself first (see README) so the S3 backend bucket is set."
  fail "Stopping. See infra/terraform/README.md > 'First-time backend setup'."
fi

terraform plan -out=tfplan
terraform apply -auto-approve tfplan
rm -f tfplan
ok "Terraform apply done"

# ----- 5. Read outputs ----------------------------------------------------
log "Reading Terraform outputs"
CLUSTER_NAME="$(terraform output -raw cluster_name)"
ECR_URLS_JSON="$(terraform output -json ecr_repository_urls)"
ECR_BACKEND=$(echo  "${ECR_URLS_JSON}" | python -c "import json,sys;print(json.load(sys.stdin)['savepoint-backend'])")
ECR_REVIEWS=$(echo  "${ECR_URLS_JSON}" | python -c "import json,sys;print(json.load(sys.stdin)['savepoint-reviews'])")
ECR_LOOKUP=$(echo   "${ECR_URLS_JSON}" | python -c "import json,sys;print(json.load(sys.stdin)['savepoint-lookup'])")
ECR_FRONTEND=$(echo "${ECR_URLS_JSON}" | python -c "import json,sys;print(json.load(sys.stdin)['savepoint-frontend'])")
REVIEW_QUEUE_URL="$(terraform output -raw review_events_queue_url 2>/dev/null || true)"
REVIEW_SENTIMENT_TABLE="$(terraform output -raw review_sentiment_table 2>/dev/null || true)"
TOP_GAMES_URL="$(terraform output -raw top_games_object_url 2>/dev/null || true)"
TOP_GAMES_LAMBDA="$(terraform output -raw top_games_lambda_name 2>/dev/null || true)"
ok "Cluster: ${CLUSTER_NAME}"
ok "ECR registry: ${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com"
ok "Review queue: ${REVIEW_QUEUE_URL:-<missing>}"
ok "Top-games snapshot URL: ${TOP_GAMES_URL:-<missing>}"

# ----- 6. Docker login to ECR ---------------------------------------------
log "Authenticating Docker to ECR"
aws ecr get-login-password --region "${AWS_REGION}" \
  | docker login --username AWS --password-stdin "${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com"
ok "Docker logged in to ECR"

# ----- 7. Build, tag, push every image -----------------------------------
TAG="phase2"

build_push () {
  local name="$1"
  local context="$2"
  local dockerfile="$3"
  local repo_url="$4"
  shift 4
  local build_args=("$@")

  log "Building ${name}"
  docker build \
    -t "${name}:${TAG}" \
    -f "${dockerfile}" \
    "${build_args[@]}" \
    "${context}"

  docker tag "${name}:${TAG}" "${repo_url}:${TAG}"
  docker push "${repo_url}:${TAG}"
  ok "Pushed ${repo_url}:${TAG}"
}

build_push savepoint-backend  "${REPO_ROOT}/SavePointBackend"  "${REPO_ROOT}/SavePointBackend/SavePoint.Host/Dockerfile"           "${ECR_BACKEND}"
build_push savepoint-reviews  "${REPO_ROOT}/SavePointBackend"  "${REPO_ROOT}/SavePointBackend/SavePoint.ReviewsService/Dockerfile" "${ECR_REVIEWS}"
build_push savepoint-lookup   "${REPO_ROOT}/SavePointBackend"  "${REPO_ROOT}/SavePointBackend/SavePoint.LookupService/Dockerfile"  "${ECR_LOOKUP}"
# Frontend uses path-based routing through the ALB, so the API base URL is the
# same host as the frontend (empty value = relative paths). VITE_TOP_GAMES_URL
# is the public S3 URL of the top-games snapshot; baked into the JS bundle.
build_push savepoint-frontend "${REPO_ROOT}/SavePointFrontend" "${REPO_ROOT}/SavePointFrontend/Dockerfile" "${ECR_FRONTEND}" \
  --build-arg "VITE_API_BASE_URL=" \
  --build-arg "VITE_TOP_GAMES_URL=${TOP_GAMES_URL}"

# ----- 8. kubeconfig ------------------------------------------------------
log "Updating kubeconfig"
aws eks update-kubeconfig --region "${AWS_REGION}" --name "${CLUSTER_NAME}"
kubectl cluster-info >/dev/null
ok "kubectl is pointed at ${CLUSTER_NAME}"

# ----- 9. Wait for cluster add-ons ----------------------------------------
log "Waiting for AWS Load Balancer Controller pods to be ready"
kubectl -n kube-system rollout status deployment/aws-load-balancer-controller --timeout=5m
log "Waiting for EBS CSI controller to be ready"
kubectl -n kube-system rollout status deployment/ebs-csi-controller --timeout=5m

# ----- 10. Rewrite image refs in kustomization, then apply ----------------
log "Pointing kustomize manifests at ECR image URLs"
cd "${K8S_DIR}"
kustomize edit set image \
  "savepoint-backend=${ECR_BACKEND}:${TAG}" \
  "savepoint-reviews=${ECR_REVIEWS}:${TAG}" \
  "savepoint-lookup=${ECR_LOOKUP}:${TAG}" \
  "savepoint-frontend=${ECR_FRONTEND}:${TAG}"

log "Creating/updating savepoint-aws-config ConfigMap (queue URL + table name)"
kubectl -n savepoint create configmap savepoint-aws-config \
  --from-literal=REVIEW_EVENTS_QUEUE_URL="${REVIEW_QUEUE_URL}" \
  --from-literal=REVIEW_SENTIMENT_TABLE="${REVIEW_SENTIMENT_TABLE}" \
  --dry-run=client -o yaml | kubectl apply -f -

log "Applying manifests"
kubectl apply -k .

log "Restarting reviews deployment so it picks up the new ConfigMap values"
kubectl -n savepoint rollout restart deployment/savepoint-reviews || true

# ----- 11. Wait for rollouts ----------------------------------------------
log "Waiting for application rollouts"
kubectl -n savepoint rollout status statefulset/savepoint-db        --timeout=10m
kubectl -n savepoint rollout status deployment/savepoint-backend    --timeout=10m
kubectl -n savepoint rollout status deployment/savepoint-reviews    --timeout=10m
kubectl -n savepoint rollout status deployment/savepoint-lookup     --timeout=10m
kubectl -n savepoint rollout status deployment/savepoint-frontend   --timeout=10m
ok "All deployments rolled out"

# ----- 12. Print ALB URL --------------------------------------------------
log "Waiting for the ALB Ingress to be provisioned (can take 2–3 min)"
ALB_DNS=""
for i in $(seq 1 60); do
  ALB_DNS="$(kubectl -n savepoint get ingress savepoint-ingress -o jsonpath='{.status.loadBalancer.ingress[0].hostname}' 2>/dev/null || true)"
  if [[ -n "${ALB_DNS}" ]]; then break; fi
  printf "."
  sleep 5
done
echo

if [[ -z "${ALB_DNS}" ]]; then
  warn "ALB hostname not yet visible. Re-run:  kubectl -n savepoint get ingress"
  exit 0
fi

# ----- 13. Patch the top-games Lambda with the ALB URL --------------------
if [[ -n "${ALB_DNS}" && -n "${TOP_GAMES_LAMBDA}" ]]; then
  APP_BASE="http://${ALB_DNS}"
  log "Patching ${TOP_GAMES_LAMBDA} env APP_BASE_URL=${APP_BASE}"
  aws lambda update-function-configuration \
    --region "${AWS_REGION}" \
    --function-name "${TOP_GAMES_LAMBDA}" \
    --environment "Variables={APP_BASE_URL=${APP_BASE},SNAPSHOT_BUCKET=$(aws lambda get-function-configuration --region ${AWS_REGION} --function-name ${TOP_GAMES_LAMBDA} --query 'Environment.Variables.SNAPSHOT_BUCKET' --output text),SNAPSHOT_KEY=top-games/latest.json,POPULARITY_TYPE=1,PAGE_SIZE=10}" \
    --output json >/dev/null
  ok "Lambda APP_BASE_URL set"

  log "Invoking the top-games Lambda once so the first snapshot is ready"
  aws lambda invoke --region "${AWS_REGION}" --function-name "${TOP_GAMES_LAMBDA}" \
    --cli-binary-format raw-in-base64-out /tmp/lambda-out.json >/dev/null || true
  cat /tmp/lambda-out.json 2>/dev/null || true; echo
fi

cat <<EOF

\033[1;32m================ DEPLOYMENT COMPLETE ================\033[0m
ALB DNS:        ${ALB_DNS}
URL:            http://${ALB_DNS}/
Hangfire:       http://${ALB_DNS}/hangfire
API:            http://${ALB_DNS}/api
Top games JSON: ${TOP_GAMES_URL:-<not configured>}
Sentiment:      POST a review, then GET /api/Review/{id}/sentiment
\033[1;33mNote:\033[0m DNS for a new ALB can take 30–90 seconds to propagate.
EOF
