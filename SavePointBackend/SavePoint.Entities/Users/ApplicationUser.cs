using Microsoft.AspNetCore.Identity;
using SavePoint.Entities.Lists;
using SavePoint.Entities.Reviews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.Entities.Users
{
	public class ApplicationUser : IdentityUser
	{
		public string DisplayName { get; set; } = string.Empty;
		public string Bio { get; set; } = string.Empty;

		// Navigation properties
		public ICollection<Review> Reviews { get; set; } = new List<Review>();
		public ICollection<UserList> UserLists { get; set; } = new List<UserList>();
	}
}
