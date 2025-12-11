using Moq;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using SavePoint.BusinessLogic.Services;
using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.Common.Dtos.Users;
using SavePoint.Entities.Users;

namespace SavePoint.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IUserListService> _mockUserListService;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            // UserManager requires a complex setup for mocking
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, 
                null!, null!, null!, null!, null!, null!, null!, null!);
            _mockMapper = new Mock<IMapper>();
            _mockUserListService = new Mock<IUserListService>();
            _userService = new UserService(_mockUserManager.Object, _mockMapper.Object, _mockUserListService.Object);
        }

        [Fact]
        public async Task RegisterAsync_WithValidDto_ReturnsSuccessAndCreatesDefaultLists()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "TestPassword123!",
                DisplayName = "Test User"
            };

            var user = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                DisplayName = registerDto.DisplayName
            };

            var successResult = IdentityResult.Success;

            _mockMapper.Setup(x => x.Map<ApplicationUser>(registerDto))
                .Returns(user);

            _mockUserManager.Setup(x => x.CreateAsync(user, registerDto.Password))
                .ReturnsAsync(successResult);

            _mockUserListService.Setup(x => x.CreateDefaultListsForUserAsync(user.Id))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.RegisterAsync(registerDto);

            // Assert
            result.Should().Be(successResult);
            result.Succeeded.Should().BeTrue();

            _mockMapper.Verify(x => x.Map<ApplicationUser>(registerDto), Times.Once);
            _mockUserManager.Verify(x => x.CreateAsync(user, registerDto.Password), Times.Once);
            _mockUserListService.Verify(x => x.CreateDefaultListsForUserAsync(user.Id), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithInvalidDto_ReturnsFailureAndDoesNotCreateLists()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                UserName = "testuser",
                Email = "invalid-email",
                Password = "weak",
                DisplayName = "Test User"
            };

            var user = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                DisplayName = registerDto.DisplayName
            };

            var failureResult = IdentityResult.Failed(new IdentityError { Description = "Invalid email" });

            _mockMapper.Setup(x => x.Map<ApplicationUser>(registerDto))
                .Returns(user);

            _mockUserManager.Setup(x => x.CreateAsync(user, registerDto.Password))
                .ReturnsAsync(failureResult);

            // Act
            var result = await _userService.RegisterAsync(registerDto);

            // Assert
            result.Should().Be(failureResult);
            result.Succeeded.Should().BeFalse();

            _mockMapper.Verify(x => x.Map<ApplicationUser>(registerDto), Times.Once);
            _mockUserManager.Verify(x => x.CreateAsync(user, registerDto.Password), Times.Once);
            _mockUserListService.Verify(x => x.CreateDefaultListsForUserAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ValidateUserAsync_WithValidEmailAndPassword_ReturnsSuccessAndUser()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                EmailOrUsername = "test@example.com",
                Password = "TestPassword123!"
            };

            var user = new ApplicationUser
            {
                Id = "test-user-id",
                Email = loginDto.EmailOrUsername,
                UserName = "testuser"
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.EmailOrUsername))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.ValidateUserAsync(loginDto);

            // Assert
            result.Success.Should().BeTrue();
            result.User.Should().Be(user);

            _mockUserManager.Verify(x => x.FindByEmailAsync(loginDto.EmailOrUsername), Times.Once);
            _mockUserManager.Verify(x => x.CheckPasswordAsync(user, loginDto.Password), Times.Once);
            _mockUserManager.Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ValidateUserAsync_WithValidUsernameAndPassword_ReturnsSuccessAndUser()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                EmailOrUsername = "testuser",
                Password = "TestPassword123!"
            };

            var user = new ApplicationUser
            {
                Id = "test-user-id",
                Email = "test@example.com",
                UserName = loginDto.EmailOrUsername
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.EmailOrUsername))
                .ReturnsAsync((ApplicationUser?)null);

            _mockUserManager.Setup(x => x.FindByNameAsync(loginDto.EmailOrUsername))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.ValidateUserAsync(loginDto);

            // Assert
            result.Success.Should().BeTrue();
            result.User.Should().Be(user);

            _mockUserManager.Verify(x => x.FindByEmailAsync(loginDto.EmailOrUsername), Times.Once);
            _mockUserManager.Verify(x => x.FindByNameAsync(loginDto.EmailOrUsername), Times.Once);
            _mockUserManager.Verify(x => x.CheckPasswordAsync(user, loginDto.Password), Times.Once);
        }

        [Fact]
        public async Task ValidateUserAsync_WithInvalidPassword_ReturnsFailure()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                EmailOrUsername = "test@example.com",
                Password = "WrongPassword"
            };

            var user = new ApplicationUser
            {
                Id = "test-user-id",
                Email = loginDto.EmailOrUsername,
                UserName = "testuser"
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.EmailOrUsername))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(false);

            // Act
            var result = await _userService.ValidateUserAsync(loginDto);

            // Assert
            result.Success.Should().BeFalse();
            result.User.Should().BeNull();

            _mockUserManager.Verify(x => x.FindByEmailAsync(loginDto.EmailOrUsername), Times.Once);
            _mockUserManager.Verify(x => x.CheckPasswordAsync(user, loginDto.Password), Times.Once);
        }

        [Fact]
        public async Task ValidateUserAsync_WithNonExistentUser_ReturnsFailure()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                EmailOrUsername = "nonexistent@example.com",
                Password = "TestPassword123!"
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.EmailOrUsername))
                .ReturnsAsync((ApplicationUser?)null);

            _mockUserManager.Setup(x => x.FindByNameAsync(loginDto.EmailOrUsername))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _userService.ValidateUserAsync(loginDto);

            // Assert
            result.Success.Should().BeFalse();
            result.User.Should().BeNull();

            _mockUserManager.Verify(x => x.FindByEmailAsync(loginDto.EmailOrUsername), Times.Once);
            _mockUserManager.Verify(x => x.FindByNameAsync(loginDto.EmailOrUsername), Times.Once);
            _mockUserManager.Verify(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
        {
            // Arrange
            var userId = "test-user-id";
            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@example.com",
                UserName = "testuser"
            };

            _mockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().Be(user);
            _mockUserManager.Verify(x => x.FindByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithValidEmail_ReturnsUser()
        {
            // Arrange
            var email = "test@example.com";
            var user = new ApplicationUser
            {
                Id = "test-user-id",
                Email = email,
                UserName = "testuser"
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByEmailAsync(email);

            // Assert
            result.Should().Be(user);
            _mockUserManager.Verify(x => x.FindByEmailAsync(email), Times.Once);
        }
    }
}