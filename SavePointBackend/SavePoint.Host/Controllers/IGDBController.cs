using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SavePoint.BusinessLogic.Services;
using SavePoint.BusinessLogic.Services.Interfaces;


namespace SavePoint.Host.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(Roles = "Admin")] // Require Admin role for all endpoints
	public class ImportController : ControllerBase
	{
		private readonly IIGDBImportService _importService;

		public ImportController(IIGDBImportService importService)
		{
			_importService = importService;
		}

		[HttpPost("genres")]
		public async Task<IActionResult> ImportGenres()
		{
			await _importService.ImportGenresAsync();
			return Ok("Genres imported successfully.");
		}
		[HttpPost("games")]
		public async Task<IActionResult> ImportGames()
		{
			await _importService.ImportGamesAsync();
			return Ok("Games imported successfully.");
		}

		[HttpPost("companies")]
		public async Task<IActionResult> ImportCompanies()
		{
			await _importService.ImportCompaniesAsync();
			return Ok("Companies imported successfully.");
		}
		[HttpPost("platforms")]
		public async Task<IActionResult> ImportPlatforms()
		{
			await _importService.ImportPlatformsAsync();
			return Ok("Platforms imported successfully.");
		}
		[HttpPost("all")]
		public async Task<IActionResult> ImportAll()
		{
			await _importService.ImportAllDataAsync();
			return Ok("All data imported successfully.");
		}
		[HttpPost("popularity")]
		public async Task<IActionResult> ImportPopularity()
		{
			await _importService.ImportPopularityAsync();
			return Ok("Popularity data imported successfully.");
		}

		[HttpPost("games-with-popularity")]
		public async Task<IActionResult> ImportGamesWithBatchedPopularity()
		{
			await _importService.ImportGamesWithBatchedPopularityAsync();
			return Ok("Games with popularity imported successfully in batches.");
		}
	}
}
