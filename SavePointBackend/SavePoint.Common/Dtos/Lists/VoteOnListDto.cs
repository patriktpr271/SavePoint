using System.ComponentModel.DataAnnotations;

namespace SavePoint.Common.Dtos.Lists
{
	public class VoteOnListDto
	{
		[Required]
		public bool IsUpvote { get; set; }
	}
}