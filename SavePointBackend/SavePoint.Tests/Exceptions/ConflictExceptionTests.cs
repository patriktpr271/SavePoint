using FluentAssertions;
using SavePoint.Common.Exceptions;
using System.Net;

namespace SavePoint.Tests.Exceptions
{
    public class ConflictExceptionTests
    {
        [Fact]
        public void Constructor_WithMessage_SetsMessageCorrectly()
        {
            // Arrange
            var message = "Test conflict message";

            // Act
            var exception = new ConflictException(message);

            // Assert
            exception.Message.Should().Be(message);
            exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
            exception.ErrorCode.Should().Be("RESOURCE_CONFLICT");
        }

        [Fact]
        public void Constructor_WithResourceTypeAndConflictField_FormatsMessageCorrectly()
        {
            // Arrange
            var resourceType = "User";
            var conflictField = "email";
            var conflictValue = "test@example.com";

            // Act
            var exception = new ConflictException(resourceType, conflictField, conflictValue);

            // Assert
            exception.Message.Should().Be("User with email 'test@example.com' already exists");
            exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
            exception.ErrorCode.Should().Be("RESOURCE_CONFLICT");
            exception.ResourceType.Should().Be(resourceType);
            exception.ConflictField.Should().Be(conflictField);
            exception.Details.Should().NotBeNull();
        }

        [Fact]
        public void DuplicateEmail_CreatesCorrectException()
        {
            // Arrange
            var email = "test@example.com";

            // Act
            var exception = ConflictException.DuplicateEmail(email);

            // Assert
            exception.Message.Should().Be("User with email 'test@example.com' already exists");
            exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
            exception.ErrorCode.Should().Be("RESOURCE_CONFLICT");
            exception.ResourceType.Should().Be("User");
            exception.ConflictField.Should().Be("email");
        }

        [Fact]
        public void DuplicateUsername_CreatesCorrectException()
        {
            // Arrange
            var username = "testuser";

            // Act
            var exception = ConflictException.DuplicateUsername(username);

            // Assert
            exception.Message.Should().Be("User with username 'testuser' already exists");
            exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
            exception.ErrorCode.Should().Be("RESOURCE_CONFLICT");
            exception.ResourceType.Should().Be("User");
            exception.ConflictField.Should().Be("username");
        }

        [Fact]
        public void DuplicateReview_CreatesCorrectException()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = "test-user-id";

            // Act
            var exception = ConflictException.DuplicateReview(gameId, userId);

            // Assert
            exception.Message.Should().Be("Review for this game already exists");
            exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
            exception.ErrorCode.Should().Be("RESOURCE_CONFLICT");
            exception.ResourceType.Should().Be("Review");
            exception.ResourceId.Should().Be($"{userId}-{gameId}");
            exception.ConflictField.Should().Be("gameId");
        }
    }
}