using AutoMapper;
using SavePoint.Common.Dtos.Games;
using SavePoint.Entities.Games;

namespace SavePoint.BusinessLogic.Mappings
{
    public class GameMappingProfile : Profile
    {
        public GameMappingProfile()
        {
            // Entity to DTO mappings
            CreateMap<Game, GameCardDto>()
                .ForMember(dest => dest.PopularityScore, opt => opt.Ignore()); // Handle separately in service

            CreateMap<Game, GameDetailDto>()
                .IncludeBase<Game, GameCardDto>() // Inherit base mapping
                .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Reviews.Count))
                .ForMember(dest => dest.AverageUserRating, opt => opt.MapFrom(src => 
                    src.Reviews.Any() ? src.Reviews.Average(r => r.Rating) : 0.0))
                .ForMember(dest => dest.Genres, opt => opt.MapFrom(src =>
                    src.GameGenres.Select(gg => new GenreDto 
                    { 
                        Id = gg.Genre.Id, 
                        Name = gg.Genre.Name 
                    }).ToList()))
                .ForMember(dest => dest.Platforms, opt => opt.MapFrom(src =>
                    src.GamePlatforms.Select(gp => new PlatformDto 
                    { 
                        Id = gp.Platform.Id, 
                        Name = gp.Platform.Name 
                    }).ToList()))
                .ForMember(dest => dest.Companies, opt => opt.MapFrom(src =>
                    src.GameCompanies.Select(gc => new CompanyDto 
                    { 
                        Id = gc.Company.Id, 
                        Name = gc.Company.Name,
                        Role = gc.Role.ToString()
                    }).ToList()));

            // Note: Input DTO to Entity mappings would go here for create/update operations
            // Example (for future use):
            // CreateMap<CreateGameDto, Game>()
            //     .ForMember(dest => dest.Id, opt => opt.Ignore())
            //     .ForMember(dest => dest.GameGenres, opt => opt.Ignore()) // Handle separately
            //     .ForMember(dest => dest.GamePlatforms, opt => opt.Ignore());
        }
    }
}