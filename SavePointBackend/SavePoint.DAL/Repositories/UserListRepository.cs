using Microsoft.EntityFrameworkCore;
using SavePoint.DAL.Contexts;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Common;
using SavePoint.Entities.Lists;

namespace SavePoint.DAL.Repositories
{
	public class UserListRepository : IUserListRepository
	{
		private readonly ApplicationDbContext _context;

		public UserListRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<UserList?> GetByIdAsync(Guid id)
		{
			return await _context.UserLists
				.Include(ul => ul.User)
				.FirstOrDefaultAsync(ul => ul.Id == id);
		}

		public async Task<UserList?> GetByIdWithDetailsAsync(Guid id)
		{
			return await _context.UserLists
				.Include(ul => ul.User)
				.Include(ul => ul.UserListItems)
				.Include(ul => ul.Votes)
				.FirstOrDefaultAsync(ul => ul.Id == id);
		}

		public async Task<UserList?> GetByIdWithGamesAsync(Guid id)
		{
			return await _context.UserLists
				.Include(ul => ul.User)
				.Include(ul => ul.UserListItems)
					.ThenInclude(uli => uli.Game)
						.ThenInclude(g => g.GameGenres)
							.ThenInclude(gg => gg.Genre)
				.Include(ul => ul.UserListItems)
					.ThenInclude(uli => uli.Game)
						.ThenInclude(g => g.GamePlatforms)
							.ThenInclude(gp => gp.Platform)
				.Include(ul => ul.UserListItems)
					.ThenInclude(uli => uli.Game)
						.ThenInclude(g => g.GameCompanies)
							.ThenInclude(gc => gc.Company)
				.Include(ul => ul.Votes)
				.FirstOrDefaultAsync(ul => ul.Id == id);
		}

		public async Task<List<UserList>> GetUserListsAsync(string userId)
		{
			return await _context.UserLists
				.Include(ul => ul.User)
				.Include(ul => ul.UserListItems)
				.Where(ul => ul.UserId == userId)
				.OrderBy(ul => ul.IsDefault ? 0 : 1) // Default lists first
				.ThenBy(ul => ul.CreatedAt)
				.ToListAsync();
		}

		public async Task<PagedResult<UserList>> GetPublicListsAsync(int pageNumber = 1, int pageSize = 20, string? search = null, string? sortBy = null)
		{
			var query = _context.UserLists
				.Include(ul => ul.User)
				.Include(ul => ul.UserListItems)
				.Where(ul => ul.IsPublic);

			// Apply search filter
			if (!string.IsNullOrWhiteSpace(search))
			{
				query = query.Where(ul => ul.Name.Contains(search) || ul.Description.Contains(search));
			}

			// Apply sorting
			query = sortBy?.ToLower() switch
			{
				"name" => query.OrderBy(ul => ul.Name),
				"votes" => query.OrderByDescending(ul => ul.VoteScore),
				"items" => query.OrderByDescending(ul => ul.UserListItems.Count),
				"created" => query.OrderByDescending(ul => ul.CreatedAt),
				_ => query.OrderByDescending(ul => ul.VoteScore) // Default sort by votes
			};

			var totalCount = await query.CountAsync();
			var items = await query
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return new PagedResult<UserList>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = pageNumber,
				PageSize = pageSize
			};
		}

		public async Task<UserList?> GetUserDefaultListAsync(string userId, string defaultListType)
		{
			return await _context.UserLists
				.FirstOrDefaultAsync(ul => ul.UserId == userId && ul.DefaultListType == defaultListType);
		}

		public async Task<UserList> CreateAsync(UserList userList)
		{
			userList.CreatedAt = DateTime.UtcNow;
			userList.UpdatedAt = DateTime.UtcNow;
			
			await _context.UserLists.AddAsync(userList);
			await _context.SaveChangesAsync();
			return userList;
		}

		public async Task<UserList> UpdateAsync(UserList userList)
		{
			userList.UpdatedAt = DateTime.UtcNow;
			_context.UserLists.Update(userList);
			await _context.SaveChangesAsync();
			return userList;
		}

		public async Task DeleteAsync(Guid id)
		{
			var userList = await _context.UserLists.FindAsync(id);
			if (userList != null)
			{
				_context.UserLists.Remove(userList);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<bool> IsGameInListAsync(Guid listId, Guid gameId)
		{
			return await _context.UserListItems
				.AnyAsync(uli => uli.UserListId == listId && uli.GameId == gameId);
		}

		public async Task<UserListItem?> GetListItemAsync(Guid listId, Guid gameId)
		{
			return await _context.UserListItems
				.FirstOrDefaultAsync(uli => uli.UserListId == listId && uli.GameId == gameId);
		}

		public async Task<UserListItem> AddGameToListAsync(Guid listId, Guid gameId)
		{
			var listItem = new UserListItem
			{
				Id = Guid.NewGuid(),
				UserListId = listId,
				GameId = gameId,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _context.UserListItems.AddAsync(listItem);
			await _context.SaveChangesAsync();
			return listItem;
		}

		public async Task RemoveGameFromListAsync(Guid listId, Guid gameId)
		{
			var listItem = await GetListItemAsync(listId, gameId);
			if (listItem != null)
			{
				_context.UserListItems.Remove(listItem);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<UserListVote?> GetUserVoteAsync(Guid listId, string userId)
		{
			return await _context.UserListVotes
				.FirstOrDefaultAsync(ulv => ulv.UserListId == listId && ulv.UserId == userId);
		}

		public async Task<UserListVote> AddOrUpdateVoteAsync(Guid listId, string userId, bool isUpvote)
		{
			var existingVote = await GetUserVoteAsync(listId, userId);

			if (existingVote != null)
			{
				existingVote.IsUpvote = isUpvote;
				existingVote.UpdatedAt = DateTime.UtcNow;
				_context.UserListVotes.Update(existingVote);
			}
			else
			{
				existingVote = new UserListVote
				{
					Id = Guid.NewGuid(),
					UserListId = listId,
					UserId = userId,
					IsUpvote = isUpvote,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};
				await _context.UserListVotes.AddAsync(existingVote);
			}

			await _context.SaveChangesAsync();
			await UpdateVoteCountsAsync(listId);
			return existingVote;
		}

		public async Task RemoveVoteAsync(Guid listId, string userId)
		{
			var vote = await GetUserVoteAsync(listId, userId);
			if (vote != null)
			{
				_context.UserListVotes.Remove(vote);
				await _context.SaveChangesAsync();
				await UpdateVoteCountsAsync(listId);
			}
		}

		public async Task UpdateVoteCountsAsync(Guid listId)
		{
			var userList = await _context.UserLists.FindAsync(listId);
			if (userList != null)
			{
				var votes = await _context.UserListVotes
					.Where(ulv => ulv.UserListId == listId)
					.ToListAsync();

				userList.UpvoteCount = votes.Count(v => v.IsUpvote);
				userList.DownvoteCount = votes.Count(v => !v.IsUpvote);
				userList.VoteScore = userList.UpvoteCount - userList.DownvoteCount;
				userList.UpdatedAt = DateTime.UtcNow;

				_context.UserLists.Update(userList);
				await _context.SaveChangesAsync();
			}
		}
	}
}