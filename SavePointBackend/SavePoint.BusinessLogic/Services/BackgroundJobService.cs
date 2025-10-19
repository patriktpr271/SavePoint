using Hangfire;
using Microsoft.Extensions.Logging;
using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Jobs;
using System.Text.Json;
using System.ComponentModel;

namespace SavePoint.BusinessLogic.Services
{
    /// <summary>
    /// Service for managing background import jobs using Hangfire
    /// </summary>
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IIGDBImportService _importService;
        private readonly IImportJobRunRepository _jobRunRepository;
        private readonly IImportStatisticsRepository _statisticsRepository;
        private readonly ILogger<BackgroundJobService> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;

        // Job type constants
        private const string JOB_TYPE_GAMES_WITH_POPULARITY = "GamesWithPopularity";
        private const string JOB_TYPE_BASE_DATA = "BaseData";
        private const string JOB_TYPE_FULL_SYNC = "FullDatabaseSync";

        public BackgroundJobService(
            IIGDBImportService importService,
            IImportJobRunRepository jobRunRepository,
            IImportStatisticsRepository statisticsRepository,
            ILogger<BackgroundJobService> logger,
            IBackgroundJobClient backgroundJobClient,
            IRecurringJobManager recurringJobManager)
        {
            _importService = importService;
            _jobRunRepository = jobRunRepository;
            _statisticsRepository = statisticsRepository;
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
        }

        public async Task SetupRecurringJobsAsync()
        {
            _logger.LogInformation("Setting up recurring import jobs");

            // Clear any existing recurring jobs to avoid duplicates
            RecurringJob.RemoveIfExists("import-games");
            RecurringJob.RemoveIfExists("import-genres");
            RecurringJob.RemoveIfExists("import-companies");
            RecurringJob.RemoveIfExists("import-platforms");
            RecurringJob.RemoveIfExists("import-popularity");
            RecurringJob.RemoveIfExists("full-import-weekly");
            RecurringJob.RemoveIfExists("weekly-incremental-games");
            RecurringJob.RemoveIfExists("weekly-incremental-genres");
            RecurringJob.RemoveIfExists("weekly-incremental-companies");
            RecurringJob.RemoveIfExists("weekly-incremental-platforms");
            RecurringJob.RemoveIfExists("weekly-incremental-popularity");

            // Initialize default statistics if they don't exist
            await InitializeDefaultStatistics();

            // 1. Weekly incremental games with popularity import - Every Monday at 2 AM UTC
            _logger.LogInformation("Setting up weekly incremental games with popularity import");
            _recurringJobManager.AddOrUpdate<BackgroundJobService>(
                "weekly-games-with-popularity",
                service => service.ExecuteIncrementalGamesWithPopularityImport(),
                "0 2 * * 1", // Every Monday at 2 AM UTC
                TimeZoneInfo.Utc,
                "import"
            );

            // 2. Weekly incremental base data import - Every Monday at 1 AM UTC (runs before games)
            _logger.LogInformation("Setting up weekly incremental base data import");
            _recurringJobManager.AddOrUpdate<BackgroundJobService>(
                "weekly-base-data",
                service => service.ExecuteIncrementalBaseDataImport(),
                "0 1 * * 1", // Every Monday at 1 AM UTC (before games)
                TimeZoneInfo.Utc,
                "import"
            );

            // 3. Full database sync - Every 6 months on the 1st at 3 AM UTC
            _logger.LogInformation("Setting up biannual full database sync");
            _recurringJobManager.AddOrUpdate<BackgroundJobService>(
                "biannual-full-sync",
                service => service.ExecuteFullDatabaseSync(),
                "0 3 1 1,7 *", // January 1st and July 1st at 3 AM UTC
                TimeZoneInfo.Utc,
                "import"
            );

            _logger.LogInformation("Recurring jobs setup completed successfully");
        }

        public async Task<object> GetJobStatusAsync()
        {
            var recentJobs = await _jobRunRepository.GetByDateRangeAsync(
                DateTime.UtcNow.AddDays(-7), 
                DateTime.UtcNow
            );

            var runningJobs = await _jobRunRepository.GetByStatusAsync(ImportJobStatus.Running);
            var queuedJobs = await _jobRunRepository.GetByStatusAsync(ImportJobStatus.Queued);

            return new
            {
                RecentJobs = recentJobs.Take(10).Select(job => new
                {
                    job.Id,
                    job.JobType,
                    job.Status,
                    job.StartedAt,
                    job.CompletedAt,
                    job.Duration,
                    job.RecordsProcessed,
                    job.RecordsAdded,
                    job.RecordsUpdated,
                    job.RecordsFailed,
                    job.ErrorMessage,
                    job.IsSuccessful
                }),
                RunningJobs = runningJobs.Select(job => new
                {
                    job.Id,
                    job.JobType,
                    job.StartedAt,
                    job.RecordsProcessed
                }),
                QueuedJobs = queuedJobs.Count(),
                TotalJobsLastWeek = recentJobs.Count(),
                SuccessfulJobsLastWeek = recentJobs.Count(j => j.IsSuccessful),
                FailedJobsLastWeek = recentJobs.Count(j => j.Status == ImportJobStatus.Failed)
            };
        }

        public async Task<object> GetImportStatisticsAsync()
        {
            var stats = await _statisticsRepository.GetAllAsync();
            
            return new
            {
                Statistics = stats.Select(s => new
                {
                    s.DataType,
                    s.TotalRecords,
                    s.LastSuccessfulImport,
                    s.LastIGDBUpdateDate,
                    s.NextScheduledImport,
                    s.AutoImportEnabled,
                    s.ImportFrequencyDays,
                    s.AverageImportDuration,
                    s.SuccessfulImports,
                    s.FailedImports,
                    SuccessRate = s.SuccessfulImports + s.FailedImports > 0 
                        ? (double)s.SuccessfulImports / (s.SuccessfulImports + s.FailedImports) * 100 
                        : 0
                }),
                LastUpdate = DateTime.UtcNow
            };
        }

        // Background job execution methods (these will be called by Hangfire)

        [AutomaticRetry(Attempts = 2)]
        [DisplayName("?? Incremental Games with Popularity Import")]
        public async Task ExecuteIncrementalGamesWithPopularityImport()
        {
            var jobRun = await StartJobRun(JOB_TYPE_GAMES_WITH_POPULARITY);
            
            try
            {
                _logger.LogInformation("Starting incremental games with popularity import job {JobId}", jobRun.Id);
                
                // Get last successful import to determine what to import
                var lastSuccessful = await _jobRunRepository.GetLastSuccessfulByJobTypeAsync(JOB_TYPE_GAMES_WITH_POPULARITY);
                var lastUpdateDate = lastSuccessful?.LastUpdateDate ?? DateTime.UtcNow.AddDays(-7); // Default to 1 week ago
                
                jobRun.LastUpdateDate = lastUpdateDate;
                jobRun.Metadata = JsonSerializer.Serialize(new { IncrementalFrom = lastUpdateDate });
                await _jobRunRepository.UpdateAsync(jobRun);

                await ExecuteImportStep(jobRun, "Games with Popularity (Incremental)", 
                    async () => await _importService.ImportGamesIncrementalWithPopularityAsync(lastUpdateDate));

                await CompleteJobRun(jobRun, true);
                _logger.LogInformation("Incremental games with popularity import job {JobId} completed successfully", jobRun.Id);
            }
            catch (Exception ex)
            {
                await CompleteJobRun(jobRun, false, ex.Message, ex.ToString());
                _logger.LogError(ex, "Incremental games with popularity import job {JobId} failed", jobRun.Id);
                throw;
            }
        }

        [AutomaticRetry(Attempts = 2)]
        [DisplayName("?? Incremental Base Data Import")]
        public async Task ExecuteIncrementalBaseDataImport()
        {
            var jobRun = await StartJobRun(JOB_TYPE_BASE_DATA);
            
            try
            {
                _logger.LogInformation("Starting incremental base data import job {JobId}", jobRun.Id);
                
                // Get last successful import to determine what to import
                var lastSuccessful = await _jobRunRepository.GetLastSuccessfulByJobTypeAsync(JOB_TYPE_BASE_DATA);
                var lastUpdateDate = lastSuccessful?.LastUpdateDate ?? DateTime.UtcNow.AddDays(-7); // Default to 1 week ago
                
                jobRun.LastUpdateDate = lastUpdateDate;
                jobRun.Metadata = JsonSerializer.Serialize(new { IncrementalFrom = lastUpdateDate });
                await _jobRunRepository.UpdateAsync(jobRun);

                await ExecuteImportStep(jobRun, "Base Data (Incremental)", 
                    async () => await _importService.ImportBaseDataIncrementalAsync(lastUpdateDate));

                await CompleteJobRun(jobRun, true);
                _logger.LogInformation("Incremental base data import job {JobId} completed successfully", jobRun.Id);
            }
            catch (Exception ex)
            {
                await CompleteJobRun(jobRun, false, ex.Message, ex.ToString());
                _logger.LogError(ex, "Incremental base data import job {JobId} failed", jobRun.Id);
                throw;
            }
        }

        [AutomaticRetry(Attempts = 2)]
        [DisplayName("?? Full Database Sync")]
        public async Task ExecuteFullDatabaseSync()
        {
            var jobRun = await StartJobRun(JOB_TYPE_FULL_SYNC);
            
            try
            {
                _logger.LogInformation("Starting full database sync job {JobId}", jobRun.Id);

                await ExecuteImportStep(jobRun, "Full Database Sync (All Missing Games)", 
                    async () => await _importService.ImportAllGamesFullSyncAsync());

                await CompleteJobRun(jobRun, true);
                _logger.LogInformation("Full database sync job {JobId} completed successfully", jobRun.Id);
            }
            catch (Exception ex)
            {
                await CompleteJobRun(jobRun, false, ex.Message, ex.ToString());
                _logger.LogError(ex, "Full database sync job {JobId} failed", jobRun.Id);
                throw;
            }
        }

        // Manual job methods (can be triggered from Hangfire dashboard)

        /// <summary>
        /// MANUAL JOB: Incremental import for all games with popularity data
        /// This job can be manually triggered from Hangfire dashboard
        /// </summary>
        [AutomaticRetry(Attempts = 1)]
        [DisplayName("?? MANUAL: Incremental Games with Popularity Import")]
        public async Task ManualIncrementalGamesWithPopularityImport()
        {
            await ExecuteIncrementalGamesWithPopularityImport();
        }

        /// <summary>
        /// MANUAL JOB: Incremental import for base data (genres, companies, platforms)
        /// This job can be manually triggered from Hangfire dashboard
        /// </summary>
        [AutomaticRetry(Attempts = 1)]
        [DisplayName("?? MANUAL: Incremental Base Data Import")]
        public async Task ManualIncrementalBaseDataImport()
        {
            await ExecuteIncrementalBaseDataImport();
        }

        /// <summary>
        /// MANUAL JOB: One-time full database sync
        /// This job can be manually triggered from Hangfire dashboard
        /// Use this ONCE to import all games that don't exist in database
        /// </summary>
        [AutomaticRetry(Attempts = 1)]
        [DisplayName("?? MANUAL: Full Database Sync (One-Time Only)")]
        public async Task ManualFullDatabaseSync()
        {
            await ExecuteFullDatabaseSync();
        }

        // Helper methods

        private async Task<ImportJobRun> StartJobRun(string jobType)
        {
            var jobRun = new ImportJobRun
            {
                Id = Guid.NewGuid(),
                JobType = jobType,
                Status = ImportJobStatus.Running,
                StartedAt = DateTime.UtcNow
            };

            return await _jobRunRepository.CreateAsync(jobRun);
        }

        private async Task CompleteJobRun(ImportJobRun jobRun, bool success, string? errorMessage = null, string? errorDetails = null)
        {
            jobRun.Status = success ? ImportJobStatus.Completed : ImportJobStatus.Failed;
            jobRun.CompletedAt = DateTime.UtcNow;
            jobRun.ErrorMessage = errorMessage;
            jobRun.ErrorDetails = errorDetails;

            await _jobRunRepository.UpdateAsync(jobRun);

            // Update statistics
            var duration = jobRun.Duration ?? TimeSpan.Zero;
            await _statisticsRepository.UpdateImportStatsAsync(jobRun.JobType, duration, success);

            if (success)
            {
                await _statisticsRepository.UpdateLastSuccessfulImportAsync(jobRun.JobType, DateTime.UtcNow);
            }
        }

        private async Task ExecuteImportStep(ImportJobRun jobRun, string stepName, Func<Task> importAction)
        {
            _logger.LogInformation("Executing step: {StepName} for job {JobId}", stepName, jobRun.Id);
            
            var stepStart = DateTime.UtcNow;
            await importAction();
            var stepDuration = DateTime.UtcNow - stepStart;
            
            // Update job progress
            jobRun.RecordsProcessed += 1; // Simple increment for now
            jobRun.Metadata = JsonSerializer.Serialize(new { 
                CurrentStep = stepName, 
                StepDuration = stepDuration,
                LastUpdated = DateTime.UtcNow 
            });
            
            await _jobRunRepository.UpdateAsync(jobRun);
            
            _logger.LogInformation("Completed step: {StepName} in {Duration}", stepName, stepDuration);
        }

        private async Task InitializeDefaultStatistics()
        {
            var dataTypes = new[] { JOB_TYPE_GAMES_WITH_POPULARITY, JOB_TYPE_BASE_DATA, JOB_TYPE_FULL_SYNC };
            
            foreach (var dataType in dataTypes)
            {
                var existing = await _statisticsRepository.GetByDataTypeAsync(dataType);
                if (existing == null)
                {
                    var stats = new ImportStatistics
                    {
                        Id = Guid.NewGuid(),
                        DataType = dataType,
                        TotalRecords = 0,
                        AutoImportEnabled = dataType != JOB_TYPE_FULL_SYNC, // Full sync is manual only
                        ImportFrequencyDays = dataType == JOB_TYPE_FULL_SYNC ? 0 : 7, // Full sync doesn't repeat
                        SuccessfulImports = 0,
                        FailedImports = 0
                    };
                    
                    await _statisticsRepository.CreateOrUpdateAsync(stats);
                }
            }
        }
    }
}