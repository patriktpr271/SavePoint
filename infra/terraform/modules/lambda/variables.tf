variable "project_name" {
  type = string
}

variable "lab_role_arn" {
  description = "LabRole ARN — used as the execution role for both Lambdas"
  type        = string
}

variable "aws_region" {
  type = string
}

variable "review_sentiment_source_dir" {
  description = "Local path to the review-sentiment Lambda source"
  type        = string
}

variable "top_games_source_dir" {
  description = "Local path to the top-games-snapshot Lambda source"
  type        = string
}

variable "snapshot_schedule" {
  description = "EventBridge cron/rate for the top-games snapshot. Use rate(5 minutes) for demo speed, or cron(0 6 * * ? *) for daily 06:00 UTC."
  type        = string
  default     = "rate(15 minutes)"
}

variable "app_base_url" {
  description = "Public URL where the SavePoint app is reachable (the ALB DNS). Set after first deploy via terraform apply -var. Defaults to an empty string; the deploy script patches the Lambda env var if this is empty."
  type        = string
  default     = ""
}
