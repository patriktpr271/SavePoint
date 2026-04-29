using Microsoft.EntityFrameworkCore;
using SavePoint.DAL.Contexts;
using SavePoint.Host.Services;

namespace SavePoint.Host.Configuration
{
    public static class DatabaseConfiguration
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sql =>
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null)));

            services.AddScoped<DatabaseSeederService>();
            return services;
        }

        public static async Task<WebApplication> SeedDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync(); // ensures SavePoint DB is created + migrations applied

            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeederService>();
            await seeder.SeedAsync();

            return app;
        }
    }
}