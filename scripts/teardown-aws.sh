#!/usr/bin/env bash
# scripts/teardown-aws.sh
#
# Tear down everything created by scripts/deploy-aws.sh.
# Run from the repository root:  bash scripts/teardown-aws.sh

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TF_DIR="${REPO_ROOT}/infra/terraform"
K8S_DIR="${REPO_ROOT}/infra/k8s-aws"

log()  { printf "\n\033[1;34m==>\033[0m %s\n" "$*"; }
ok()   { printf "\033[1;32m[OK]\033[0m %s\n" "$*"; }
warn() { printf "\033[1;33m[WARN]\033[0m %s\n" "$*"; }

cat <<'EOF'
==============================================================
                       !!  WARNING  !!
This will DELETE every AWS resource created for SavePoint:
  - All pods, services, ingress, PVCs, the MSSQL volume
  - The Application Load Balancer (DNS will stop resolving)
  - The EKS cluster, node group, and worker EC2 instances
  - The VPC, subnets, NAT gateway, IAM/EIP attachments
  - The four ECR repositories AND all images pushed into them

Data in the SQL Server PVC will be PERMANENTLY LOST.
This action CANNOT be undone.
==============================================================
EOF

read -r -p 'Type "destroy" to confirm: ' CONFIRM
if [[ "${CONFIRM}" != "destroy" ]]; then
  warn "Confirmation did not match. Aborting."
  exit 1
fi

# ----- 1. Delete k8s resources first (so the ALB is released) -------------
if command -v kubectl >/dev/null 2>&1 && kubectl cluster-info >/dev/null 2>&1; then
  log "Deleting Kubernetes resources"
  kubectl delete -k "${K8S_DIR}" --ignore-not-found=true --wait=true || true
  ok "kubectl delete done"
else
  warn "kubectl is not pointed at a reachable cluster; skipping kubectl delete"
fi

# Give the ALB controller a moment to delete the ALB and its target groups
# (Terraform will fail to delete the VPC if the ALB still exists).
log "Waiting 60s for the ALB controller to clean up the load balancer"
sleep 60

# ----- 2. Terraform destroy -----------------------------------------------
log "Running terraform destroy"
cd "${TF_DIR}"
terraform destroy -auto-approve

ok "Teardown complete. The S3 backend bucket and its state file are NOT deleted (delete them manually if you don't need them)."
