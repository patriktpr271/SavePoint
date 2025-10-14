using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.Common.Dtos.Lists;
using SavePoint.Entities.Users;

namespace SavePoint.Host.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ListController : ControllerBase
	{
		private readonly IUserListService _userListService;
		private readonly UserManager<ApplicationUser> _userManager;

		public ListController(IUserListService userListService, UserManager<ApplicationUser> userManager)
		{
			_userListService = userListService;
			_userManager = userManager;
		}

		private async Task<string?> GetCurrentUserIdAsync()
		{
			if (User.Identity?.IsAuthenticated == true)
			{
				var user = await _userManager.GetUserAsync(User);
				return user?.Id;
			}
			return null;
		}

		#region Public List Endpoints

		[HttpGet("public")]
		public async Task<IActionResult> GetPublicLists(
			[FromQuery] int pageNumber = 1,
			[FromQuery] int pageSize = 20,
			[FromQuery] string? search = null,
			[FromQuery] string? sortBy = null)
		{
			var currentUserId = await GetCurrentUserIdAsync();
			var result = await _userListService.GetPublicListsAsync(pageNumber, pageSize, search, sortBy, currentUserId);
			return Ok(result);
		}

		[HttpGet("{id:guid}")]
		public async Task<IActionResult> GetList(Guid id, [FromQuery] bool includeGames = false)
		{
			var currentUserId = await GetCurrentUserIdAsync();
			
			var list = includeGames 
				? await _userListService.GetListWithGamesAsync(id, currentUserId)
				: await _userListService.GetListByIdAsync(id, currentUserId);

			if (list == null)
			{
				return NotFound(new { message = "List not found or access denied" });
			}

			return Ok(list);
		}

		#endregion

		#region User List Management

		[HttpGet("user/{userId}")]
		public async Task<IActionResult> GetUserLists(string userId)
		{
			var currentUserId = await GetCurrentUserIdAsync();
			var lists = await _userListService.GetUserListsAsync(userId, currentUserId);
			return Ok(lists);
		}

		[HttpGet("my")]
		[Authorize]
		public async Task<IActionResult> GetMyLists()
		{
			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			var lists = await _userListService.GetUserListsAsync(currentUserId, currentUserId);
			return Ok(lists);
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> CreateList([FromBody] CreateUserListDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			var createdList = await _userListService.CreateListAsync(dto, currentUserId);
			return CreatedAtAction(nameof(GetList), new { id = createdList.Id }, createdList);
		}

		[HttpPut("{id:guid}")]
		[Authorize]
		public async Task<IActionResult> UpdateList(Guid id, [FromBody] UpdateUserListDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			var updatedList = await _userListService.UpdateListAsync(id, dto, currentUserId);
			if (updatedList == null)
			{
				return NotFound(new { message = "List not found or access denied" });
			}

			return Ok(updatedList);
		}

		[HttpDelete("{id:guid}")]
		[Authorize]
		public async Task<IActionResult> DeleteList(Guid id)
		{
			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			var success = await _userListService.DeleteListAsync(id, currentUserId);
			if (!success)
			{
				return NotFound(new { message = "List not found, access denied, or cannot delete default list" });
			}

			return NoContent();
		}

		#endregion

		#region Game Management in Lists

		[HttpPost("{id:guid}/games")]
		[Authorize]
		public async Task<IActionResult> AddGameToList(Guid id, [FromBody] AddGameToListDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			var success = await _userListService.AddGameToListAsync(id, dto.GameId, currentUserId);
			if (!success)
			{
				return BadRequest(new { message = "Failed to add game. List not found, access denied, game not found, or game already in list." });
			}

			return Ok(new { message = "Game added to list successfully" });
		}

		[HttpDelete("{id:guid}/games/{gameId:guid}")]
		[Authorize]
		public async Task<IActionResult> RemoveGameFromList(Guid id, Guid gameId)
		{
			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			var success = await _userListService.RemoveGameFromListAsync(id, gameId, currentUserId);
			if (!success)
			{
				return NotFound(new { message = "List not found or access denied" });
			}

			return Ok(new { message = "Game removed from list successfully" });
		}

		[HttpGet("{id:guid}/games/{gameId:guid}/check")]
		public async Task<IActionResult> CheckGameInList(Guid id, Guid gameId)
		{
			var isInList = await _userListService.IsGameInListAsync(id, gameId);
			return Ok(new { isInList });
		}

		#endregion

		#region Voting

		[HttpPost("{id:guid}/vote")]
		[Authorize]
		public async Task<IActionResult> VoteOnList(Guid id, [FromBody] VoteOnListDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			var success = await _userListService.VoteOnListAsync(id, dto.IsUpvote, currentUserId);
			if (!success)
			{
				return BadRequest(new { message = "Failed to vote. List not found, not public, or you cannot vote on your own list." });
			}

			return Ok(new { message = "Vote recorded successfully" });
		}

		[HttpDelete("{id:guid}/vote")]
		[Authorize]
		public async Task<IActionResult> RemoveVote(Guid id)
		{
			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			var success = await _userListService.RemoveVoteAsync(id, currentUserId);
			if (!success)
			{
				return NotFound(new { message = "List not found" });
			}

			return Ok(new { message = "Vote removed successfully" });
		}

		#endregion

		#region Default List Shortcuts

		[HttpGet("default/{defaultListType}")]
		[Authorize]
		public async Task<IActionResult> GetMyDefaultList(string defaultListType)
		{
			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			var list = await _userListService.GetUserDefaultListAsync(currentUserId, defaultListType, currentUserId);
			if (list == null)
			{
				return NotFound(new { message = "Default list not found" });
			}

			return Ok(list);
		}

		[HttpPost("want-to-play/{gameId:guid}")]
		[Authorize]
		public async Task<IActionResult> AddToWantToPlay(Guid gameId)
		{
			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			var success = await _userListService.AddGameToWantToPlayAsync(gameId, currentUserId);
			if (!success)
			{
				return BadRequest(new { message = "Failed to add game to Want to Play list" });
			}

			return Ok(new { message = "Game added to Want to Play list" });
		}

		[HttpPost("finished/{gameId:guid}")]
		[Authorize]
		public async Task<IActionResult> AddToFinished(Guid gameId)
		{
			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			var success = await _userListService.AddGameToFinishedAsync(gameId, currentUserId);
			if (!success)
			{
				return BadRequest(new { message = "Failed to add game to Finished list" });
			}

			return Ok(new { message = "Game added to Finished list" });
		}

		[HttpPost("move-to-finished/{gameId:guid}")]
		[Authorize]
		public async Task<IActionResult> MoveToFinished(Guid gameId)
		{
			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			var success = await _userListService.MoveGameToFinished(gameId, currentUserId);
			if (!success)
			{
				return BadRequest(new { message = "Failed to move game to Finished list" });
			}

			return Ok(new { message = "Game moved to Finished list" });
		}

		#endregion
	}
}