using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;

namespace SavePoint.Host.Configuration
{
    public static class HangfireConfiguration
    {
        /// <summary>
        /// Registers Hangfire services with SQL Server storage
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The application configuration</param>
        /// <returns>The configured service collection</returns>
        public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Add Hangfire services with SQL Server storage
            services.AddHangfire(config =>
                config
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true,
                        SchemaName = "HangFire"
                    }));

            // Add the processing server as hosted service
            services.AddHangfireServer(options =>
            {
                options.WorkerCount = Environment.ProcessorCount;
                options.Queues = new[] { "default", "import" };
            });

            // Add job initialization service
            services.AddHostedService<SavePoint.Host.Services.JobInitializationService>();

            return services;
        }

        /// <summary>
        /// Configure Hangfire dashboard and middleware
        /// </summary>
        /// <param name="app">The web application</param>
        /// <returns>The configured application</returns>
        public static WebApplication UseHangfireDashboard(this WebApplication app)
        {
            // Configure Hangfire Dashboard with authentication
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireDashboardAuthorizationFilter() },
                DashboardTitle = "SavePoint Background Jobs"
            });

            return app;
        }
    }

    /// <summary>
    /// Custom authorization filter for Hangfire dashboard
    /// Restricts access to authenticated admin users
    /// </summary>
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            
            // Check if user is authenticated and has Admin role
            return httpContext.User?.Identity?.IsAuthenticated == true && 
                   httpContext.User.IsInRole("Admin");
        }
    }
}