using Microsoft.AspNetCore.Identity;
using SavePoint.Common.Dtos.Users;

namespace SavePoint.BusinessLogic.Services.Interfaces
{
	public interface IUserService
	{
		Task<IdentityResult> RegisterAsync(RegisterDto dto);
	}
}
