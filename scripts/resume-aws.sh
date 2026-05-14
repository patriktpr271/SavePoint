#!/usr/bin/env bash
# scripts/resume-aws.sh
#
# Scales the EKS node group back up after a pause-aws.sh run.
# Re-points kubectl at the cluster (in case you're in a new shell session),
# waits for nodes to be Ready, then waits for all five app pods to roll out.
#
# Run from the repository root.

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TF_DIR="${REPO_ROOT}/infra/terraform"

log()  { printf "\n\033[1;34m==>\033[0m %s\n" "$*"; }
ok()   { printf "\033[1;32m[OK]\033[0m %s\n" "$*"; }
fail() { printf "\033[1;31m[FAIL]\033[0m %s\n" "$*" >&2; exit 1; }

# Default scale target — change DESIRED if you ever want more/fewer nodes.
DESIRED="${DESIRED:-2}"
MIN="${MIN:-1}"
MAX="${MAX:-3}"

# ----- Region / credentials check ------------------------------------------
: "${AWS_REGION:=us-east-1}"
export AWS_REGION

if ! aws sts get-caller-identity >/dev/null 2>&1; then
  cat >&2 <<'EOF'
[FAIL] AWS credentials missing or expired.

Refresh them from the Learner Lab portal:
  1. Start the Lab, click "AWS Details" -> "AWS CLI" -> "Show".
  2. Paste the three export lines in this terminal:
       export AWS_ACCESS_KEY_ID="ASIA..."
       export AWS_SECRET_ACCESS_KEY="..."
       export AWS_SESSION_TOKEN="..."
  3. Re-run:  bash scripts/resume-aws.sh
EOF
  exit 1
fi

# ----- Read cluster + node group names from Terraform ---------------------
log "Reading cluster name from Terraform"
CLUSTER_NAME="$(terraform -chdir="${TF_DIR}" output -raw cluster_name)"
NODEGROUP_NAME="${CLUSTER_NAME}-ng"
ok "Cluster: ${CLUSTER_NAME}, node group: ${NODEGROUP_NAME}"

# ----- Scale back up -------------------------------------------------------
log "Scaling node group back to desired=${DESIRED} (min=${MIN}, max=${MAX})"
aws eks update-nodegroup-config --region "${AWS_REGION}" \
  --cluster-name "${CLUSTER_NAME}" \
  --nodegroup-name "${NODEGROUP_NAME}" \
  --scaling-config "minSize=${MIN},maxSize=${MAX},desiredSize=${DESIRED}" \
  --output table

# ----- Re-point kubectl at the cluster (safe in any shell session) ---------
log "Refreshing kubeconfig"
aws eks update-kubeconfig --region "${AWS_REGION}" --name "${CLUSTER_NAME}" >/dev/null

# ----- Wait for nodes to register and become Ready ------------------------
log "Waiting for ${DESIRED} node(s) to become Ready (up to 5 min)"
for i in $(seq 1 60); do
  READY=$(kubectl get nodes --no-headers 2>/dev/null | awk '$2=="Ready"{c++} END{print c+0}')
  if [[ "${READY}" -ge "${DESIRED}" ]]; then
    ok "${READY} node(s) Ready"
    break
  fi
  printf "."
  sleep 5
done
echo

# ----- Wait for the app to roll out ---------------------------------------
log "Waiting for application pods to roll out"
kubectl -n savepoint rollout status statefulset/savepoint-db      --timeout=10m
kubectl -n savepoint rollout status deployment/savepoint-backend  --timeout=10m
kubectl -n savepoint rollout status deployment/savepoint-reviews  --timeout=10m
kubectl -n savepoint rollout status deployment/savepoint-lookup   --timeout=10m
kubectl -n savepoint rollout status deployment/savepoint-frontend --timeout=10m

# ----- Print URL -----------------------------------------------------------
ALB_DNS="$(kubectl -n savepoint get ingress savepoint-ingress \
  -o jsonpath='{.status.loadBalancer.ingress[0].hostname}' 2>/dev/null || true)"

cat <<EOF

\033[1;32m================ RESUMED ================\033[0m
ALB DNS:  ${ALB_DNS:-<not yet visible — kubectl get ingress -n savepoint>}
URL:      http://${ALB_DNS}/
Hangfire: http://${ALB_DNS}/hangfire

Your SQL data, IGDB lookup tables, reviews, and users are intact — the
EBS volume re-attached to the new SQL Server pod.
EOF
