using SavePoint.Entities.Common;
using SavePoint.Entities.Lists;

namespace SavePoint.DAL.Repositories.Interfaces
{
	public interface IUserListRepository
	{
		Task<UserList?> GetByIdAsync(Guid id);
		Task<UserList?> GetByIdWithDetailsAsync(Guid id);
		Task<UserList?> GetByIdWithGamesAsync(Guid id);
		Task<List<UserList>> GetUserListsAsync(string userId);
		Task<PagedResult<UserList>> GetPublicListsAsync(int pageNumber = 1, int pageSize = 20, string? search = null, string? sortBy = null);
		Task<UserList?> GetUserDefaultListAsync(string userId, string defaultListType);
		Task<UserList> CreateAsync(UserList userList);
		Task<UserList> UpdateAsync(UserList userList);
		Task DeleteAsync(Guid id);
		Task<bool> IsGameInListAsync(Guid listId, Guid gameId);
		Task<UserListItem?> GetListItemAsync(Guid listId, Guid gameId);
		Task<UserListItem> AddGameToListAsync(Guid listId, Guid gameId);
		Task RemoveGameFromListAsync(Guid listId, Guid gameId);
		Task<UserListVote?> GetUserVoteAsync(Guid listId, string userId);
		Task<UserListVote> AddOrUpdateVoteAsync(Guid listId, string userId, bool isUpvote);
		Task RemoveVoteAsync(Guid listId, string userId);
		Task UpdateVoteCountsAsync(Guid listId);
	}
}