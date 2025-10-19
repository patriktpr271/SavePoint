using Microsoft.EntityFrameworkCore;
using SavePoint.DAL.Contexts;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Jobs;

namespace SavePoint.DAL.Repositories
{
    /// <summary>
    /// Repository implementation for managing import job runs
    /// </summary>
    public class ImportJobRunRepository : IImportJobRunRepository
    {
        private readonly ApplicationDbContext _context;

        public ImportJobRunRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ImportJobRun>> GetAllAsync()
        {
            return await _context.ImportJobRuns
                .OrderByDescending(ijr => ijr.StartedAt)
                .ToListAsync();
        }

        public async Task<ImportJobRun?> GetByIdAsync(Guid id)
        {
            return await _context.ImportJobRuns
                .FirstOrDefaultAsync(ijr => ijr.Id == id);
        }

        public async Task<IEnumerable<ImportJobRun>> GetByJobTypeAsync(string jobType)
        {
            return await _context.ImportJobRuns
                .Where(ijr => ijr.JobType == jobType)
                .OrderByDescending(ijr => ijr.StartedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ImportJobRun>> GetByStatusAsync(ImportJobStatus status)
        {
            return await _context.ImportJobRuns
                .Where(ijr => ijr.Status == status)
                .OrderByDescending(ijr => ijr.StartedAt)
                .ToListAsync();
        }

        public async Task<ImportJobRun?> GetLatestByJobTypeAsync(string jobType)
        {
            return await _context.ImportJobRuns
                .Where(ijr => ijr.JobType == jobType)
                .OrderByDescending(ijr => ijr.StartedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<ImportJobRun?> GetLastSuccessfulByJobTypeAsync(string jobType)
        {
            return await _context.ImportJobRuns
                .Where(ijr => ijr.JobType == jobType && ijr.Status == ImportJobStatus.Completed && string.IsNullOrEmpty(ijr.ErrorMessage))
                .OrderByDescending(ijr => ijr.StartedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ImportJobRun>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.ImportJobRuns
                .Where(ijr => ijr.StartedAt >= startDate && ijr.StartedAt <= endDate)
                .OrderByDescending(ijr => ijr.StartedAt)
                .ToListAsync();
        }

        public async Task<ImportJobRun> CreateAsync(ImportJobRun jobRun)
        {
            jobRun.CreatedAt = DateTime.UtcNow;
            jobRun.UpdatedAt = DateTime.UtcNow;
            
            _context.ImportJobRuns.Add(jobRun);
            await _context.SaveChangesAsync();
            return jobRun;
        }

        public async Task<ImportJobRun> UpdateAsync(ImportJobRun jobRun)
        {
            jobRun.UpdatedAt = DateTime.UtcNow;
            
            _context.ImportJobRuns.Update(jobRun);
            await _context.SaveChangesAsync();
            return jobRun;
        }

        public async Task DeleteAsync(Guid id)
        {
            var jobRun = await GetByIdAsync(id);
            if (jobRun != null)
            {
                _context.ImportJobRuns.Remove(jobRun);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteOldJobRunsAsync(DateTime olderThan)
        {
            var oldJobRuns = await _context.ImportJobRuns
                .Where(ijr => ijr.StartedAt < olderThan)
                .ToListAsync();

            _context.ImportJobRuns.RemoveRange(oldJobRuns);
            await _context.SaveChangesAsync();
        }
    }
}