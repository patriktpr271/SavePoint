using System.Globalization;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using SavePoint.ReviewsService.Models;

namespace SavePoint.ReviewsService.Services
{
    public class ReviewSentimentService : IReviewSentimentService
    {
        private readonly IAmazonSQS _sqs;
        private readonly IAmazonDynamoDB _dynamo;
        private readonly ILogger<ReviewSentimentService> _logger;
        private readonly string? _queueUrl;
        private readonly string? _tableName;

        public ReviewSentimentService(
            IAmazonSQS sqs,
            IAmazonDynamoDB dynamo,
            IConfiguration config,
            ILogger<ReviewSentimentService> logger)
        {
            _sqs = sqs;
            _dynamo = dynamo;
            _logger = logger;
            _queueUrl = config["AWS:ReviewEventsQueueUrl"];
            _tableName = config["AWS:ReviewSentimentTable"];
        }

        public async Task PublishAsync(Guid reviewId, string content, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_queueUrl))
            {
                _logger.LogDebug("Sentiment queue URL not configured; skipping publish for review {ReviewId}", reviewId);
                return;
            }

            try
            {
                var body = JsonSerializer.Serialize(new { reviewId, content });
                await _sqs.SendMessageAsync(new SendMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MessageBody = body,
                }, ct);
            }
            catch (Exception ex)
            {
                // Sentiment is best-effort. Never fail the review create on this.
                _logger.LogWarning(ex, "Failed to publish review {ReviewId} to sentiment queue", reviewId);
            }
        }

        public async Task<ReviewSentimentDto?> GetAsync(Guid reviewId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_tableName))
            {
                return null;
            }

            try
            {
                var resp = await _dynamo.GetItemAsync(new GetItemRequest
                {
                    TableName = _tableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        ["reviewId"] = new AttributeValue { S = reviewId.ToString() },
                    },
                    ConsistentRead = false,
                }, ct);

                if (!resp.IsItemSet || resp.Item.Count == 0)
                {
                    return null;
                }

                return new ReviewSentimentDto
                {
                    ReviewId   = reviewId,
                    Sentiment  = GetString(resp.Item, "sentiment"),
                    Positive   = GetDouble(resp.Item, "positive"),
                    Negative   = GetDouble(resp.Item, "negative"),
                    Neutral    = GetDouble(resp.Item, "neutral"),
                    Mixed      = GetDouble(resp.Item, "mixed"),
                    AnalyzedAt = DateTime.TryParse(GetString(resp.Item, "analyzedAt"), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt) ? dt : default,
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read sentiment for review {ReviewId}", reviewId);
                return null;
            }
        }

        private static string GetString(IDictionary<string, AttributeValue> item, string key) =>
            item.TryGetValue(key, out var v) ? v.S ?? string.Empty : string.Empty;

        private static double GetDouble(IDictionary<string, AttributeValue> item, string key) =>
            item.TryGetValue(key, out var v) && double.TryParse(v.N, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : 0.0;
    }
}
