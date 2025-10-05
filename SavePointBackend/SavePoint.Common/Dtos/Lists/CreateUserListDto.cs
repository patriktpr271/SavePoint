using System.ComponentModel.DataAnnotations;

namespace SavePoint.Common.Dtos.Lists
{
	public class CreateUserListDto
	{
		[Required]
		[StringLength(100, MinimumLength = 1)]
		public string Name { get; set; } = string.Empty;

		[StringLength(500)]
		public string Description { get; set; } = string.Empty;

		public bool IsPublic { get; set; } = true;
	}
}