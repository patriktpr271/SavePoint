using System.Net;

namespace SavePoint.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when a user lacks permission to perform an action.
    /// Maps to HTTP 403 Forbidden.
    /// </summary>
    public class ForbiddenException : BaseException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
        public override string ErrorCode => "ACCESS_FORBIDDEN";

        /// <summary>
        /// The action that was attempted.
        /// </summary>
        public string? Action { get; }

        /// <summary>
        /// The resource that the action was attempted on.
        /// </summary>
        public string? Resource { get; }

        public ForbiddenException(string message) : base(message)
        {
        }

        public ForbiddenException(string action, string resource) 
            : base($"You do not have permission to {action} {resource}")
        {
            Action = action;
            Resource = resource;
            Details = new { Action, Resource };
        }

        public ForbiddenException(string message, string? action, string? resource) : base(message)
        {
            Action = action;
            Resource = resource;
            Details = new { Action, Resource };
        }
    }
}