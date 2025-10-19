using SavePoint.Entities.Jobs;

namespace SavePoint.DAL.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for managing import job runs
    /// </summary>
    public interface IImportJobRunRepository
    {
        /// <summary>
        /// Get all job runs
        /// </summary>
        Task<IEnumerable<ImportJobRun>> GetAllAsync();

        /// <summary>
        /// Get job run by ID
        /// </summary>
        Task<ImportJobRun?> GetByIdAsync(Guid id);

        /// <summary>
        /// Get job runs by type
        /// </summary>
        Task<IEnumerable<ImportJobRun>> GetByJobTypeAsync(string jobType);

        /// <summary>
        /// Get job runs by status
        /// </summary>
        Task<IEnumerable<ImportJobRun>> GetByStatusAsync(ImportJobStatus status);

        /// <summary>
        /// Get the most recent job run for a specific job type
        /// </summary>
        Task<ImportJobRun?> GetLatestByJobTypeAsync(string jobType);

        /// <summary>
        /// Get the last successful job run for a specific job type
        /// </summary>
        Task<ImportJobRun?> GetLastSuccessfulByJobTypeAsync(string jobType);

        /// <summary>
        /// Get job runs in a date range
        /// </summary>
        Task<IEnumerable<ImportJobRun>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Create a new job run
        /// </summary>
        Task<ImportJobRun> CreateAsync(ImportJobRun jobRun);

        /// <summary>
        /// Update an existing job run
        /// </summary>
        Task<ImportJobRun> UpdateAsync(ImportJobRun jobRun);

        /// <summary>
        /// Delete a job run
        /// </summary>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Delete old job runs (older than specified date)
        /// </summary>
        Task DeleteOldJobRunsAsync(DateTime olderThan);
    }
}