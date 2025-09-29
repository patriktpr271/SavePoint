using SavePoint.Entities.Common;
using SavePoint.Common.Dtos.Games;

namespace SavePoint.BusinessLogic.Services.Interfaces
{
	public interface IGameService
	{
		Task<PagedResult<GameCardDto>> GetPopularGames(int popularityType, int pageNumber = 1, int pageSize = 20);
		Task<PagedResult<GameCardDto>> GetGames(
			int pageNumber = 1, 
			int pageSize = 20,
			string? search = null,
			string? sortBy = null,
			string? sortOrder = "asc",
			double? minRating = null,
			double? maxRating = null,
			int? fromYear = null,
			int? toYear = null,
			Guid[]? platformIds = null, // Changed to Guid array for IDs
			Guid[]? companyIds = null, // Changed to Guid array for IDs
			Guid[]? genreIds = null); // Changed to Guid array for IDs
		Task<GameDetailDto?> GetGameById(Guid id);
	}
}
