using System.Text.Json.Serialization;

namespace SavePoint.Host.Configuration
{
    public static class WebApiConfiguration
    {
        public static IServiceCollection AddWebApiServices(this IServiceCollection services)
        {
            // Add controllers with JSON configuration
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            // Add API documentation
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            return services;
        }

        public static WebApplication ConfigureWebApiPipeline(this WebApplication app)
        {
            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // CORS middleware (should be before authentication)
            app.UseCors("AllowFrontend");

            // Only serve static files in production (when frontend is built into wwwroot)
            if (!app.Environment.IsDevelopment())
            {
                // Serve static files from wwwroot (for frontend in production)
                app.UseDefaultFiles();
                app.UseStaticFiles();
            }

            // Authentication & Authorization middleware (order is important!)
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Only use fallback to index.html in production (for SPA routing)
            if (!app.Environment.IsDevelopment())
            {
                // Fallback to index.html for SPA routing
                app.MapFallbackToFile("index.html");
            }

            return app;
        }
    }
}