using System.ComponentModel.DataAnnotations;

namespace SavePoint.Common.Dtos.Reviews
{
    public class CreateReviewDto
    {
        [Required]
        public Guid GameId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Review content must be between 10 and 2000 characters")]
        public string Content { get; set; } = string.Empty;
    }
}