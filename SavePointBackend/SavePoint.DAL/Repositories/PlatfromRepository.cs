using Microsoft.EntityFrameworkCore;
using SavePoint.DAL.Contexts;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.DAL.Repositories
{
	public class PlatfromRepository : IPlatfromRepository
	{
		private readonly ApplicationDbContext _context;
		public PlatfromRepository(ApplicationDbContext context)
		{
			_context = context;
		}
		public async Task<List<Platform>> GetAllAsync()
		{
			return await _context.Platforms.ToListAsync();
		}

		public Task<List<Platform>> GetByExternalIds(IEnumerable<long> externalIds)
		{
			throw new NotImplementedException();
		}

		public async Task InsertOrUpdateAsyncBatch(List<Platform> platforms)
		{
			//get all external ids from the input list 
			var externalIds = platforms
				.Where(p => p.ExternalId.HasValue)
				.Select(p => p.ExternalId.Value)
				.ToList();

			//fetch existing platforms from the database
			var existingPlatforms = await _context.Platforms
				.Where(p => p.ExternalId.HasValue && externalIds.Contains(p.ExternalId.Value))
				.ToListAsync();

			foreach (var platform in platforms)
			{
				var existing = existingPlatforms.FirstOrDefault(p => p.ExternalId == platform.ExternalId);

				if (existing == null)
				{
					await _context.Platforms.AddAsync(platform);
				}
				else
				{
					existing.Name = platform.Name;
					existing.UpdatedAt = DateTime.UtcNow;
					existing.Abbreviation = platform.Abbreviation;
					_context.Platforms.Update(existing);
				}
			}
			await _context.SaveChangesAsync();
		}
	}
}
