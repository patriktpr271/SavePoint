namespace SavePoint.BusinessLogic.Services.Interfaces
{
    /// <summary>
    /// Service interface for managing background import jobs
    /// </summary>
    public interface IBackgroundJobService
    {
        /// <summary>
        /// Set up recurring import jobs (weekly incremental + biannual full sync)
        /// </summary>
        Task SetupRecurringJobsAsync();

        /// <summary>
        /// Get status of all import jobs
        /// </summary>
        Task<object> GetJobStatusAsync();

        /// <summary>
        /// Get import statistics dashboard data
        /// </summary>
        Task<object> GetImportStatisticsAsync();
    }
}