using SavePoint.Entities.Games;
using SavePoint.Entities.Users;

namespace SavePoint.Entities.Reviews
{
	public class Review : Common.BaseEntity
	{
		public string UserId { get; set; }
		public ApplicationUser User { get; set; }

		public Guid GameId { get; set; }
		public Game Game { get; set; }
		public int Rating { get; set; } 
		public string Content { get; set; } = string.Empty;
	}
}
