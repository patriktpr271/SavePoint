using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using SavePoint.Host.Middleware;
using SavePoint.Host.Filters;

namespace SavePoint.Host.Configuration
{
    public static class WebApiConfiguration
    {
        public static IServiceCollection AddWebApiServices(this IServiceCollection services)
        {
            // Add controllers with JSON configuration and filters
            services.AddControllers(options =>
            {
                // Add global model validation filter
                options.Filters.Add<ModelValidationFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            // Disable automatic model validation since we handle it in the filter
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // Add API documentation
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            return services;
        }

        public static WebApplication ConfigureWebApiPipeline(this WebApplication app)
        {
            // Global exception handling (should be one of the first middleware)
            app.UseGlobalExceptionHandling();

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
                app.UseHttpsRedirection();
            }

            // Authentication & Authorization middleware (order is important!)
            app.UseAuthentication();
            app.UseAuthorization();

            // Hangfire dashboard (after authentication for security)
            app.UseHangfireDashboard();

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