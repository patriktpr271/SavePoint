output "aws_region" {
  description = "AWS region used"
  value       = var.aws_region
}

output "aws_account_id" {
  description = "AWS account ID detected from current credentials"
  value       = data.aws_caller_identity.current.account_id
}

output "cluster_name" {
  description = "EKS cluster name (use with: aws eks update-kubeconfig)"
  value       = module.eks.cluster_name
}

output "cluster_endpoint" {
  description = "EKS cluster API endpoint"
  value       = module.eks.cluster_endpoint
}

output "vpc_id" {
  description = "VPC ID"
  value       = module.networking.vpc_id
}

output "public_subnet_ids" {
  description = "Public subnet IDs (used by ALB)"
  value       = module.networking.public_subnet_ids
}

output "private_subnet_ids" {
  description = "Private subnet IDs (used by EKS nodes)"
  value       = module.networking.private_subnet_ids
}

output "ecr_repository_urls" {
  description = "Map of ECR repository name -> URL"
  value       = module.ecr.repository_urls
}

output "kubeconfig_command" {
  description = "Run this command to point kubectl at the new cluster"
  value       = "aws eks update-kubeconfig --region ${var.aws_region} --name ${module.eks.cluster_name}"
}
