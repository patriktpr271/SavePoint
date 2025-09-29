using Microsoft.EntityFrameworkCore;
using SavePoint.DAL.Contexts;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Common;
using SavePoint.Entities.Games;

namespace SavePoint.DAL.Repositories
{
	public class GameRepository : IGameRepository
	{
		private readonly ApplicationDbContext _context;

		public GameRepository(ApplicationDbContext context)
		{
			_context = context;
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

				_context.Games.Update(existing);
			}

			await _context.SaveChangesAsync();
		}

		public async Task<Game?> GetByExternalIdAsync(long? externalId)
		{
			return await _context.Games
				.Include(g => g.GameGenres)
				.Include(g => g.GamePlatforms)
				.Include(g => g.GameCompanies)
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