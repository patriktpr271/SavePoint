using Microsoft.EntityFrameworkCore;
using SavePoint.DAL.Contexts;
using SavePoint.Host.Services;

namespace SavePoint.Host.Configuration
{
    public static class DatabaseConfiguration
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure database
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Register DatabaseSeederService
            services.AddScoped<DatabaseSeederService>();

            return services;
        }

        public static async Task<WebApplication> SeedDatabaseAsync(this WebApplication app)
        {
            // Seed the database with roles and default admin user
            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeederService>();
                await seeder.SeedAsync();
            }

            return app;
        }
    }
}