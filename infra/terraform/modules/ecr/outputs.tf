output "repository_urls" {
  description = "Map of repository name -> repository URL"
  value       = { for k, v in aws_ecr_repository.this : k => v.repository_url }
}

output "repository_names" {
  value = [for v in aws_ecr_repository.this : v.name]
}
