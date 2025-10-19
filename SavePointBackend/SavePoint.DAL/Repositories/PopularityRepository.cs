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
			var gameIds = popularities.Select(p => p.GameId).Distinct().ToList();

			// Verify all referenced games exist in the database before processing
			var existingGameIds = await _context.Games
				.Where(g => gameIds.Contains(g.Id))
				.Select(g => g.Id)
				.ToListAsync();

			// Filter out popularity records for games that don't exist
			var validPopularities = popularities
				.Where(p => existingGameIds.Contains(p.GameId))
				.ToList();

			if (!validPopularities.Any())
			{
				Console.WriteLine($"Warning: No valid games found for {popularities.Count} popularity records. Skipping batch.");
				return;
			}

			if (validPopularities.Count != popularities.Count)
			{
				Console.WriteLine($"Warning: {popularities.Count - validPopularities.Count} popularity records skipped due to missing games.");
			}

			// Fetch existing popularity records from the database
			var existingPopularities = await _context.Popularities
				.Where(p => externalGameIds.Contains(p.ExternalGameId) && popularityTypes.Contains(p.PopularityType))
				.ToListAsync();

			foreach (var popularity in validPopularities)
			{
				try
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
						// Only update GameId if the new game exists and is different
						if (existingGameIds.Contains(popularity.GameId) && existing.GameId != popularity.GameId)
						{
							existing.GameId = popularity.GameId;
						}
						existing.UpdatedAt = DateTime.UtcNow;
						_context.Popularities.Update(existing);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error processing popularity for external game ID {popularity.ExternalGameId}: {ex.Message}");
					// Continue with the next record instead of failing the entire batch
				}
			}

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error saving popularity batch: {ex.Message}");
				throw;
			}
		}
	}
}
