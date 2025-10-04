using IGDB;

namespace SavePoint.Host.Configuration
{
    public static class ExternalServicesConfiguration
    {
        public static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register IGDB Client using configuration values
            services.AddSingleton<IGDBClient>(sp =>
            {
                var clientId = configuration["IGDB:ClientId"] 
                    ?? throw new InvalidOperationException("IGDB:ClientId is not configured");
                var accessToken = configuration["IGDB:AccessToken"] 
                    ?? throw new InvalidOperationException("IGDB:AccessToken is not configured");
                
                return new IGDBClient(clientId, accessToken);
            });

            return services;
        }
    }
}