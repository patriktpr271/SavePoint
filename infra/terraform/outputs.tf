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

output "review_events_queue_url" {
  description = "SQS queue URL the reviews service publishes to"
  value       = module.lambda.review_events_queue_url
}

output "review_sentiment_table" {
  description = "DynamoDB table where the Lambda writes sentiment results"
  value       = module.lambda.review_sentiment_table
}

output "top_games_object_url" {
  description = "Public URL of the top-games JSON snapshot (fetched by the frontend)"
  value       = module.lambda.top_games_object_url
}

output "top_games_lambda_name" {
  description = "Lambda function name (used by the deploy script to patch APP_BASE_URL)"
  value       = module.lambda.top_games_lambda_name
}
