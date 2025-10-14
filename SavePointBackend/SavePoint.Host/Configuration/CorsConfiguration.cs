namespace SavePoint.Host.Configuration
{
    public static class CorsConfiguration
    {
        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    var allowedOrigins = new List<string>
                    {
                        "http://localhost:5173", 
                        "https://localhost:5173",
                        "https://localhost:7198",
                        "https://savepoint-gameaabaccb473.switzerlandnorth-01.azurewebsites.net"
                    };

                    // Add Azure Web App URL from configuration if available
                    var azureUrl = configuration["Azure:WebAppUrl"];
                    if (!string.IsNullOrEmpty(azureUrl))
                    {
                        allowedOrigins.Add(azureUrl);
                    }

                    policy.WithOrigins(allowedOrigins.ToArray())
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            return services;
        }
    }
}