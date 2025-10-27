using Microsoft.AspNetCore.Identity;
using SavePoint.IntegrationTests.Fixtures;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SavePoint.IntegrationTests.Api
{
    public class AuthControllerTests : IClassFixture<WebApplicationFixture>
    {
        private readonly WebApplicationFixture _factory;
        private readonly HttpClient _client;

        public AuthControllerTests(WebApplicationFixture factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                UserName = "testuser123",
                Email = "test@example.com",
                Password = "TestPassword123!",
                DisplayName = "Test User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Registration successful");
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                UserName = "testuser456",
                Email = "duplicate@example.com",
                Password = "TestPassword123!",
                DisplayName = "Test User"
            };

            // First registration
            await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Second registration with same email
            var duplicateDto = new RegisterDto
            {
                UserName = "differentuser",
                Email = "duplicate@example.com", // Same email
                Password = "TestPassword123!",
                DisplayName = "Different User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", duplicateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsUserData()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                UserName = "logintest",
                Email = "logintest@example.com",
                Password = "TestPassword123!",
                DisplayName = "Login Test User"
            };

            // Register user first
            await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            var loginDto = new LoginDto
            {
                EmailOrUsername = "logintest@example.com",
                Password = "TestPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<JsonElement>(content);
            
            loginResponse.TryGetProperty("message", out var messageProperty).Should().BeTrue();
            messageProperty.GetString().Should().Be("Login successful");
            
            loginResponse.TryGetProperty("user", out var userProperty).Should().BeTrue();
            userProperty.TryGetProperty("email", out var emailProperty).Should().BeTrue();
            emailProperty.GetString().Should().Be(registerDto.Email);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                EmailOrUsername = "nonexistent@example.com",
                Password = "WrongPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Invalid email/username or password");
        }

        [Fact]
        public async Task Register_WithInvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                UserName = "testuser789",
                Email = "invalid-email", // Invalid email format
                Password = "TestPassword123!",
                DisplayName = "Test User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WithWeakPassword_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                UserName = "testuser101",
                Email = "weakpass@example.com",
                Password = "123", // Too weak
                DisplayName = "Test User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("")]
        [InlineData("ab")]
        [InlineData("this_username_is_way_too_long_for_validation_rules_x")] // 52 chars > 50 max
        public async Task Register_WithInvalidUsername_ReturnsBadRequest(string invalidUsername)
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                UserName = invalidUsername,
                Email = $"test{Guid.NewGuid()}@example.com",
                Password = "TestPassword123!",
                DisplayName = "Test User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithUsername_ReturnsUserData()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                UserName = "usernamelogin",
                Email = "usernamelogin@example.com",
                Password = "TestPassword123!",
                DisplayName = "Username Login Test"
            };

            // Register user first
            await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            var loginDto = new LoginDto
            {
                EmailOrUsername = "usernamelogin", // Use username instead of email
                Password = "TestPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<JsonElement>(content);
            
            loginResponse.TryGetProperty("message", out var messageProperty).Should().BeTrue();
            messageProperty.GetString().Should().Be("Login successful");
            
            loginResponse.TryGetProperty("user", out var userProperty).Should().BeTrue();
            userProperty.TryGetProperty("username", out var usernameProperty).Should().BeTrue();
            usernameProperty.GetString().Should().Be(registerDto.UserName);
        }

        [Fact]
        public async Task GetCurrentUser_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/auth/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Logout_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.PostAsync("/api/auth/logout", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}