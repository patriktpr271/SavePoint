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
            var message = "Test conflict message";

            var exception = new ConflictException(message);

            exception.Message.Should().Be(message);
            exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
            exception.ErrorCode.Should().Be("RESOURCE_CONFLICT");
        }

        [Fact]
        public void Constructor_WithResourceTypeAndConflictField_FormatsMessageCorrectly()
        {
            var resourceType = "User";
            var conflictField = "email";
            var conflictValue = "test@example.com";

            var exception = new ConflictException(resourceType, conflictField, conflictValue);

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
            var email = "test@example.com";

            var exception = ConflictException.DuplicateEmail(email);

            exception.Message.Should().Be("User with email 'test@example.com' already exists");
            exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
            exception.ErrorCode.Should().Be("RESOURCE_CONFLICT");
            exception.ResourceType.Should().Be("User");
            exception.ConflictField.Should().Be("email");
        }

        [Fact]
        public void DuplicateUsername_CreatesCorrectException()
        {
            var username = "testuser";

            var exception = ConflictException.DuplicateUsername(username);

            exception.Message.Should().Be("User with username 'testuser' already exists");
            exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
            exception.ErrorCode.Should().Be("RESOURCE_CONFLICT");
            exception.ResourceType.Should().Be("User");
            exception.ConflictField.Should().Be("username");
        }

        [Fact]
        public void DuplicateReview_CreatesCorrectException()
        {
            var gameId = Guid.NewGuid();
            var userId = "test-user-id";

            var exception = ConflictException.DuplicateReview(gameId, userId);

            exception.Message.Should().Be("Review for this game already exists");
            exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
            exception.ErrorCode.Should().Be("RESOURCE_CONFLICT");
            exception.ResourceType.Should().Be("Review");
            exception.ResourceId.Should().Be($"{userId}-{gameId}");
            exception.ConflictField.Should().Be("gameId");
        }
    }
}