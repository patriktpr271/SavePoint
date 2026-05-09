using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SavePoint.BusinessLogic.Services;
using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.BusinessLogic.Tests.Helpers;
using SavePoint.Common.Dtos.Users;
using SavePoint.Entities.Users;

namespace SavePoint.BusinessLogic.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUserListService> _userListServiceMock;
        private readonly UserService _sut;

        public UserServiceTests()
        {
            _userManagerMock = UserManagerMockFactory.Create<ApplicationUser>();
            _mapperMock = new Mock<IMapper>();
            _userListServiceMock = new Mock<IUserListService>();
            _sut = new UserService(_userManagerMock.Object, _mapperMock.Object, _userListServiceMock.Object);
        }

        // ---------- RegisterAsync ----------

        [Fact]
        public async Task RegisterAsync_OnSuccess_SeedsDefaultListsForNewUser()
        {
            // Arrange
            var dto = new RegisterDto { UserName = "alice", Email = "a@b.com", Password = "Pwd1234!", DisplayName = "Alice" };
            var mappedUser = new ApplicationUser { Id = "user-1", UserName = "alice", Email = "a@b.com" };
            _mapperMock.Setup(m => m.Map<ApplicationUser>(dto)).Returns(mappedUser);
            _userManagerMock
                .Setup(m => m.CreateAsync(mappedUser, dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _sut.RegisterAsync(dto);

            // Assert
            result.Succeeded.Should().BeTrue();
            _userListServiceMock.Verify(s => s.CreateDefaultListsForUserAsync("user-1"), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_OnFailure_DoesNotSeedDefaultLists()
        {
            // Arrange
            var dto = new RegisterDto { UserName = "alice", Email = "bad", Password = "weak", DisplayName = "Alice" };
            var mappedUser = new ApplicationUser();
            var failure = IdentityResult.Failed(new IdentityError { Code = "InvalidPassword", Description = "weak" });
            _mapperMock.Setup(m => m.Map<ApplicationUser>(dto)).Returns(mappedUser);
            _userManagerMock
                .Setup(m => m.CreateAsync(mappedUser, dto.Password))
                .ReturnsAsync(failure);

            // Act
            var result = await _sut.RegisterAsync(dto);

            // Assert
            result.Succeeded.Should().BeFalse();
            _userListServiceMock.Verify(s => s.CreateDefaultListsForUserAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WhenUserManagerThrows_PropagatesException()
        {
            // Arrange
            var dto = new RegisterDto { UserName = "u", Email = "e@e.com", Password = "P!1abcdef", DisplayName = "u" };
            _mapperMock.Setup(m => m.Map<ApplicationUser>(dto)).Returns(new ApplicationUser());
            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("identity-failed"));

            // Act
            Func<Task> act = async () => await _sut.RegisterAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // ---------- ValidateUserAsync ----------

        [Fact]
        public async Task ValidateUserAsync_WithValidEmailAndCorrectPassword_ReturnsSuccessAndUser()
        {
            // Arrange
            var dto = new LoginDto { EmailOrUsername = "a@b.com", Password = "Pwd!" };
            var user = new ApplicationUser { Id = "u1", Email = "a@b.com" };
            _userManagerMock.Setup(m => m.FindByEmailAsync(dto.EmailOrUsername)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);

            // Act
            var (success, returnedUser) = await _sut.ValidateUserAsync(dto);

            // Assert
            success.Should().BeTrue();
            returnedUser.Should().BeSameAs(user);
            _userManagerMock.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ValidateUserAsync_WhenEmailLookupFails_FallsBackToUsernameLookup()
        {
            // Arrange
            var dto = new LoginDto { EmailOrUsername = "alice", Password = "Pwd!" };
            var user = new ApplicationUser { Id = "u1", UserName = "alice" };
            _userManagerMock.Setup(m => m.FindByEmailAsync(dto.EmailOrUsername)).ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(m => m.FindByNameAsync(dto.EmailOrUsername)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);

            // Act
            var (success, returnedUser) = await _sut.ValidateUserAsync(dto);

            // Assert
            success.Should().BeTrue();
            returnedUser.Should().BeSameAs(user);
        }

        [Fact]
        public async Task ValidateUserAsync_WithUnknownIdentifier_ReturnsFailure()
        {
            // Arrange
            var dto = new LoginDto { EmailOrUsername = "nobody", Password = "x" };
            _userManagerMock.Setup(m => m.FindByEmailAsync(dto.EmailOrUsername)).ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(m => m.FindByNameAsync(dto.EmailOrUsername)).ReturnsAsync((ApplicationUser?)null);

            // Act
            var (success, returnedUser) = await _sut.ValidateUserAsync(dto);

            // Assert
            success.Should().BeFalse();
            returnedUser.Should().BeNull();
            _userManagerMock.Verify(m => m.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ValidateUserAsync_WithCorrectUserButWrongPassword_ReturnsFailure()
        {
            // Arrange
            var dto = new LoginDto { EmailOrUsername = "a@b.com", Password = "wrong" };
            var user = new ApplicationUser { Id = "u1" };
            _userManagerMock.Setup(m => m.FindByEmailAsync(dto.EmailOrUsername)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(false);

            // Act
            var (success, returnedUser) = await _sut.ValidateUserAsync(dto);

            // Assert
            success.Should().BeFalse();
            returnedUser.Should().BeNull();
        }

        // ---------- GetUserByIdAsync / GetUserByEmailAsync ----------

        [Fact]
        public async Task GetUserByIdAsync_DelegatesToUserManager()
        {
            // Arrange
            var user = new ApplicationUser { Id = "u1" };
            _userManagerMock.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync(user);

            // Act
            var result = await _sut.GetUserByIdAsync("u1");

            // Assert
            result.Should().BeSameAs(user);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithUnknownId_ReturnsNull()
        {
            // Arrange
            _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _sut.GetUserByIdAsync("missing");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByEmailAsync_DelegatesToUserManager()
        {
            // Arrange
            var user = new ApplicationUser { Id = "u1", Email = "a@b.com" };
            _userManagerMock.Setup(m => m.FindByEmailAsync("a@b.com")).ReturnsAsync(user);

            // Act
            var result = await _sut.GetUserByEmailAsync("a@b.com");

            // Assert
            result.Should().BeSameAs(user);
        }
    }
}
