using Microsoft.EntityFrameworkCore;
using SavePoint.DAL.Contexts;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Jobs;

namespace SavePoint.DAL.Repositories
{
    /// <summary>
    /// Repository implementation for managing import statistics
    /// </summary>
    public class ImportStatisticsRepository : IImportStatisticsRepository
    {
        private readonly ApplicationDbContext _context;

        public ImportStatisticsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ImportStatistics>> GetAllAsync()
        {
            return await _context.ImportStatistics
                .OrderBy(ist => ist.DataType)
                .ToListAsync();
        }

        public async Task<ImportStatistics?> GetByDataTypeAsync(string dataType)
        {
            return await _context.ImportStatistics
                .FirstOrDefaultAsync(ist => ist.DataType == dataType);
        }

        public async Task<ImportStatistics?> GetByIdAsync(Guid id)
        {
            return await _context.ImportStatistics
                .FirstOrDefaultAsync(ist => ist.Id == id);
        }

        public async Task<ImportStatistics> CreateOrUpdateAsync(ImportStatistics statistics)
        {
            var existing = await GetByDataTypeAsync(statistics.DataType);
            
            if (existing != null)
            {
                // Update existing
                existing.TotalRecords = statistics.TotalRecords;
                existing.LastSuccessfulImport = statistics.LastSuccessfulImport;
                existing.LastIGDBUpdateDate = statistics.LastIGDBUpdateDate;
                existing.NextScheduledImport = statistics.NextScheduledImport;
                existing.AutoImportEnabled = statistics.AutoImportEnabled;
                existing.ImportFrequencyDays = statistics.ImportFrequencyDays;
                existing.AverageImportDuration = statistics.AverageImportDuration;
                existing.SuccessfulImports = statistics.SuccessfulImports;
                existing.FailedImports = statistics.FailedImports;
                existing.Configuration = statistics.Configuration;
                existing.UpdatedAt = DateTime.UtcNow;
                
                _context.ImportStatistics.Update(existing);
                await _context.SaveChangesAsync();
                return existing;
            }
            else
            {
                // Create new
                statistics.CreatedAt = DateTime.UtcNow;
                statistics.UpdatedAt = DateTime.UtcNow;
                
                _context.ImportStatistics.Add(statistics);
                await _context.SaveChangesAsync();
                return statistics;
            }
        }

        public async Task UpdateRecordCountAsync(string dataType, int totalRecords)
        {
            var stats = await GetByDataTypeAsync(dataType);
            if (stats != null)
            {
                stats.TotalRecords = totalRecords;
                stats.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateLastSuccessfulImportAsync(string dataType, DateTime importDate)
        {
            var stats = await GetByDataTypeAsync(dataType);
            if (stats != null)
            {
                stats.LastSuccessfulImport = importDate;
                stats.NextScheduledImport = importDate.AddDays(stats.ImportFrequencyDays);
                stats.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateLastIGDBUpdateDateAsync(string dataType, DateTime updateDate)
        {
            var stats = await GetByDataTypeAsync(dataType);
            if (stats != null)
            {
                stats.LastIGDBUpdateDate = updateDate;
                stats.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateImportStatsAsync(string dataType, TimeSpan duration, bool success)
        {
            var stats = await GetByDataTypeAsync(dataType);
            if (stats != null)
            {
                // Update average duration
                if (stats.AverageImportDuration.HasValue)
                {
                    var totalImports = stats.SuccessfulImports + stats.FailedImports;
                    var totalDurationTicks = stats.AverageImportDuration.Value.Ticks * totalImports;
                    stats.AverageImportDuration = new TimeSpan((totalDurationTicks + duration.Ticks) / (totalImports + 1));
                }
                else
                {
                    stats.AverageImportDuration = duration;
                }

                // Update counters
                if (success)
                {
                    stats.SuccessfulImports++;
                    stats.LastSuccessfulImport = DateTime.UtcNow;
                    stats.NextScheduledImport = DateTime.UtcNow.AddDays(stats.ImportFrequencyDays);
                }
                else
                {
                    stats.FailedImports++;
                }

                stats.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ToggleAutoImportAsync(string dataType, bool enabled)
        {
            var stats = await GetByDataTypeAsync(dataType);
            if (stats != null)
            {
                stats.AutoImportEnabled = enabled;
                stats.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var stats = await GetByIdAsync(id);
            if (stats != null)
            {
                _context.ImportStatistics.Remove(stats);
                await _context.SaveChangesAsync();
            }
        }
    }
}