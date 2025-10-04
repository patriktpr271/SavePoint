using Microsoft.AspNetCore.Identity;
using SavePoint.Common.Dtos.Users;
using SavePoint.Entities.Users;

namespace SavePoint.BusinessLogic.Services.Interfaces
{
	public interface IUserService
	{
		Task<IdentityResult> RegisterAsync(RegisterDto dto);
		Task<(bool Success, ApplicationUser? User)> ValidateUserAsync(LoginDto dto);
		Task<ApplicationUser?> GetUserByIdAsync(string userId);
		Task<ApplicationUser?> GetUserByEmailAsync(string email);
	}
}
