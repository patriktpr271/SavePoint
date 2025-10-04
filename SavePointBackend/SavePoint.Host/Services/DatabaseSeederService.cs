using Microsoft.AspNetCore.Identity;
using SavePoint.Entities.Users;

namespace SavePoint.Host.Services
{
    public class DatabaseSeederService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DatabaseSeederService> _logger;

        public DatabaseSeederService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<DatabaseSeederService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            // Create roles if they don't exist
            await CreateRoleIfNotExists("Admin");
            await CreateRoleIfNotExists("User");

            // Create default admin user if it doesn't exist
            await CreateDefaultAdminUser();
        }

        private async Task CreateRoleIfNotExists(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole(roleName);
                var result = await _roleManager.CreateAsync(role);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Role '{roleName}' created successfully.");
                }
                else
                {
                    _logger.LogError($"Error creating role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        private async Task CreateDefaultAdminUser()
        {
            const string adminEmail = "admin@savepoint.com";
            const string adminPassword = "Admin123!";
            const string adminUserName = "admin";
            const string adminDisplayName = "System Administrator";

            var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    DisplayName = adminDisplayName,
                    EmailConfirmed = true,
                    Bio = "Default system administrator account"
                };

                var result = await _userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation($"Default admin user created with email: {adminEmail}");
                    _logger.LogWarning($"IMPORTANT: Change the default admin password! Email: {adminEmail}, Password: {adminPassword}");
                }
                else
                {
                    _logger.LogError($"Error creating default admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}