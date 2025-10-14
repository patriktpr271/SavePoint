using System.Net;

namespace SavePoint.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when a user is not authenticated but authentication is required.
    /// Maps to HTTP 401 Unauthorized.
    /// </summary>
    public class UnauthorizedException : BaseException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
        public override string ErrorCode => "AUTHENTICATION_REQUIRED";

        public UnauthorizedException() : base("Authentication is required to access this resource")
        {
        }

        public UnauthorizedException(string message) : base(message)
        {
        }

        public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}