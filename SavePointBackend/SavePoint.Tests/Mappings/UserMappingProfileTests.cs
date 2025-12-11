using AutoMapper;
using FluentAssertions;
using SavePoint.BusinessLogic.Mappings;
using SavePoint.Common.Dtos.Users;
using SavePoint.Entities.Users;
using SavePoint.Entities.Reviews;
using SavePoint.Entities.Lists;

namespace SavePoint.Tests.Mappings
{
    public class UserMappingProfileTests
    {
        private readonly IMapper _mapper;

        public UserMappingProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<UserMappingProfile>();
            });
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void RegisterDto_To_ApplicationUser_MapsCorrectly()
        {
            var registerDto = new RegisterDto
            {
                UserName = "testuser",
                Email = "test@example.com",
                DisplayName = "Test User",
                Password = "TestPassword123!"
            };

            var result = _mapper.Map<ApplicationUser>(registerDto);
            result.Should().NotBeNull();
            result.UserName.Should().Be("testuser");
            result.Email.Should().Be("test@example.com");
            result.DisplayName.Should().Be("Test User");
            result.EmailConfirmed.Should().BeFalse();
            result.PhoneNumberConfirmed.Should().BeFalse();
            result.TwoFactorEnabled.Should().BeFalse();
            result.LockoutEnabled.Should().BeTrue();
            result.AccessFailedCount.Should().Be(0);
            result.Bio.Should().Be(string.Empty);
            result.Id.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ApplicationUser_To_UserProfileDto_MapsCorrectly()
        {
            var user = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "testuser",
                Email = "test@example.com",
                DisplayName = "Test User",
                Bio = "This is my bio",
                Reviews = new List<Review>
                {
                    new Review { Id = Guid.NewGuid() },
                    new Review { Id = Guid.NewGuid() }
                },
                UserLists = new List<UserList>
                {
                    new UserList { Id = Guid.NewGuid() },
                    new UserList { Id = Guid.NewGuid() },
                    new UserList { Id = Guid.NewGuid() }
                }
            };

            var result = _mapper.Map<SavePoint.BusinessLogic.Mappings.UserProfileDto>(user);

            result.Should().NotBeNull();
            result.Id.Should().Be("test-user-id");
            result.UserName.Should().Be("testuser");
            result.Email.Should().Be("test@example.com");
            result.DisplayName.Should().Be("Test User");
            result.Bio.Should().Be("This is my bio");
            result.ReviewCount.Should().Be(2);
            result.ListCount.Should().Be(3);
        }

        [Fact]
        public void RegisterDto_To_ApplicationUser_WithMinimalData_MapsCorrectly()
        {
            var registerDto = new RegisterDto
            {
                UserName = "testuser",
                Email = "test@example.com",
                DisplayName = "Test User",
                Password = "TestPassword123!"
            };

            var result = _mapper.Map<ApplicationUser>(registerDto);

            result.Should().NotBeNull();
            result.UserName.Should().Be("testuser");
            result.Email.Should().Be("test@example.com");
            result.DisplayName.Should().Be("Test User");
        }

        [Fact]
        public void ApplicationUser_To_UserProfileDto_WithEmptyCollections_ReturnsZeroCounts()
        {
            var user = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "testuser",
                Email = "test@example.com",
                DisplayName = "Test User",
                Bio = "",
                Reviews = new List<Review>(),
                UserLists = new List<UserList>()
            };

            var result = _mapper.Map<SavePoint.BusinessLogic.Mappings.UserProfileDto>(user);
            result.Should().NotBeNull();
            result.ReviewCount.Should().Be(0);
            result.ListCount.Should().Be(0);
        }

        [Fact]
        public void UserMappingProfile_CanBeInstantiated()
        {
            var profile = new UserMappingProfile();

            profile.Should().NotBeNull();
        }
    }
}