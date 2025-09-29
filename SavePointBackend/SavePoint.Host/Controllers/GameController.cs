using Microsoft.AspNetCore.Mvc;
using SavePoint.BusinessLogic.Services.Interfaces;

namespace SavePoint.Host.Controllers
{
	[Route("api/[controller]")]
	public class GameController : ControllerBase
	{
		private readonly IGameService _gameService;

		public GameController(IGameService gameService)
		{
			_gameService = gameService;
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
