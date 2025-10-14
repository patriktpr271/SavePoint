using AutoMapper;
using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.Common.Dtos.Lists;
using SavePoint.Common.Exceptions;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Common;
using SavePoint.Entities.Lists;

namespace SavePoint.BusinessLogic.Services
{
	public class UserListService : IUserListService
	{
		private readonly IUserListRepository _userListRepository;
		private readonly IGameRepository _gameRepository;
		private readonly IMapper _mapper;

		public UserListService(IUserListRepository userListRepository, IGameRepository gameRepository, IMapper mapper)
		{
			_userListRepository = userListRepository;
			_gameRepository = gameRepository;
			_mapper = mapper;
		}

		public async Task<UserListDto> GetListByIdAsync(Guid id, string? currentUserId = null)
		{
			var userList = await _userListRepository.GetByIdWithDetailsAsync(id);
			if (userList == null)
				throw new NotFoundException("List", id);

			// Check privacy settings
			if (!userList.IsPublic && userList.UserId != currentUserId)
				throw new ForbiddenException("access", "private list");

			var dto = _mapper.Map<UserListDto>(userList);
			
			// Set current user's vote if applicable
			if (!string.IsNullOrEmpty(currentUserId))
			{
				var userVote = userList.Votes.FirstOrDefault(v => v.UserId == currentUserId);
				dto.CurrentUserVote = userVote?.IsUpvote;
			}

			return dto;
		}

		public async Task<UserListDto> GetListWithGamesAsync(Guid id, string? currentUserId = null)
		{
			var userList = await _userListRepository.GetByIdWithGamesAsync(id);
			if (userList == null)
				throw new NotFoundException("List", id);

			// Check privacy settings
			if (!userList.IsPublic && userList.UserId != currentUserId)
				throw new ForbiddenException("access", "private list");

			var dto = _mapper.Map<UserListDto>(userList);
			
			// Set current user's vote if applicable
			if (!string.IsNullOrEmpty(currentUserId))
			{
				var userVote = userList.Votes.FirstOrDefault(v => v.UserId == currentUserId);
				dto.CurrentUserVote = userVote?.IsUpvote;
			}

			return dto;
		}

		public async Task<List<UserListDto>> GetUserListsAsync(string userId, string? currentUserId = null)
		{
			var userLists = await _userListRepository.GetUserListsAsync(userId);
			
			// Filter private lists if not the owner
			if (currentUserId != userId)
			{
				userLists = userLists.Where(ul => ul.IsPublic).ToList();
			}

			var dtos = _mapper.Map<List<UserListDto>>(userLists);

			// Set current user's votes if applicable
			if (!string.IsNullOrEmpty(currentUserId))
			{
				foreach (var dto in dtos)
				{
					var userList = userLists.First(ul => ul.Id == dto.Id);
					var userVote = userList.Votes.FirstOrDefault(v => v.UserId == currentUserId);
					dto.CurrentUserVote = userVote?.IsUpvote;
				}
			}

			return dtos;
		}

		public async Task<PagedResult<UserListDto>> GetPublicListsAsync(int pageNumber = 1, int pageSize = 20, string? search = null, string? sortBy = null, string? currentUserId = null)
		{
			var result = await _userListRepository.GetPublicListsAsync(pageNumber, pageSize, search, sortBy);
			var dtos = _mapper.Map<List<UserListDto>>(result.Items);

			// Set current user's votes if applicable
			if (!string.IsNullOrEmpty(currentUserId))
			{
				foreach (var dto in dtos)
				{
					var userList = result.Items.First(ul => ul.Id == dto.Id);
					var userVote = userList.Votes.FirstOrDefault(v => v.UserId == currentUserId);
					dto.CurrentUserVote = userVote?.IsUpvote;
				}
			}

			return new PagedResult<UserListDto>
			{
				Items = dtos,
				TotalCount = result.TotalCount,
				PageNumber = result.PageNumber,
				PageSize = result.PageSize
			};
		}

		public async Task<UserListDto> CreateListAsync(CreateUserListDto dto, string userId)
		{
			var userList = _mapper.Map<UserList>(dto);
			userList.Id = Guid.NewGuid();
			userList.UserId = userId;
			userList.IsDefault = false;
			userList.DefaultListType = null;

			var createdList = await _userListRepository.CreateAsync(userList);
			return _mapper.Map<UserListDto>(createdList);
		}

		public async Task<UserListDto> UpdateListAsync(Guid id, UpdateUserListDto dto, string userId)
		{
			var userList = await _userListRepository.GetByIdAsync(id);
			if (userList == null)
				throw new NotFoundException("List", id);
			
			if (userList.UserId != userId)
				throw new ForbiddenException("update", "list");

			// Don't allow updating default lists' core properties
			if (!userList.IsDefault)
			{
				userList.Name = dto.Name;
				userList.Description = dto.Description;
				userList.IsPublic = dto.IsPublic;
			}
			else
			{
				// Only allow updating description and privacy for default lists
				userList.Description = dto.Description;
				userList.IsPublic = dto.IsPublic;
			}

			var updatedList = await _userListRepository.UpdateAsync(userList);
			return _mapper.Map<UserListDto>(updatedList);
		}

		public async Task DeleteListAsync(Guid id, string userId)
		{
			var userList = await _userListRepository.GetByIdAsync(id);
			if (userList == null)
				throw new NotFoundException("List", id);
			
			if (userList.UserId != userId)
				throw new ForbiddenException("delete", "list");

			// Don't allow deleting default lists
			if (userList.IsDefault)
				throw new BusinessException("Cannot delete default lists", "DELETE_DEFAULT_LIST", System.Net.HttpStatusCode.BadRequest);

			await _userListRepository.DeleteAsync(id);
		}

		public async Task AddGameToListAsync(Guid listId, Guid gameId, string userId)
		{
			var userList = await _userListRepository.GetByIdAsync(listId);
			if (userList == null)
				throw new NotFoundException("List", listId);
			
			if (userList.UserId != userId)
				throw new ForbiddenException("add games to", "list");

			// Check if game exists
			var game = await _gameRepository.GetByIdWithDetailsAsync(gameId);
			if (game == null)
				throw new NotFoundException("Game", gameId);

			// Check if game is already in the list
			if (await _userListRepository.IsGameInListAsync(listId, gameId))
				throw new ConflictException("Game is already in this list");

			await _userListRepository.AddGameToListAsync(listId, gameId);
		}

		public async Task RemoveGameFromListAsync(Guid listId, Guid gameId, string userId)
		{
			var userList = await _userListRepository.GetByIdAsync(listId);
			if (userList == null)
				throw new NotFoundException("List", listId);
			
			if (userList.UserId != userId)
				throw new ForbiddenException("remove games from", "list");

			await _userListRepository.RemoveGameFromListAsync(listId, gameId);
		}

		public async Task<bool> IsGameInListAsync(Guid listId, Guid gameId)
		{
			return await _userListRepository.IsGameInListAsync(listId, gameId);
		}

		public async Task VoteOnListAsync(Guid listId, bool isUpvote, string userId)
		{
			var userList = await _userListRepository.GetByIdAsync(listId);
			if (userList == null)
				throw new NotFoundException("List", listId);
			
			if (!userList.IsPublic)
				throw new BusinessException("Cannot vote on private lists", "PRIVATE_LIST_VOTING", System.Net.HttpStatusCode.BadRequest);

			// Users can't vote on their own lists
			if (userList.UserId == userId)
				throw new BusinessException("Cannot vote on your own lists", "SELF_VOTING", System.Net.HttpStatusCode.BadRequest);

			await _userListRepository.AddOrUpdateVoteAsync(listId, userId, isUpvote);
		}

		public async Task<bool> RemoveVoteAsync(Guid listId, string userId)
		{
			var userList = await _userListRepository.GetByIdAsync(listId);
			if (userList == null)
				return false;

			await _userListRepository.RemoveVoteAsync(listId, userId);
			return true;
		}

		public async Task CreateDefaultListsForUserAsync(string userId)
		{
			// Create "Want to Play" list
			var wantToPlayList = new UserList
			{
				Id = Guid.NewGuid(),
				Name = "Want to Play",
				Description = "Games I want to play in the future",
				UserId = userId,
				IsPublic = true,
				IsDefault = true,
				DefaultListType = "WantToPlay",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			// Create "Finished" list
			var finishedList = new UserList
			{
				Id = Guid.NewGuid(),
				Name = "Finished",
				Description = "Games I have completed",
				UserId = userId,
				IsPublic = true,
				IsDefault = true,
				DefaultListType = "Finished",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _userListRepository.CreateAsync(wantToPlayList);
			await _userListRepository.CreateAsync(finishedList);
		}

		public async Task<UserListDto?> GetUserDefaultListAsync(string userId, string defaultListType, string? currentUserId = null)
		{
			var userList = await _userListRepository.GetUserDefaultListAsync(userId, defaultListType);
			if (userList == null)
				return null;

			// Check privacy settings
			if (!userList.IsPublic && userList.UserId != currentUserId)
				return null;

			var dto = _mapper.Map<UserListDto>(userList);
			
			// Set current user's vote if applicable
			if (!string.IsNullOrEmpty(currentUserId))
			{
				var userVote = await _userListRepository.GetUserVoteAsync(userList.Id, currentUserId);
				dto.CurrentUserVote = userVote?.IsUpvote;
			}

			return dto;
		}

		public async Task<bool> AddGameToWantToPlayAsync(Guid gameId, string userId)
		{
			try
			{
				var wantToPlayList = await _userListRepository.GetUserDefaultListAsync(userId, "WantToPlay");
				if (wantToPlayList == null)
					return false;

				await AddGameToListAsync(wantToPlayList.Id, gameId, userId);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public async Task<bool> AddGameToFinishedAsync(Guid gameId, string userId)
		{
			try
			{
				var finishedList = await _userListRepository.GetUserDefaultListAsync(userId, "Finished");
				if (finishedList == null)
					return false;

				await AddGameToListAsync(finishedList.Id, gameId, userId);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public async Task<bool> MoveGameToFinished(Guid gameId, string userId)
		{
			var wantToPlayList = await _userListRepository.GetUserDefaultListAsync(userId, "WantToPlay");
			var finishedList = await _userListRepository.GetUserDefaultListAsync(userId, "Finished");
			
			if (wantToPlayList == null || finishedList == null)
				return false;

			// Remove from Want to Play list if it exists there
			if (await _userListRepository.IsGameInListAsync(wantToPlayList.Id, gameId))
			{
				await _userListRepository.RemoveGameFromListAsync(wantToPlayList.Id, gameId);
			}

			// Add to Finished list if not already there
			if (!await _userListRepository.IsGameInListAsync(finishedList.Id, gameId))
			{
				await _userListRepository.AddGameToListAsync(finishedList.Id, gameId);
			}

			return true;
		}
	}
}