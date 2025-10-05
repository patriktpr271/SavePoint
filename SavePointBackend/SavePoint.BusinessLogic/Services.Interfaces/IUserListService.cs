using SavePoint.Common.Dtos.Lists;
using SavePoint.Entities.Common;

namespace SavePoint.BusinessLogic.Services.Interfaces
{
	public interface IUserListService
	{
		// List management
		Task<UserListDto?> GetListByIdAsync(Guid id, string? currentUserId = null);
		Task<UserListDto?> GetListWithGamesAsync(Guid id, string? currentUserId = null);
		Task<List<UserListDto>> GetUserListsAsync(string userId, string? currentUserId = null);
		Task<PagedResult<UserListDto>> GetPublicListsAsync(int pageNumber = 1, int pageSize = 20, string? search = null, string? sortBy = null, string? currentUserId = null);
		Task<UserListDto> CreateListAsync(CreateUserListDto dto, string userId);
		Task<UserListDto?> UpdateListAsync(Guid id, UpdateUserListDto dto, string userId);
		Task<bool> DeleteListAsync(Guid id, string userId);

		// Game management in lists
		Task<bool> AddGameToListAsync(Guid listId, Guid gameId, string userId);
		Task<bool> RemoveGameFromListAsync(Guid listId, Guid gameId, string userId);
		Task<bool> IsGameInListAsync(Guid listId, Guid gameId);

		// Voting
		Task<bool> VoteOnListAsync(Guid listId, bool isUpvote, string userId);
		Task<bool> RemoveVoteAsync(Guid listId, string userId);

		// Default lists management
		Task CreateDefaultListsForUserAsync(string userId);
		Task<UserListDto?> GetUserDefaultListAsync(string userId, string defaultListType, string? currentUserId = null);

		// Quick actions for default lists
		Task<bool> AddGameToWantToPlayAsync(Guid gameId, string userId);
		Task<bool> AddGameToFinishedAsync(Guid gameId, string userId);
		Task<bool> MoveGameToFinished(Guid gameId, string userId);
	}
}