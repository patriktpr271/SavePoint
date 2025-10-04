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

		public UserService(UserManager<ApplicationUser> userManager, IMapper mapper)
		{
			_userManager = userManager;
			_mapper = mapper;
		}

		public async Task<IdentityResult> RegisterAsync(RegisterDto dto)
		{
			var user = _mapper.Map<ApplicationUser>(dto);
			return await _userManager.CreateAsync(user, dto.Password);
		}

		public async Task<(bool Success, ApplicationUser? User)> ValidateUserAsync(LoginDto dto)
		{
			var user = await _userManager.FindByEmailAsync(dto.Email);
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
