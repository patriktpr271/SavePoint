#!/usr/bin/env bash
# scripts/pause-aws.sh
#
# Scales the EKS node group to 0 to stop paying for EC2 nodes.
# Everything else (EKS control plane, ALB, NAT, EBS, ECR, Lambdas) stays up.
# Data on the SQL Server EBS volume is preserved.
#
# Resume with: bash scripts/resume-aws.sh
# Run from the repository root.

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TF_DIR="${REPO_ROOT}/infra/terraform"

log()  { printf "\n\033[1;34m==>\033[0m %s\n" "$*"; }
ok()   { printf "\033[1;32m[OK]\033[0m %s\n" "$*"; }
fail() { printf "\033[1;31m[FAIL]\033[0m %s\n" "$*" >&2; exit 1; }

# ----- Region / credentials check -----------------------------------------
: "${AWS_REGION:=us-east-1}"
export AWS_REGION

if ! aws sts get-caller-identity >/dev/null 2>&1; then
  fail "AWS credentials missing or expired. Refresh them from the Learner Lab portal first."
fi

# ----- Cluster + node group names come from Terraform outputs --------------
log "Reading cluster name from Terraform"
CLUSTER_NAME="$(terraform -chdir="${TF_DIR}" output -raw cluster_name)"
NODEGROUP_NAME="${CLUSTER_NAME}-ng"
ok "Cluster: ${CLUSTER_NAME}, node group: ${NODEGROUP_NAME}"

# ----- Show current scale before changing it -------------------------------
log "Current node-group scaling config:"
aws eks describe-nodegroup --region "${AWS_REGION}" \
  --cluster-name "${CLUSTER_NAME}" --nodegroup-name "${NODEGROUP_NAME}" \
  --query 'nodegroup.scalingConfig' --output table

# ----- Scale to 0 ----------------------------------------------------------
log "Scaling node group to 0 (this takes 2–4 min while EC2 instances drain)"
aws eks update-nodegroup-config --region "${AWS_REGION}" \
  --cluster-name "${CLUSTER_NAME}" \
  --nodegroup-name "${NODEGROUP_NAME}" \
  --scaling-config minSize=0,maxSize=3,desiredSize=0 \
  --output table

ok "Pause request submitted."

cat <<EOF

\033[1;33mWhat happens next:\033[0m
  - EC2 instances will be terminated by the EKS Auto Scaling Group
    (you'll stop being charged for them when AWS marks them Terminated).
  - All pods will go into Pending state. SQL Server data on the EBS volume
    is preserved.
  - You will continue paying for EKS control plane (~\$0.10/hr),
    NAT Gateway (~\$0.045/hr), ALB (~\$0.0225/hr), and EBS storage.
    Roughly \$4/day instead of \$6/day while paused.

\033[1;32mResume with:\033[0m
  bash scripts/resume-aws.sh
EOF
