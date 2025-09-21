using SavePoint.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.Entities.Lists
{
	public class UserList : Common.BaseEntity
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string UserId { get; set; }
		public ApplicationUser User { get; set; }

		// Navigation properties
		public ICollection<UserListItem> UserListItems { get; set; } = new List<UserListItem>();
	}
}
