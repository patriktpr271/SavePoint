using FluentAssertions;
using SavePoint.Common.Dtos.Users;
using System.ComponentModel.DataAnnotations;

namespace SavePoint.Tests.Dtos
{
    public class RegisterDtoTests
    {
        private static IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }

        [Fact]
        public void RegisterDto_WithValidData_PassesValidation()
        {
            // Arrange
            var dto = new RegisterDto
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "TestPassword123!",
                DisplayName = "Test User"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void RegisterDto_WithEmptyUserName_FailsValidation()
        {
            // Arrange
            var dto = new RegisterDto
            {
                UserName = "",
                Email = "test@example.com",
                Password = "TestPassword123!",
                DisplayName = "Test User"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(r => r.MemberNames.Contains("UserName"));
        }

        [Fact]
        public void RegisterDto_WithTooShortUserName_FailsValidation()
        {
            // Arrange
            var dto = new RegisterDto
            {
                UserName = "ab", // Less than 3 characters
                Email = "test@example.com",
                Password = "TestPassword123!",
                DisplayName = "Test User"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(r => r.MemberNames.Contains("UserName"));
        }

        [Fact]
        public void RegisterDto_WithEmptyEmail_FailsValidation()
        {
            // Arrange
            var dto = new RegisterDto
            {
                UserName = "testuser",
                Email = "",
                Password = "TestPassword123!",
                DisplayName = "Test User"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void RegisterDto_WithInvalidEmail_FailsValidation()
        {
            // Arrange
            var dto = new RegisterDto
            {
                UserName = "testuser",
                Email = "invalid-email",
                Password = "TestPassword123!",
                DisplayName = "Test User"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void RegisterDto_WithEmptyPassword_FailsValidation()
        {
            // Arrange
            var dto = new RegisterDto
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "",
                DisplayName = "Test User"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(r => r.MemberNames.Contains("Password"));
        }

        [Fact]
        public void RegisterDto_WithTooShortPassword_FailsValidation()
        {
            // Arrange
            var dto = new RegisterDto
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "12345", // Less than 6 characters
                DisplayName = "Test User"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(r => r.MemberNames.Contains("Password"));
        }

        [Fact]
        public void RegisterDto_WithEmptyDisplayName_FailsValidation()
        {
            // Arrange
            var dto = new RegisterDto
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "TestPassword123!",
                DisplayName = ""
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(r => r.MemberNames.Contains("DisplayName"));
        }

        [Fact]
        public void RegisterDto_WithTooShortDisplayName_FailsValidation()
        {
            // Arrange
            var dto = new RegisterDto
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "TestPassword123!",
                DisplayName = "a" // Less than 2 characters
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(r => r.MemberNames.Contains("DisplayName"));
        }

        [Fact]
        public void RegisterDto_WithValidEmailFormats_PassesValidation()
        {
            // Arrange
            var validEmails = new[]
            {
                "test@example.com",
                "user.name@domain.co.uk",
                "firstname+lastname@example.org",
                "email@subdomain.example.com"
            };

            foreach (var email in validEmails)
            {
                var dto = new RegisterDto
                {
                    UserName = "testuser",
                    Email = email,
                    Password = "TestPassword123!",
                    DisplayName = "Test User"
                };

                // Act
                var validationResults = ValidateModel(dto);

                // Assert
                validationResults.Should().BeEmpty($"Email {email} should be valid");
            }
        }

        [Fact]
        public void RegisterDto_WithMinimumValidLengths_PassesValidation()
        {
            // Arrange
            var dto = new RegisterDto
            {
                UserName = "abc", // Minimum 3 characters
                Email = "a@b.co",
                Password = "123456", // Minimum 6 characters
                DisplayName = "ab" // Minimum 2 characters
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void RegisterDto_WithMaximumValidLengths_PassesValidation()
        {
            // Arrange
            var dto = new RegisterDto
            {
                UserName = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuv", // 50 characters
                Email = "test@example.com",
                Password = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqr", // 100 characters
                DisplayName = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqr" // 100 characters
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().BeEmpty();
        }
    }
}