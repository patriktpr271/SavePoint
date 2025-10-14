using SavePoint.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.Entities.Lists
{
	public class UserList : Common.BaseEntity
	{
		[Required]
		[StringLength(100)]
		public string Name { get; set; } = string.Empty;
		
		[StringLength(500)]
		public string Description { get; set; } = string.Empty;
		
		[Required]
		public string UserId { get; set; } = string.Empty;
		public ApplicationUser User { get; set; } = null!;

		/// <summary>
		/// Indicates if this list is visible to other users
		/// </summary>
		public bool IsPublic { get; set; } = true;

		/// <summary>
		/// Indicates if this is a default system list (Want to Play, Finished)
		/// </summary>
		public bool IsDefault { get; set; } = false;

		/// <summary>
		/// The type of default list (if applicable): "WantToPlay", "Finished", or null for custom lists
		/// </summary>
		[StringLength(50)]
		public string? DefaultListType { get; set; }

		/// <summary>
		/// Total upvotes minus downvotes
		/// </summary>
		public int VoteScore { get; set; } = 0;

		/// <summary>
		/// Total number of upvotes
		/// </summary>
		public int UpvoteCount { get; set; } = 0;

		/// <summary>
		/// Total number of downvotes
		/// </summary>
		public int DownvoteCount { get; set; } = 0;

		// Navigation properties
		public ICollection<UserListItem> UserListItems { get; set; } = new List<UserListItem>();
		public ICollection<UserListVote> Votes { get; set; } = new List<UserListVote>();
	}
}
