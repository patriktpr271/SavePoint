using SavePoint.Entities.Jobs;

namespace SavePoint.DAL.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for managing import statistics
    /// </summary>
    public interface IImportStatisticsRepository
    {
        /// <summary>
        /// Get all import statistics
        /// </summary>
        Task<IEnumerable<ImportStatistics>> GetAllAsync();

        /// <summary>
        /// Get statistics by data type
        /// </summary>
        Task<ImportStatistics?> GetByDataTypeAsync(string dataType);

        /// <summary>
        /// Get statistics by ID
        /// </summary>
        Task<ImportStatistics?> GetByIdAsync(Guid id);

        /// <summary>
        /// Create or update statistics for a data type
        /// </summary>
        Task<ImportStatistics> CreateOrUpdateAsync(ImportStatistics statistics);

        /// <summary>
        /// Update record counts for a data type
        /// </summary>
        Task UpdateRecordCountAsync(string dataType, int totalRecords);

        /// <summary>
        /// Update last successful import date for a data type
        /// </summary>
        Task UpdateLastSuccessfulImportAsync(string dataType, DateTime importDate);

        /// <summary>
        /// Update last IGDB update date for incremental imports
        /// </summary>
        Task UpdateLastIGDBUpdateDateAsync(string dataType, DateTime updateDate);

        /// <summary>
        /// Update import statistics after a job completion
        /// </summary>
        Task UpdateImportStatsAsync(string dataType, TimeSpan duration, bool success);

        /// <summary>
        /// Toggle auto import for a data type
        /// </summary>
        Task ToggleAutoImportAsync(string dataType, bool enabled);

        /// <summary>
        /// Delete statistics for a data type
        /// </summary>
        Task DeleteAsync(Guid id);
    }
}