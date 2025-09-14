using Microsoft.AspNetCore.Identity;

namespace SavePoint.Entities.Users
{
	public class ApplicationUser : IdentityUser
	{
		public string DisplayName { get; set; } = string.Empty;
		public string Bio { get; set; } = string.Empty;

		// Navigation properties
		public ICollection<Reviews.Review> Reviews { get; set; } = new List<Reviews.Review>();
		public ICollection<Games.Game> Games { get; set; } = new List<Games.Game>();
	}
}
