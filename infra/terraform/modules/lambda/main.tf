############################################################
# Shared
############################################################

data "aws_caller_identity" "current" {}

locals {
  name_prefix = var.project_name
}

############################################################
# 1) review-sentiment Lambda
############################################################

resource "aws_sqs_queue" "review_events" {
  name                       = "${local.name_prefix}-review-events"
  visibility_timeout_seconds = 60
  message_retention_seconds  = 345600 # 4 days
}

resource "aws_dynamodb_table" "review_sentiment" {
  name         = "${local.name_prefix}-review-sentiment"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "reviewId"

  attribute {
    name = "reviewId"
    type = "S"
  }
}

data "archive_file" "review_sentiment_zip" {
  type        = "zip"
  source_dir  = var.review_sentiment_source_dir
  output_path = "${path.module}/build/review-sentiment.zip"
}

resource "aws_lambda_function" "review_sentiment" {
  function_name    = "${local.name_prefix}-review-sentiment"
  role             = var.lab_role_arn
  handler          = "handler.lambda_handler"
  runtime          = "python3.12"
  timeout          = 30
  memory_size      = 256
  filename         = data.archive_file.review_sentiment_zip.output_path
  source_code_hash = data.archive_file.review_sentiment_zip.output_base64sha256

  environment {
    variables = {
      SENTIMENT_TABLE = aws_dynamodb_table.review_sentiment.name
      COMPREHEND_LANG = "en"
    }
  }
}

resource "aws_lambda_event_source_mapping" "review_sentiment_sqs" {
  event_source_arn                   = aws_sqs_queue.review_events.arn
  function_name                      = aws_lambda_function.review_sentiment.arn
  batch_size                         = 5
  maximum_batching_window_in_seconds = 5
  function_response_types            = ["ReportBatchItemFailures"]
}

############################################################
# 2) top-games-snapshot Lambda
############################################################

resource "aws_s3_bucket" "top_games" {
  bucket_prefix = "${local.name_prefix}-top-games-"
  force_destroy = true
}

# Allow this bucket to host public objects. Public access is restricted to
# the single snapshot object via the bucket policy below.
resource "aws_s3_bucket_public_access_block" "top_games" {
  bucket                  = aws_s3_bucket.top_games.id
  block_public_acls       = true
  ignore_public_acls      = true
  block_public_policy     = false
  restrict_public_buckets = false
}

resource "aws_s3_bucket_cors_configuration" "top_games" {
  bucket = aws_s3_bucket.top_games.id

  cors_rule {
    allowed_methods = ["GET", "HEAD"]
    allowed_origins = ["*"]
    allowed_headers = ["*"]
    max_age_seconds = 300
  }
}

resource "aws_s3_bucket_policy" "top_games_public_read" {
  bucket = aws_s3_bucket.top_games.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid       = "PublicReadSnapshot"
        Effect    = "Allow"
        Principal = "*"
        Action    = ["s3:GetObject"]
        Resource  = ["${aws_s3_bucket.top_games.arn}/top-games/*"]
      }
    ]
  })

  depends_on = [aws_s3_bucket_public_access_block.top_games]
}

data "archive_file" "top_games_zip" {
  type        = "zip"
  source_dir  = var.top_games_source_dir
  output_path = "${path.module}/build/top-games-snapshot.zip"
}

resource "aws_lambda_function" "top_games_snapshot" {
  function_name    = "${local.name_prefix}-top-games-snapshot"
  role             = var.lab_role_arn
  handler          = "handler.lambda_handler"
  runtime          = "python3.12"
  timeout          = 30
  memory_size      = 256
  filename         = data.archive_file.top_games_zip.output_path
  source_code_hash = data.archive_file.top_games_zip.output_base64sha256

  environment {
    variables = {
      # APP_BASE_URL is patched by the deploy script once the ALB DNS is known.
      APP_BASE_URL    = var.app_base_url
      SNAPSHOT_BUCKET = aws_s3_bucket.top_games.bucket
      SNAPSHOT_KEY    = "top-games/latest.json"
      POPULARITY_TYPE = "1"
      PAGE_SIZE       = "10"
    }
  }

  lifecycle {
    # Don't fight the deploy script when it patches APP_BASE_URL out-of-band.
    ignore_changes = [environment[0].variables["APP_BASE_URL"]]
  }
}

resource "aws_cloudwatch_event_rule" "top_games_schedule" {
  name                = "${local.name_prefix}-top-games-snapshot"
  description         = "Periodically snapshots top games to S3"
  schedule_expression = var.snapshot_schedule
}

resource "aws_cloudwatch_event_target" "top_games_target" {
  rule      = aws_cloudwatch_event_rule.top_games_schedule.name
  target_id = "lambda"
  arn       = aws_lambda_function.top_games_snapshot.arn
}

resource "aws_lambda_permission" "allow_eventbridge_to_invoke_top_games" {
  statement_id  = "AllowExecutionFromEventBridge"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.top_games_snapshot.function_name
  principal     = "events.amazonaws.com"
  source_arn    = aws_cloudwatch_event_rule.top_games_schedule.arn
}
