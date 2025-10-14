using System.Net;

namespace SavePoint.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested resource cannot be found.
    /// Maps to HTTP 404 Not Found.
    /// </summary>
    public class NotFoundException : BaseException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
        public override string ErrorCode => "RESOURCE_NOT_FOUND";

        /// <summary>
        /// The type of resource that was not found (e.g., "Game", "User", "Review").
        /// </summary>
        public string ResourceType { get; }

        /// <summary>
        /// The identifier that was used to search for the resource.
        /// </summary>
        public string? ResourceId { get; }

        public NotFoundException(string resourceType, string? resourceId = null) 
            : base($"{resourceType} not found" + (resourceId != null ? $" with id: {resourceId}" : ""))
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
            Details = new { ResourceType, ResourceId };
        }

        public NotFoundException(string resourceType, Guid resourceId) 
            : this(resourceType, resourceId.ToString())
        {
        }

        public NotFoundException(string resourceType, int resourceId) 
            : this(resourceType, resourceId.ToString())
        {
        }
    }
}