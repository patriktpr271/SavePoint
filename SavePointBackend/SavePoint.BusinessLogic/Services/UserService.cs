using Microsoft.AspNetCore.Identity;
using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.Common.Dtos.Users;
using SavePoint.Entities.Users;
using AutoMapper;

namespace SavePoint.BusinessLogic.Services
{
	public class UserService : IUserService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IMapper _mapper;
		private readonly IUserListService _userListService;

		public UserService(UserManager<ApplicationUser> userManager, IMapper mapper, IUserListService userListService)
		{
			_userManager = userManager;
			_mapper = mapper;
			_userListService = userListService;
		}

		public async Task<IdentityResult> RegisterAsync(RegisterDto dto)
		{
			var user = _mapper.Map<ApplicationUser>(dto);
			var result = await _userManager.CreateAsync(user, dto.Password);
			
			// If user creation was successful, create default lists
			if (result.Succeeded)
			{
				await _userListService.CreateDefaultListsForUserAsync(user.Id);
			}
			
			return result;
		}

		public async Task<(bool Success, ApplicationUser? User)> ValidateUserAsync(LoginDto dto)
		{
			// First try to find by email
			var user = await _userManager.FindByEmailAsync(dto.EmailOrUsername);
			
			// If not found by email, try to find by username
			if (user == null)
			{
				user = await _userManager.FindByNameAsync(dto.EmailOrUsername);
			}

			if (user != null && await _userManager.CheckPasswordAsync(user, dto.Password))
			{
				return (true, user);
			}
			return (false, null);
		}

		public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
		{
			return await _userManager.FindByIdAsync(userId);
		}

		public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
		{
			return await _userManager.FindByEmailAsync(email);
		}
	}
}
