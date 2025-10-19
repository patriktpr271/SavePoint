using SavePoint.BusinessLogic.Services;
using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.DAL.Repositories;
using SavePoint.DAL.Repositories.Interfaces;

namespace SavePoint.Host.Configuration
{
    public static class BusinessServicesConfiguration
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            // Register business services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IIGDBImportService, IGDBImportService>();
            services.AddScoped<IGameService, GameService>();
            services.AddScoped<ILookupService, LookupService>();
            services.AddScoped<IUserListService, UserListService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IBackgroundJobService, BackgroundJobService>();

            return services;
        }
    }
}