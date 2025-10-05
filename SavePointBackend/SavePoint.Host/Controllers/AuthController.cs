using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.Common.Dtos.Users;
using SavePoint.Entities.Users;

namespace SavePoint.Host.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(
            IUserService userService, 
            SignInManager<ApplicationUser> signInManager, 
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userService = userService;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.RegisterAsync(dto);
            
            if (result.Succeeded)
            {
                // Add new users to the "User" role by default
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user != null)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }
                
                return Ok(new { message = "Registration successful" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Find user by email or username
            ApplicationUser? user = null;
            
            // First try to find by email
            user = await _userManager.FindByEmailAsync(dto.EmailOrUsername);
            
            // If not found by email, try to find by username
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(dto.EmailOrUsername);
            }

            if (user == null)
            {
                return BadRequest(new { message = "Invalid email/username or password" });
            }

            // Use the found user's UserName for sign-in
            var result = await _signInManager.PasswordSignInAsync(user.UserName!, dto.Password, dto.RememberMe, lockoutOnFailure: false);
            
            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                return Ok(new 
                { 
                    message = "Login successful",
                    user = new 
                    {
                        id = user.Id,
                        username = user.UserName,
                        email = user.Email,
                        displayName = user.DisplayName,
                        roles = roles
                    }
                });
            }

            if (result.IsLockedOut)
            {
                return BadRequest(new { message = "Account is locked out" });
            }

            if (result.IsNotAllowed)
            {
                return BadRequest(new { message = "Sign in not allowed. Please confirm your email or phone number." });
            }

            if (result.RequiresTwoFactor)
            {
                return BadRequest(new { message = "Two-factor authentication required" });
            }

            return BadRequest(new { message = "Invalid email/username or password" });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logout successful" });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return Unauthorized();
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new 
            {
                id = user.Id,
                username = user.UserName,
                email = user.Email,
                displayName = user.DisplayName,
                bio = user.Bio,
                roles = roles
            });
        }

        #region Admin Role Management

        [HttpPost("assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(dto.Role))
            {
                return BadRequest(new { message = $"Role '{dto.Role}' does not exist" });
            }

            // Check if user already has the role
            if (await _userManager.IsInRoleAsync(user, dto.Role))
            {
                return BadRequest(new { message = $"User already has the '{dto.Role}' role" });
            }

            var result = await _userManager.AddToRoleAsync(user, dto.Role);
            if (result.Succeeded)
            {
                return Ok(new { message = $"Role '{dto.Role}' assigned to user {user.UserName}" });
            }

            return BadRequest(new { message = "Failed to assign role", errors = result.Errors });
        }

        [HttpPost("remove-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Check if user has the role
            if (!await _userManager.IsInRoleAsync(user, dto.Role))
            {
                return BadRequest(new { message = $"User does not have the '{dto.Role}' role" });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, dto.Role);
            if (result.Succeeded)
            {
                return Ok(new { message = $"Role '{dto.Role}' removed from user {user.UserName}" });
            }

            return BadRequest(new { message = "Failed to remove role", errors = result.Errors });
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            var userList = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new
                {
                    id = user.Id,
                    username = user.UserName,
                    email = user.Email,
                    displayName = user.DisplayName,
                    roles = roles
                });
            }

            return Ok(userList);
        }

        [HttpGet("roles")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllRoles()
        {
            var roles = _roleManager.Roles.Select(r => new { id = r.Id, name = r.Name }).ToList();
            return Ok(roles);
        }

        #endregion

        #region Convenience Methods for Quick Admin Management

        [HttpPost("make-admin/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MakeUserAdmin(string userId)
        {
            var dto = new AssignRoleDto { UserId = userId, Role = "Admin" };
            return await AssignRole(dto);
        }

        [HttpPost("remove-admin/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveUserFromAdmin(string userId)
        {
            var dto = new AssignRoleDto { UserId = userId, Role = "Admin" };
            return await RemoveRole(dto);
        }

        #endregion
    }
}