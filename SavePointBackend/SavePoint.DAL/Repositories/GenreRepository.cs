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
	public class GenreRepository : IGenreRepository
	{

		private readonly ApplicationDbContext _context;
		public GenreRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<List<Genre>> GetByExternalIds(IEnumerable<long> externalIds)
		{
			var externalIdsList = externalIds.ToList();
			return await _context.Genres
				.Where(g => externalIdsList.Contains((long)g.ExternalId))
				.ToListAsync();
		}

		public async Task InsertOrUpdateAsync(Genre genre)
		{
			var existing = await _context.Genres
				.FirstOrDefaultAsync(x => x.ExternalId == genre.ExternalId);

			if (existing == null)
			{
				await _context.Genres.AddAsync(genre);
			}
			else
			{
				existing.Name = genre.Name;
				existing.UpdatedAt = DateTime.UtcNow;
				_context.Genres.Update(existing);
			}

			await _context.SaveChangesAsync();
		}

		public async Task<List<Genre>> GetAllAsync()
		{
			return await _context.Genres.ToListAsync();
		}
	}
}
