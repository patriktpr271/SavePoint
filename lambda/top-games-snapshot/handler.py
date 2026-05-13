"""
top-games-snapshot Lambda.

Triggered by EventBridge on a schedule. Calls the SavePoint backend's
"popular games" endpoint over HTTP (via the public ALB), then writes the
JSON to a public-read S3 object. The frontend fetches the JSON directly
from S3 — no backend hop on page load.

Environment variables (set by Terraform):
    APP_BASE_URL     Public URL of the app, e.g. "http://k8s-...elb.amazonaws.com"
                     (must NOT include a trailing slash)
    SNAPSHOT_BUCKET  S3 bucket name
    SNAPSHOT_KEY     Object key, e.g. "top-games/latest.json"
    POPULARITY_TYPE  Integer popularity type id (1, 2, or 5) — defaults to 1
    PAGE_SIZE        Number of games to include (default: 10)
"""
import json
import logging
import os
import urllib.request
from datetime import datetime, timezone

import boto3

logger = logging.getLogger()
logger.setLevel(logging.INFO)

APP_BASE_URL = os.environ["APP_BASE_URL"].rstrip("/")
BUCKET = os.environ["SNAPSHOT_BUCKET"]
KEY = os.environ.get("SNAPSHOT_KEY", "top-games/latest.json")
POPULARITY_TYPE = int(os.environ.get("POPULARITY_TYPE", "1"))
PAGE_SIZE = int(os.environ.get("PAGE_SIZE", "10"))

s3 = boto3.client("s3")


def lambda_handler(event, context):
    url = f"{APP_BASE_URL}/api/game/popular/{POPULARITY_TYPE}?pageNumber=1&pageSize={PAGE_SIZE}"
    logger.info("GET %s", url)

    req = urllib.request.Request(url, headers={"Accept": "application/json"})
    with urllib.request.urlopen(req, timeout=15) as resp:
        payload = json.loads(resp.read().decode("utf-8"))

    snapshot = {
        "generatedAt": datetime.now(timezone.utc).isoformat(timespec="seconds"),
        "popularityType": POPULARITY_TYPE,
        "source": url,
        "data": payload,
    }

    body = json.dumps(snapshot, default=str).encode("utf-8")
    s3.put_object(
        Bucket=BUCKET,
        Key=KEY,
        Body=body,
        ContentType="application/json",
        CacheControl="public, max-age=60",
    )

    logger.info("wrote %d bytes to s3://%s/%s", len(body), BUCKET, KEY)
    return {"ok": True, "bytes": len(body), "items": len(payload.get("items", []))}
