output "review_events_queue_url" {
  value = aws_sqs_queue.review_events.url
}

output "review_events_queue_arn" {
  value = aws_sqs_queue.review_events.arn
}

output "review_sentiment_table" {
  value = aws_dynamodb_table.review_sentiment.name
}

output "review_sentiment_lambda_name" {
  value = aws_lambda_function.review_sentiment.function_name
}

output "top_games_bucket" {
  value = aws_s3_bucket.top_games.bucket
}

output "top_games_object_url" {
  description = "Public URL the frontend should fetch"
  value       = "https://${aws_s3_bucket.top_games.bucket_regional_domain_name}/top-games/latest.json"
}

output "top_games_lambda_name" {
  value = aws_lambda_function.top_games_snapshot.function_name
}
