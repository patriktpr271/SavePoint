resource "aws_eks_cluster" "main" {
  name     = var.cluster_name
  role_arn = var.lab_role_arn
  version  = var.kubernetes_version

  vpc_config {
    subnet_ids              = var.subnet_ids
    endpoint_public_access  = true
    endpoint_private_access = true
  }

  # Learner Lab note: the user who runs `terraform apply` (the lab principal)
  # automatically becomes a cluster admin because of the bootstrap below.
  access_config {
    authentication_mode                         = "API_AND_CONFIG_MAP"
    bootstrap_cluster_creator_admin_permissions = true
  }
}

resource "aws_eks_node_group" "main" {
  cluster_name    = aws_eks_cluster.main.name
  node_group_name = "${var.cluster_name}-ng"
  node_role_arn   = var.lab_role_arn
  subnet_ids      = var.node_subnet_ids
  instance_types  = [var.node_instance_type]
  ami_type        = "AL2_x86_64"
  disk_size       = 30

  scaling_config {
    desired_size = var.desired_size
    min_size     = var.min_size
    max_size     = var.max_size
  }

  update_config {
    max_unavailable = 1
  }

  # Roll node group automatically if AMI or version updates
  lifecycle {
    ignore_changes = [scaling_config[0].desired_size]
  }

  depends_on = [aws_eks_cluster.main]
}

# vpc-cni, coredns, kube-proxy are installed by default. We don't add the EBS
# CSI addon here because the Learner Lab IAM trust policy generally cannot be
# modified; we install the EBS CSI driver via Helm from the root module instead.
