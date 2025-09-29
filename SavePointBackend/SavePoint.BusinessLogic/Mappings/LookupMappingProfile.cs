using AutoMapper;
using SavePoint.Common.Dtos.Lookups;
using SavePoint.Entities.Games;

namespace SavePoint.BusinessLogic.Mappings
{
    public class LookupMappingProfile : Profile
    {
        public LookupMappingProfile()
        {
            // Entity to DTO mappings
            CreateMap<Genre, GenreDto>();
            
            CreateMap<Platform, PlatformDto>();
            
            CreateMap<Company, CompanyDto>();
        }
    }
}