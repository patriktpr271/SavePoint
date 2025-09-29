using Microsoft.AspNetCore.Mvc;
using SavePoint.BusinessLogic.Services;
using SavePoint.Common.Dtos.Users;

namespace SavePoint.Host.Controllers
{
	[ApiController]
	[Route("/api/[controller]")]
	public class UserController : ControllerBase
	{
		private readonly UserService userService;

		public UserController(UserService userService)
		{
			this.userService = userService;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterDto dto)
		{
			var result = await userService.RegisterAsync(dto);
			if (result.Succeeded)
			{
				return Ok(new { Message = "User registered successfully" });
			}
			return BadRequest(result.Errors);
		}
	}
}
