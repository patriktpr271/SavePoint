using System.Net;

namespace SavePoint.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested operation conflicts with the current state of the resource.
    /// Maps to HTTP 409 Conflict.
    /// </summary>
    public class ConflictException : BaseException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;
        public override string ErrorCode => "RESOURCE_CONFLICT";

        /// <summary>
        /// The resource that is in conflict.
        /// </summary>
        public string? ResourceType { get; }

        /// <summary>
        /// The identifier of the resource that is in conflict.
        /// </summary>
        public string? ResourceId { get; }

        /// <summary>
        /// The specific field or property that is causing the conflict.
        /// </summary>
        public string? ConflictField { get; }

        public ConflictException(string message) : base(message)
        {
        }

        public ConflictException(string resourceType, string conflictField, string conflictValue) 
            : base($"{resourceType} with {conflictField} '{conflictValue}' already exists")
        {
            ResourceType = resourceType;
            ConflictField = conflictField;
            Details = new { ResourceType, ConflictField, ConflictValue = conflictValue };
        }

        public ConflictException(string message, string? resourceType, string? resourceId, string? conflictField) 
            : base(message)
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
            ConflictField = conflictField;
            Details = new { ResourceType, ResourceId, ConflictField };
        }

        /// <summary>
        /// Creates a conflict exception for duplicate email.
        /// </summary>
        public static ConflictException DuplicateEmail(string email)
        {
            return new ConflictException("User", "email", email);
        }

        /// <summary>
        /// Creates a conflict exception for duplicate username.
        /// </summary>
        public static ConflictException DuplicateUsername(string username)
        {
            return new ConflictException("User", "username", username);
        }

        /// <summary>
        /// Creates a conflict exception for duplicate review.
        /// </summary>
        public static ConflictException DuplicateReview(Guid gameId, string userId)
        {
            return new ConflictException("Review for this game already exists", "Review", $"{userId}-{gameId}", "gameId");
        }
    }
}