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

            // Authentication & Authorization middleware (order is important!)
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
    }
}