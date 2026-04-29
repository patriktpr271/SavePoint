using SavePoint.DAL.Repositories;
using SavePoint.DAL.Repositories.Interfaces;

namespace SavePoint.Host.Configuration
{
    public static class RepositoryConfiguration
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<IGenreRepository, GenreRepository>();
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IPlatfromRepository, PlatfromRepository>();
            services.AddScoped<IPopularityRepository, PopularityRepository>();
            services.AddScoped<IUserListRepository, UserListRepository>();
            services.AddScoped<IImportJobRunRepository, ImportJobRunRepository>();
            services.AddScoped<IImportStatisticsRepository, ImportStatisticsRepository>();

            return services;
        }
    }
}