using Microsoft.AspNetCore.Identity;
using SavePoint.IntegrationTests.Fixtures;
using SavePoint.BusinessLogic.Services;
using AutoMapper;
using SavePoint.BusinessLogic.Services.Interfaces;
using Moq;

namespace SavePoint.IntegrationTests.Services
{
    public class UserServiceIntegrationTests : IClassFixture<WebApplicationFixture>
    {
        private readonly WebApplicationFixture _factory;

        public UserServiceIntegrationTests(WebApplicationFixture factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_CreatesUserAndDefaultLists()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            var mockUserListService = new Mock<IUserListService>();
            
            mockUserListService.Setup(x => x.CreateDefaultListsForUserAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var userService = new UserService(userManager, mapper, mockUserListService.Object);

            var registerDto = new RegisterDto
            {
                UserName = "integrationtestuser",
                Email = "integration@test.com",
                Password = "TestPassword123!",
                DisplayName = "Integration Test User"
            };

            // Act
            var result = await userService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();

            // Verify user was created in database
            var createdUser = await userManager.FindByEmailAsync(registerDto.Email);
            createdUser.Should().NotBeNull();
            createdUser!.UserName.Should().Be(registerDto.UserName);
            createdUser.Email.Should().Be(registerDto.Email);
            createdUser.DisplayName.Should().Be(registerDto.DisplayName);

            // Verify default lists creation was called
            mockUserListService.Verify(x => x.CreateDefaultListsForUserAsync(createdUser.Id), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_ReturnsFailure()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            var mockUserListService = new Mock<IUserListService>();

            var userService = new UserService(userManager, mapper, mockUserListService.Object);

            var email = "duplicate@integration.test";
            var firstUserDto = new RegisterDto
            {
                UserName = "firstuser",
                Email = email,
                Password = "TestPassword123!",
                DisplayName = "First User"
            };

            var secondUserDto = new RegisterDto
            {
                UserName = "seconduser",
                Email = email, // Same email
                Password = "TestPassword123!",
                DisplayName = "Second User"
            };

            // Act
            var firstResult = await userService.RegisterAsync(firstUserDto);
            var secondResult = await userService.RegisterAsync(secondUserDto);

            // Assert
            firstResult.Succeeded.Should().BeTrue();
            secondResult.Succeeded.Should().BeFalse();
            secondResult.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ValidateUserAsync_WithValidCredentials_ReturnsSuccess()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            var mockUserListService = new Mock<IUserListService>();

            var userService = new UserService(userManager, mapper, mockUserListService.Object);

            // First create a user
            var registerDto = new RegisterDto
            {
                UserName = "validationuser",
                Email = "validation@test.com",
                Password = "TestPassword123!",
                DisplayName = "Validation Test User"
            };

            await userService.RegisterAsync(registerDto);

            var loginDto = new LoginDto
            {
                EmailOrUsername = "validation@test.com",
                Password = "TestPassword123!"
            };

            // Act
            var result = await userService.ValidateUserAsync(loginDto);

            // Assert
            result.Success.Should().BeTrue();
            result.User.Should().NotBeNull();
            result.User!.Email.Should().Be(registerDto.Email);
            result.User.UserName.Should().Be(registerDto.UserName);
        }

        [Fact]
        public async Task ValidateUserAsync_WithInvalidPassword_ReturnsFailure()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            var mockUserListService = new Mock<IUserListService>();

            var userService = new UserService(userManager, mapper, mockUserListService.Object);

            // First create a user
            var registerDto = new RegisterDto
            {
                UserName = "invalidpassuser",
                Email = "invalidpass@test.com",
                Password = "TestPassword123!",
                DisplayName = "Invalid Pass Test User"
            };

            await userService.RegisterAsync(registerDto);

            var loginDto = new LoginDto
            {
                EmailOrUsername = "invalidpass@test.com",
                Password = "WrongPassword!" // Wrong password
            };

            // Act
            var result = await userService.ValidateUserAsync(loginDto);

            // Assert
            result.Success.Should().BeFalse();
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ValidateUserAsync_WithUsername_ReturnsSuccess()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            var mockUserListService = new Mock<IUserListService>();

            var userService = new UserService(userManager, mapper, mockUserListService.Object);

            // First create a user
            var registerDto = new RegisterDto
            {
                UserName = "usernamelogintest",
                Email = "usernamelogin@test.com",
                Password = "TestPassword123!",
                DisplayName = "Username Login Test User"
            };

            await userService.RegisterAsync(registerDto);

            var loginDto = new LoginDto
            {
                EmailOrUsername = "usernamelogintest", // Use username instead of email
                Password = "TestPassword123!"
            };

            // Act
            var result = await userService.ValidateUserAsync(loginDto);

            // Assert
            result.Success.Should().BeTrue();
            result.User.Should().NotBeNull();
            result.User!.UserName.Should().Be(registerDto.UserName);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            var mockUserListService = new Mock<IUserListService>();

            var userService = new UserService(userManager, mapper, mockUserListService.Object);

            // First create a user
            var registerDto = new RegisterDto
            {
                UserName = "getuserbyid",
                Email = "getuserbyid@test.com",
                Password = "TestPassword123!",
                DisplayName = "Get User By ID Test"
            };

            await userService.RegisterAsync(registerDto);
            var createdUser = await userManager.FindByEmailAsync(registerDto.Email);

            // Act
            var result = await userService.GetUserByIdAsync(createdUser!.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(createdUser.Id);
            result.Email.Should().Be(registerDto.Email);
            result.UserName.Should().Be(registerDto.UserName);
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithValidEmail_ReturnsUser()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            var mockUserListService = new Mock<IUserListService>();

            var userService = new UserService(userManager, mapper, mockUserListService.Object);

            // First create a user
            var registerDto = new RegisterDto
            {
                UserName = "getuserbyemail",
                Email = "getuserbyemail@test.com",
                Password = "TestPassword123!",
                DisplayName = "Get User By Email Test"
            };

            await userService.RegisterAsync(registerDto);

            // Act
            var result = await userService.GetUserByEmailAsync(registerDto.Email);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(registerDto.Email);
            result.UserName.Should().Be(registerDto.UserName);
            result.DisplayName.Should().Be(registerDto.DisplayName);
        }

        [Fact]
        public async Task RegisterAsync_WithWeakPassword_ReturnsFailure()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            var mockUserListService = new Mock<IUserListService>();

            var userService = new UserService(userManager, mapper, mockUserListService.Object);

            var registerDto = new RegisterDto
            {
                UserName = "weakpassuser",
                Email = "weakpass@test.com",
                Password = "123", // Too weak
                DisplayName = "Weak Password User"
            };

            // Act
            var result = await userService.RegisterAsync(registerDto);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            
            // Should contain password-related errors
            var errorDescriptions = result.Errors.Select(e => e.Description).ToList();
            errorDescriptions.Should().Contain(desc => desc.ToLower().Contains("password"));
        }
    }
}