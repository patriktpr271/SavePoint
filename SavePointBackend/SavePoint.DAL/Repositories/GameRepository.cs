using Microsoft.EntityFrameworkCore;
using SavePoint.DAL.Contexts;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Common;
using SavePoint.Entities.Games;
using SavePoint.Entities.Popularity;

namespace SavePoint.DAL.Repositories
{
	public class GameRepository : IGameRepository
	{
		private readonly ApplicationDbContext _context;

		public GameRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<PagedResult<Game>> GetGames(
			int pageNumber = 1, 
			int pageSize = 20,
			string? search = null,
			string? sortBy = null,
			string? sortOrder = "asc",
			double? minRating = null,
			double? maxRating = null,
			int? fromYear = null,
			int? toYear = null,
			Guid[]? platformIds = null, 
			Guid[]? companyIds = null, 
			Guid[]? genreIds = null) 
		{
			var query = _context.Games.AsQueryable();

			// Text search in name and summary
			if (!string.IsNullOrWhiteSpace(search))
			{
				query = query.Where(g => g.Name.Contains(search) || 
									   (g.Summary != null && g.Summary.Contains(search)));
			}

			// Rating range filtering
			if (minRating.HasValue)
			{
				query = query.Where(g => g.Rating >= minRating.Value);
			}

			if (maxRating.HasValue)
			{
				query = query.Where(g => g.Rating <= maxRating.Value);
			}

			// Release year range filtering
			if (fromYear.HasValue)
			{
				query = query.Where(g => g.ReleaseDate.Year >= fromYear.Value);
			}

			if (toYear.HasValue)
			{
				query = query.Where(g => g.ReleaseDate.Year <= toYear.Value);
			}

			// Multiple platform filtering by ID - game must have at least one of the specified platforms
			if (platformIds != null && platformIds.Length > 0)
			{
				query = query.Where(g => g.GamePlatforms.Any(gp => platformIds.Contains(gp.PlatformId)));
			}

			// Multiple company filtering by ID - game must have at least one of the specified companies
			if (companyIds != null && companyIds.Length > 0)
			{
				query = query.Where(g => g.GameCompanies.Any(gc => companyIds.Contains(gc.CompanyId)));
			}

			// Multiple genre filtering by ID - game must have at least one of the specified genres
			if (genreIds != null && genreIds.Length > 0)
			{
				query = query.Where(g => g.GameGenres.Any(gg => genreIds.Contains(gg.GenreId)));
			}

			// Include related data only if we need it for filtering or if no filtering is applied
			// Note: When filtering by IDs, we don't need to include the related entities for filtering,
			// but we might want them for the response. Let's include them conditionally.
			var needsIncludes = (platformIds != null && platformIds.Length > 0) || 
								(companyIds != null && companyIds.Length > 0) || 
								(genreIds != null && genreIds.Length > 0);

			if (needsIncludes)
			{
				query = query.Include(g => g.GamePlatforms)
							 .ThenInclude(gp => gp.Platform)
							 .Include(g => g.GameCompanies)
							 .ThenInclude(gc => gc.Company)
							 .Include(g => g.GameGenres)
							 .ThenInclude(gg => gg.Genre);
			}

			// Apply sorting
			query = ApplySorting(query, sortBy, sortOrder);

			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return new PagedResult<Game>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = pageNumber,
				PageSize = pageSize
			};
		}

		private IQueryable<Game> ApplySorting(IQueryable<Game> query, string? sortBy, string? sortOrder)
		{
			var isDescending = sortOrder?.ToLower() == "desc";

			return sortBy?.ToLower() switch
			{
				"name" => isDescending 
					? query.OrderByDescending(g => g.Name)
					: query.OrderBy(g => g.Name),
				
				"rating" => isDescending 
					? query.OrderByDescending(g => g.Rating)
					: query.OrderBy(g => g.Rating),
				
				"releasedate" => isDescending 
					? query.OrderByDescending(g => g.ReleaseDate)
					: query.OrderBy(g => g.ReleaseDate),
				
				// Default sorting by name ascending
				_ => query.OrderBy(g => g.Name)
			};
		}

		public async Task InsertOrUpdateAsync(Game game)
		{
			var existing = await GetByExternalIdAsync(game.ExternalId);

			if (existing == null)
			{
				// Add new game and its relationships
				await _context.Games.AddAsync(game);
			}
			else
			{
				existing.Name = game.Name;
				existing.Summary = game.Summary;
				existing.CoverUrl = game.CoverUrl;
				existing.Rating = game.Rating;
				existing.ReleaseDate = game.ReleaseDate;
				existing.UpdatedAt = DateTime.UtcNow;

				// Update GameGenres
				var newGenreIds = game.GameGenres.Select(gg => gg.GenreId).ToHashSet();
				var toRemove = existing.GameGenres.Where(gg => !newGenreIds.Contains(gg.GenreId)).ToList();
				_context.GameGenres.RemoveRange(toRemove);

				var existingGenreIds = existing.GameGenres.Select(gg => gg.GenreId).ToHashSet();
				var toAdd = game.GameGenres.Where(gg => !existingGenreIds.Contains(gg.GenreId)).ToList();
				foreach (var gg in toAdd)
				{
					existing.GameGenres.Add(new GameGenre
					{
						GameId = existing.Id,
						GenreId = gg.GenreId
					});
				}

				// Update GamePlatforms
				var newPlatformIds = game.GamePlatforms.Select(gp => gp.PlatformId).ToHashSet();
				var platformsToRemove = existing.GamePlatforms.Where(gp => !newPlatformIds.Contains(gp.PlatformId)).ToList();
				_context.GamePlatforms.RemoveRange(platformsToRemove);

				var existingPlatformIds = existing.GamePlatforms.Select(gp => gp.PlatformId).ToHashSet();
				var platformsToAdd = game.GamePlatforms.Where(gp => !existingPlatformIds.Contains(gp.PlatformId)).ToList();
				foreach (var gp in platformsToAdd)
				{
					existing.GamePlatforms.Add(new GamePlatform
					{
						GameId = existing.Id,
						PlatformId = gp.PlatformId
					});
				}

				// Update GameCompanies
				var newCompanyRoles = game.GameCompanies.Select(gc => new { gc.CompanyId, gc.Role }).ToHashSet();
				var companiesToRemove = existing.GameCompanies.Where(gc => !newCompanyRoles.Contains(new { gc.CompanyId, gc.Role })).ToList();
				_context.GameCompanies.RemoveRange(companiesToRemove);

				var existingCompanyRoles = existing.GameCompanies.Select(gc => new { gc.CompanyId, gc.Role }).ToHashSet();
				var companiesToAdd = game.GameCompanies.Where(gc => !existingCompanyRoles.Contains(new { gc.CompanyId, gc.Role })).ToList();
				foreach (var gc in companiesToAdd)
				{
					existing.GameCompanies.Add(new GameCompany
					{
						GameId = existing.Id,
						CompanyId = gc.CompanyId,
						Role = gc.Role
					});
				}

				// Update Popularities if provided
				if (game.Popularities?.Any() == true)
				{
					// Remove existing popularities that are not in the new set
					var newPopularityTypes = game.Popularities.Select(p => p.PopularityType).ToHashSet();
					var popularitiesToRemove = existing.Popularities
						.Where(p => !newPopularityTypes.Contains(p.PopularityType))
						.ToList();
					_context.Popularities.RemoveRange(popularitiesToRemove);

					// Add or update popularities
					var existingPopularityTypes = existing.Popularities.Select(p => p.PopularityType).ToHashSet();
					foreach (var newPop in game.Popularities)
					{
						var existingPop = existing.Popularities
							.FirstOrDefault(p => p.PopularityType == newPop.PopularityType);
						
						if (existingPop != null)
						{
							// Update existing
							existingPop.PopularityScore = newPop.PopularityScore;
							existingPop.UpdatedAt = DateTime.UtcNow;
						}
						else
						{
							// Add new
							existing.Popularities.Add(new Popularity
							{
								Id = Guid.NewGuid(),
								GameId = existing.Id,
								ExternalGameId = newPop.ExternalGameId,
								PopularityScore = newPop.PopularityScore,
								PopularityType = newPop.PopularityType,
								CreatedAt = DateTime.UtcNow,
								UpdatedAt = DateTime.UtcNow
							});
						}
					}
				}

				_context.Games.Update(existing);
			}

			await _context.SaveChangesAsync();
		}

		public async Task<HashSet<long>> GetExistingExternalIdsAsync(IEnumerable<long> externalIds)
		{
			var existingIds = await _context.Games
				.Where(g => g.ExternalId.HasValue && externalIds.Contains(g.ExternalId.Value))
				.Select(g => g.ExternalId.Value)
				.ToListAsync();
			
			return new HashSet<long>(existingIds);
		}

		public async Task<Game?> GetByExternalIdAsync(long? externalId)
		{
			return await _context.Games
				.Include(g => g.GameGenres)
				.Include(g => g.GamePlatforms)
				.Include(g => g.GameCompanies)
				.Include(g => g.Popularities)
				.FirstOrDefaultAsync(x => x.ExternalId == externalId);
		}

		public async Task<Game?> GetByIdWithDetailsAsync(Guid id)
		{
			return await _context.Games
				.Include(g => g.GameGenres)
					.ThenInclude(gg => gg.Genre)
				.Include(g => g.GamePlatforms)
					.ThenInclude(gp => gp.Platform)
				.Include(g => g.GameCompanies)
					.ThenInclude(gc => gc.Company)
				.Include(g => g.Reviews)
				.Include(g => g.Popularities)
				.FirstOrDefaultAsync(g => g.Id == id);
		}

		public async Task<List<Game>> GetAllAsync()
		{
			return await _context.Games
				.Include(g => g.GameGenres)
				.Include(g => g.GamePlatforms)
				.Include(g => g.GameCompanies)
				.ToListAsync();
		}

		public async Task<PagedResult<Game>> GetPopularGames(int popularityType, int pageNumber = 1, int pageSize = 20)
		{
			//get games ordered by popularity score by the popularity type
			var query = _context.Games
				.Include(g => g.GameGenres)
					.ThenInclude(gg => gg.Genre)
				.Include(g => g.GamePlatforms)
					.ThenInclude(gp => gp.Platform)
				.Include(g => g.GameCompanies)
					.ThenInclude(gc => gc.Company)
				.Include(g => g.Popularities)
				.Where(g => g.Popularities.Any(p => p.PopularityType == popularityType))
				.OrderByDescending(g => g.Popularities
					.Where(p => p.PopularityType == popularityType)
					.Max(p => p.PopularityScore));

						var totalCount = await query.CountAsync();

						var items = await query
							.Skip((pageNumber - 1) * pageSize)
							.Take(pageSize)
							.ToListAsync();

			return new PagedResult<Game>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = pageNumber,
				PageSize = pageSize
			};
		}
	}
}