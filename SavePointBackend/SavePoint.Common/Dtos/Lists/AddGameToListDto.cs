using System.ComponentModel.DataAnnotations;

namespace SavePoint.Common.Dtos.Lists
{
	public class AddGameToListDto
	{
		[Required]
		public Guid GameId { get; set; }
	}
}