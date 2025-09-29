using SavePoint.Entities.Common;
using SavePoint.Entities.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.DAL.Repositories.Interfaces
{
	public interface IGameRepository
	{
		Task InsertOrUpdateAsync(Game game);
		Task<Game?> GetByExternalIdAsync(long? externalId);
		Task<Game?> GetByIdWithDetailsAsync(Guid id);
		Task<List<Game>> GetAllAsync();
		Task<PagedResult<Game>> GetGames(
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
		Task<PagedResult<Game>> GetPopularGames(int popularityType, int pageNumber = 1, int pageSize = 20);
	}
}
