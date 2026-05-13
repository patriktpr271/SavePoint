"""
review-sentiment Lambda.

Triggered by an SQS queue. Each message body is a JSON document:
    { "reviewId": "<uuid>", "content": "<review text>" }

For each message we call AWS Comprehend's DetectSentiment, then write the
result to a DynamoDB table keyed by reviewId. The C# ReviewsService reads
that table when the frontend asks for the sentiment of a review.

Environment variables (set by Terraform):
    SENTIMENT_TABLE   DynamoDB table name
    COMPREHEND_LANG   Language code passed to Comprehend (default: "en")
"""
import json
import logging
import os
from datetime import datetime, timezone
from decimal import Decimal

import boto3

logger = logging.getLogger()
logger.setLevel(logging.INFO)

TABLE_NAME = os.environ["SENTIMENT_TABLE"]
LANG = os.environ.get("COMPREHEND_LANG", "en")

comprehend = boto3.client("comprehend")
table = boto3.resource("dynamodb").Table(TABLE_NAME)


def _truncate(text: str, max_bytes: int = 4500) -> str:
    """Comprehend DetectSentiment caps input at 5000 UTF-8 bytes."""
    encoded = text.encode("utf-8")
    if len(encoded) <= max_bytes:
        return text
    return encoded[:max_bytes].decode("utf-8", errors="ignore")


def _process_one(review_id: str, content: str) -> None:
    text = _truncate(content or "")
    if not text.strip():
        logger.info("review %s has empty content; skipping", review_id)
        return

    resp = comprehend.detect_sentiment(Text=text, LanguageCode=LANG)
    scores = resp["SentimentScore"]

    item = {
        "reviewId": review_id,
        "sentiment": resp["Sentiment"],            # POSITIVE | NEGATIVE | NEUTRAL | MIXED
        "positive": Decimal(str(scores["Positive"])),
        "negative": Decimal(str(scores["Negative"])),
        "neutral":  Decimal(str(scores["Neutral"])),
        "mixed":    Decimal(str(scores["Mixed"])),
        "analyzedAt": datetime.now(timezone.utc).isoformat(timespec="seconds"),
    }
    table.put_item(Item=item)
    logger.info("review %s -> %s", review_id, resp["Sentiment"])


def lambda_handler(event, context):
    failures = []

    for record in event.get("Records", []):
        message_id = record.get("messageId", "?")
        try:
            body = json.loads(record["body"])
            review_id = body["reviewId"]
            content = body.get("content", "")
            _process_one(review_id, content)
        except Exception:
            # Reporting itemFailures lets SQS retry only the failed messages
            # instead of the whole batch.
            logger.exception("failed to process message %s", message_id)
            failures.append({"itemIdentifier": message_id})

    return {"batchItemFailures": failures}
