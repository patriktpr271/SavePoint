using SavePoint.BusinessLogic.Services.Interfaces;

namespace SavePoint.Host.Services
{
    /// <summary>
    /// Background service that initializes Hangfire recurring jobs on application startup
    /// </summary>
    public class JobInitializationService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobInitializationService> _logger;

        public JobInitializationService(IServiceProvider serviceProvider, ILogger<JobInitializationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting job initialization service...");

            try
            {
                // Use a scope to resolve scoped services
                using var scope = _serviceProvider.CreateScope();
                var backgroundJobService = scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();
                
                // Initialize recurring jobs
                await backgroundJobService.SetupRecurringJobsAsync();
                
                _logger.LogInformation("Recurring jobs initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize recurring jobs");
                // Don't throw - let the application continue to start
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Job initialization service stopping...");
            return Task.CompletedTask;
        }
    }
}