using SavePoint.Entities.Games;

namespace SavePoint.Entities.Lists
{
	public class UserListItem : Common.BaseEntity
	{
		public Guid UserListId { get; set; }
		public UserList UserList { get; set; }
		public Guid GameId { get; set; }
		public Game Game { get; set; }
	}
}
