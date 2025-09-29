using Microsoft.AspNetCore.Mvc;
using SavePoint.BusinessLogic.Services.Interfaces;

namespace SavePoint.Host.Controllers
{
	[ApiController]
	[Route("api/game")] // Changed to match frontend expectation
	public class GameController : ControllerBase
	{
		private readonly IGameService _gameService;

		public GameController(IGameService gameService)
		{
			_gameService = gameService;
		}

		[HttpGet]
		public async Task<IActionResult> GetGames(
			[FromQuery] int pageNumber = 1,
			[FromQuery] int pageSize = 20,
			[FromQuery] string? search = null,
			[FromQuery] string? sortBy = null, 
			[FromQuery] string? sortOrder = "asc",
			[FromQuery] double? minRating = null,
			[FromQuery] double? maxRating = null,
			[FromQuery] int? fromYear = null,
			[FromQuery] int? toYear = null,
			[FromQuery] Guid[]? platforms = null, 
			[FromQuery] Guid[]? companies = null, 
			[FromQuery] Guid[]? genres = null) 
		{
			if (pageNumber < 1) pageNumber = 1;
			if (pageSize < 1 || pageSize > 100) pageSize = 20;

			// Validate sort parameters
			if (!string.IsNullOrEmpty(sortBy))
			{
				var validSortFields = new[] { "name", "rating", "releasedate" };
				if (!validSortFields.Contains(sortBy.ToLower()))
				{
					return BadRequest(new { Message = "Invalid sortBy field. Valid options: name, rating, releaseDate" });
				}
			}

			if (!string.IsNullOrEmpty(sortOrder) && !new[] { "asc", "desc" }.Contains(sortOrder.ToLower()))
			{
				return BadRequest(new { Message = "Invalid sortOrder. Valid options: asc, desc" });
			}

			var result = await _gameService.GetGames(
				pageNumber, 
				pageSize, 
				search, 
				sortBy?.ToLower(), 
				sortOrder?.ToLower() ?? "asc",
				minRating, 
				maxRating, 
				fromYear,
				toYear,
				platforms, 
				companies, 
				genres); 

			return Ok(result);
		}

		[HttpGet("popular/{popularityType}")]
		public async Task<IActionResult> GetPopularGames(
			int popularityType,
			[FromQuery] int pageNumber = 1,
			[FromQuery] int pageSize = 20)
		{
			if (pageNumber < 1) pageNumber = 1;
			if (pageSize < 1 || pageSize > 100) pageSize = 20;

			if (pageNumber > 5) pageNumber = 5;

			var result = await _gameService.GetPopularGames(popularityType, pageNumber, pageSize);

			var maxItems = 5 * pageSize;
			if (result.TotalCount > maxItems)
			{
				result.TotalCount = maxItems;
			}

			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetGameById(Guid id)
		{
			var game = await _gameService.GetGameById(id);
			if (game == null)
			{
				return NotFound(new { Message = "Game not found" });
			}

			return Ok(game);
		}
	}
}
