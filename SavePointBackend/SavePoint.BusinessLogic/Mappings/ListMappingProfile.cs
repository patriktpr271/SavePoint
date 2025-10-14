using AutoMapper;
using SavePoint.Common.Dtos.Games;
using SavePoint.Common.Dtos.Lists;
using SavePoint.Entities.Lists;

namespace SavePoint.BusinessLogic.Mappings
{
	public class ListMappingProfile : Profile
	{
		public ListMappingProfile()
		{
			// Entity to DTO mappings
			CreateMap<UserList, UserListDto>()
				.ForMember(dest => dest.UserDisplayName, opt => opt.MapFrom(src => src.User.DisplayName))
				.ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
				.ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.UserListItems.Count))
				.ForMember(dest => dest.Games, opt => opt.MapFrom(src => src.UserListItems.Select(uli => uli.Game).ToList()))
				.ForMember(dest => dest.CurrentUserVote, opt => opt.Ignore()); // Set manually in service

			// Input DTO to Entity mappings
			CreateMap<CreateUserListDto, UserList>()
				.ForMember(dest => dest.Id, opt => opt.Ignore())
				.ForMember(dest => dest.UserId, opt => opt.Ignore())
				.ForMember(dest => dest.User, opt => opt.Ignore())
				.ForMember(dest => dest.IsDefault, opt => opt.Ignore())
				.ForMember(dest => dest.DefaultListType, opt => opt.Ignore())
				.ForMember(dest => dest.VoteScore, opt => opt.Ignore())
				.ForMember(dest => dest.UpvoteCount, opt => opt.Ignore())
				.ForMember(dest => dest.DownvoteCount, opt => opt.Ignore())
				.ForMember(dest => dest.UserListItems, opt => opt.Ignore())
				.ForMember(dest => dest.Votes, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
				.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
		}
	}
}