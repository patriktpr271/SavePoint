using AutoMapper;
using SavePoint.Common.Dtos.Users;
using SavePoint.Entities.Users;

namespace SavePoint.BusinessLogic.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            // Input DTO to Entity mappings
            CreateMap<RegisterDto, ApplicationUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Let Identity handle ID generation
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.LockoutEnabled, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.AccessFailedCount, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.UserLists, opt => opt.Ignore());

            // Entity to DTO mappings (for future use)
            CreateMap<ApplicationUser, UserProfileDto>()
                .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Reviews.Count))
                .ForMember(dest => dest.ListCount, opt => opt.MapFrom(src => src.UserLists.Count));

            // Note: Password is handled separately by Identity framework
            // Never map passwords in AutoMapper profiles for security reasons
        }
    }

    // Future DTO for user profile display
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public int ReviewCount { get; set; }
        public int ListCount { get; set; }
    }
}