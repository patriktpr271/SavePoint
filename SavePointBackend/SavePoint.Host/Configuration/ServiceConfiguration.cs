namespace SavePoint.Host.Configuration
{
    /// <summary>
    /// Main configuration class that orchestrates all service registrations
    /// </summary>
    public static class ServiceConfiguration
    {
        /// <summary>
        /// Registers all application services in the dependency injection container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The application configuration</param>
        /// <returns>The configured service collection</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Core Web API services
            services.AddWebApiServices();

            // Database and Entity Framework
            services.AddDatabaseServices(configuration);

            // Identity and Authentication
            services.AddIdentityServices();

            // CORS configuration
            services.AddCorsConfiguration(configuration);

            // AutoMapper profiles
            services.AddMappingProfiles();

            // Repository layer
            services.AddRepositories();

            // Business service layer
            services.AddBusinessServices();

            // External services (IGDB, etc.)
            services.AddExternalServices(configuration);

            // Hangfire background job processing
            services.AddHangfireServices(configuration);

            return services;
        }
    }
}