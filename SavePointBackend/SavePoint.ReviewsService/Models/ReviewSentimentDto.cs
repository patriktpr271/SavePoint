namespace SavePoint.ReviewsService.Models
{
    public class ReviewSentimentDto
    {
        public Guid ReviewId { get; set; }
        public string Sentiment { get; set; } = string.Empty;
        public double Positive { get; set; }
        public double Negative { get; set; }
        public double Neutral { get; set; }
        public double Mixed { get; set; }
        public DateTime AnalyzedAt { get; set; }
    }
}
