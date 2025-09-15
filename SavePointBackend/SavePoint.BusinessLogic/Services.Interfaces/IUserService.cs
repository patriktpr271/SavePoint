using Microsoft.AspNetCore.Identity;
using SavePoint.Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.BusinessLogic.Services.Interfaces
{
	public interface IUserService
	{
		Task<IdentityResult> RegisterAsync(RegisterDto dto);
	}
}
