using Microsoft.EntityFrameworkCore;
using SavePoint.DAL.Contexts;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Popularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.DAL.Repositories
{
	public class PopularityRepository : IPopularityRepository
	{
		private readonly ApplicationDbContext _context;

		public PopularityRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<List<Popularity>> GetAllAsync()
		{
			return await _context.Popularities
				.Include(p => p.Game)
				.ToListAsync();
		}

		public async Task<List<Popularity>> GetByGameIdAsync(Guid gameId)
		{
			return await _context.Popularities
				.Include(p => p.Game)
				.Where(p => p.GameId == gameId)
				.ToListAsync();
		}

		public async Task InsertOrUpdateAsyncBatch(List<Popularity> popularities)
		{
			if (!popularities.Any())
				return;

			// Get all external game IDs and popularity types from the input list
			var externalGameIds = popularities.Select(p => p.ExternalGameId).ToList();
			var popularityTypes = popularities.Select(p => p.PopularityType).Distinct().ToList();

			// Fetch existing popularity records from the database
			var existingPopularities = await _context.Popularities
				.Where(p => externalGameIds.Contains(p.ExternalGameId) && popularityTypes.Contains(p.PopularityType))
				.ToListAsync();

			foreach (var popularity in popularities)
			{
				// Find existing popularity record by ExternalGameId and PopularityType
				var existing = existingPopularities.FirstOrDefault(p =>
					p.ExternalGameId == popularity.ExternalGameId &&
					p.PopularityType == popularity.PopularityType);

				if (existing == null)
				{
					// Add new popularity record
					await _context.Popularities.AddAsync(popularity);
				}
				else
				{
					// Update existing popularity record
					existing.PopularityScore = popularity.PopularityScore;
					existing.GameId = popularity.GameId; // Update GameId in case it changed
					existing.UpdatedAt = DateTime.UtcNow;
					_context.Popularities.Update(existing);
				}
			}

			await _context.SaveChangesAsync();
		}
	}
}
