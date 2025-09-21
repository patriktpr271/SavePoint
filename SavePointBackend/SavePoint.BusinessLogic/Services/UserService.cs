using Microsoft.AspNetCore.Identity;
using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.Entities.Dtos;
using SavePoint.Entities.Users;

namespace SavePoint.BusinessLogic.Services
{
	public class UserService : IUserService
	{
		private readonly UserManager<ApplicationUser> _userManager;

		public UserService(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task<IdentityResult> RegisterAsync(RegisterDto dto)
		{
			var user = new ApplicationUser
			{
				UserName = dto.UserName,
				Email = dto.Email,
				DisplayName = dto.DisplayName
			};
			return await _userManager.CreateAsync(user, dto.Password);
		}
	}
}
