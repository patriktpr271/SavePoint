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
	public class CompanyRepository : ICompanyRepository
	{
		private readonly ApplicationDbContext _context;

		public CompanyRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<List<Company>> GetAllAsync()
		{
			return await _context.Companies.ToListAsync();
		}

		public async Task<List<Company>> GetByExternalIds(IEnumerable<long> externalIds)
		{
			var externalIdsList = externalIds.ToList();
			return await _context.Companies
				.Where(c => externalIdsList.Contains((long)c.ExternalId))
				.ToListAsync();
		}

		public async Task<Company?> GetByExternalIdAsync(long externalId)
		{
			return await _context.Companies
				.FirstOrDefaultAsync(c => c.ExternalId == externalId);
		}

		public async Task InsertOrUpdateAsyncBatch(List<Company> companies)
		{
			// Get all external IDs from the input list
			var externalIds = companies
				.Where(c => c.ExternalId.HasValue)
				.Select(c => c.ExternalId.Value)
				.ToList();

			// Fetch existing companies from the database
			var existingCompanies = await _context.Companies
				.Where(c => c.ExternalId.HasValue && externalIds.Contains(c.ExternalId.Value))
				.ToListAsync();

			foreach (var company in companies)
			{
				var existing = existingCompanies.FirstOrDefault(c => c.ExternalId == company.ExternalId);

				if (existing == null)
				{
					await _context.Companies.AddAsync(company);
				}
				else
				{
					existing.Name = company.Name;
					existing.UpdatedAt = DateTime.UtcNow;
					_context.Companies.Update(existing);
				}
			}
			await _context.SaveChangesAsync();
		}
	}
}
