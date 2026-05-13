using SavePoint.ReviewsService.Models;

namespace SavePoint.ReviewsService.Services
{
    public interface IReviewSentimentService
    {
        /// <summary>
        /// Best-effort: drops the review id + content onto an SQS queue so the
        /// review-sentiment Lambda can analyse it. Failures are logged and
        /// swallowed — sentiment analysis is non-critical.
        /// </summary>
        Task PublishAsync(Guid reviewId, string content, CancellationToken ct = default);

        /// <summary>
        /// Reads the latest analysis result from DynamoDB. Returns null when the
        /// Lambda hasn't processed this review yet.
        /// </summary>
        Task<ReviewSentimentDto?> GetAsync(Guid reviewId, CancellationToken ct = default);
    }
}
