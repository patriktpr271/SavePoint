using SavePoint.Common.Dtos.Games;

namespace SavePoint.Common.Dtos.Lists
{
	public class UserListDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string UserId { get; set; } = string.Empty;
		public string UserDisplayName { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public bool IsPublic { get; set; }
		public bool IsDefault { get; set; }
		public string? DefaultListType { get; set; }
		public int VoteScore { get; set; }
		public int UpvoteCount { get; set; }
		public int DownvoteCount { get; set; }
		public int ItemCount { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		
		// Current user's vote on this list (null if no vote)
		public bool? CurrentUserVote { get; set; }
		
		// Include games if requested
		public List<GameCardDto>? Games { get; set; }
	}
}