using SavePoint.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace SavePoint.Entities.Lists
{
	public class UserListVote : Common.BaseEntity
	{
		[Required]
		public Guid UserListId { get; set; }
		public UserList UserList { get; set; } = null!;

		[Required]
		public string UserId { get; set; } = string.Empty;
		public ApplicationUser User { get; set; } = null!;

		/// <summary>
		/// True for upvote, False for downvote
		/// </summary>
		public bool IsUpvote { get; set; }
	}
}