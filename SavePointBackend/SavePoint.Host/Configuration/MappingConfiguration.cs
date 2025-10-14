using SavePoint.BusinessLogic.Mappings;

namespace SavePoint.Host.Configuration
{
    public static class MappingConfiguration
    {
        public static IServiceCollection AddMappingProfiles(this IServiceCollection services)
        {
            // Add AutoMapper with all mapping profiles
            services.AddAutoMapper(
                typeof(GameMappingProfile),
                typeof(UserMappingProfile),
                typeof(LookupMappingProfile),
                typeof(ReviewMappingProfile));

            return services;
        }
    }
}