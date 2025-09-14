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

		//foreign key for the user who owns the list
		public string UserId { get; set; }
		public ApplicationUser User { get; set; }
	}
}
