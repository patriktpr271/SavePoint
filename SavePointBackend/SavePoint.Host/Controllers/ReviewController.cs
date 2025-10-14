using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.Common.Dtos.Reviews;
using SavePoint.Entities.Users;

namespace SavePoint.Host.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ReviewController : ControllerBase
	{
		private readonly IReviewService _reviewService;
		private readonly UserManager<ApplicationUser> _userManager;

		public ReviewController(IReviewService reviewService, UserManager<ApplicationUser> userManager)
		{
			_reviewService = reviewService;
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

		#region Public Review Endpoints

		[HttpGet("game/{gameId:guid}")]
		public async Task<IActionResult> GetReviewsForGame(
			Guid gameId,
			[FromQuery] int pageNumber = 1,
			[FromQuery] int pageSize = 20,
			[FromQuery] string? sortBy = "newest")
		{
			if (pageNumber < 1) pageNumber = 1;
			if (pageSize < 1 || pageSize > 50) pageSize = 20;

			var result = await _reviewService.GetPagedAsync(pageNumber, pageSize, gameId);
			
			// Return as an anonymous object instead of tuple for proper JSON serialization
			return Ok(new 
			{
				Reviews = result.Reviews,
				TotalCount = result.TotalCount
			});
		}

		[HttpGet("game/{gameId:guid}/average")]
		public async Task<IActionResult> GetAverageRating(Guid gameId)
		{
			var averageRating = await _reviewService.GetAverageRatingForGameAsync(gameId);
			return Ok(new { gameId, averageRating });
		}

		[HttpGet("game/{gameId:guid}/count")]
		public async Task<IActionResult> GetReviewCount(Guid gameId)
		{
			var count = await _reviewService.GetReviewCountForGameAsync(gameId);
			return Ok(new { gameId, reviewCount = count });
		}

		[HttpGet("{id:guid}")]
		public async Task<IActionResult> GetReview(Guid id)
		{
			var review = await _reviewService.GetByIdAsync(id);
			if (review == null)
			{
				return NotFound(new { message = "Review not found" });
			}

			return Ok(review);
		}

		#endregion

		#region User Review Management

		[HttpGet("user/{userId}")]
		public async Task<IActionResult> GetUserReviews(
			string userId,
			[FromQuery] int pageNumber = 1,
			[FromQuery] int pageSize = 20)
		{
			if (pageNumber < 1) pageNumber = 1;
			if (pageSize < 1 || pageSize > 50) pageSize = 20;

			var result = await _reviewService.GetPagedAsync(pageNumber, pageSize, null, userId);
			
			// Return as an anonymous object instead of tuple for proper JSON serialization
			return Ok(new 
			{
				Reviews = result.Reviews,
				TotalCount = result.TotalCount
			});
		}

		[HttpGet("my")]
		[Authorize]
		public async Task<IActionResult> GetMyReviews(
			[FromQuery] int pageNumber = 1,
			[FromQuery] int pageSize = 20)
		{
			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			if (pageNumber < 1) pageNumber = 1;
			if (pageSize < 1 || pageSize > 50) pageSize = 20;

			var result = await _reviewService.GetPagedAsync(pageNumber, pageSize, null, currentUserId);
			
			// Return as an anonymous object instead of tuple for proper JSON serialization
			return Ok(new 
			{
				Reviews = result.Reviews,
				TotalCount = result.TotalCount
			});
		}

		[HttpGet("my/game/{gameId:guid}")]
		[Authorize]
		public async Task<IActionResult> GetMyReviewForGame(Guid gameId)
		{
			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			var review = await _reviewService.GetUserReviewForGameAsync(currentUserId, gameId);
			if (review == null)
			{
				return NotFound(new { message = "Review not found" });
			}

			return Ok(review);
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			try
			{
				var createdReview = await _reviewService.CreateAsync(dto, currentUserId);
				return CreatedAtAction(nameof(GetReview), new { id = createdReview.Id }, createdReview);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpPut("{id:guid}")]
		[Authorize]
		public async Task<IActionResult> UpdateReview(Guid id, [FromBody] UpdateReviewDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			try
			{
				var updatedReview = await _reviewService.UpdateAsync(id, dto, currentUserId);
				if (updatedReview == null)
				{
					return NotFound(new { message = "Review not found or access denied" });
				}

				return Ok(updatedReview);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpDelete("{id:guid}")]
		[Authorize]
		public async Task<IActionResult> DeleteReview(Guid id)
		{
			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			var success = await _reviewService.DeleteAsync(id, currentUserId);
			if (!success)
			{
				return NotFound(new { message = "Review not found or access denied" });
			}

			return NoContent();
		}

		#endregion

		#region Review Statistics

		[HttpGet("statistics/user/{userId}")]
		public async Task<IActionResult> GetUserReviewStatistics(string userId)
		{
			var reviews = await _reviewService.GetByUserIdAsync(userId);
			var count = reviews.Count();
			var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
			return Ok(new { TotalReviews = count, AverageRating = averageRating });
		}

		[HttpGet("statistics/my")]
		[Authorize]
		public async Task<IActionResult> GetMyReviewStatistics()
		{
			var currentUserId = await GetCurrentUserIdAsync();
			if (currentUserId == null)
				return Unauthorized();

			var reviews = await _reviewService.GetByUserIdAsync(currentUserId);
			var count = reviews.Count();
			var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
			return Ok(new { TotalReviews = count, AverageRating = averageRating });
		}

		#endregion
	}
}