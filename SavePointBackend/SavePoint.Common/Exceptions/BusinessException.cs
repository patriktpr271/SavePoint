using System.Net;

namespace SavePoint.Common.Exceptions
{
    /// <summary>
    /// Exception thrown for general business logic errors that don't fit other categories.
    /// Maps to HTTP 400 Bad Request by default.
    /// </summary>
    public class BusinessException : BaseException
    {
        public override HttpStatusCode StatusCode { get; }
        public override string ErrorCode { get; }

        /// <summary>
        /// The business rule that was violated.
        /// </summary>
        public string? BusinessRule { get; }

        public BusinessException(string message, string errorCode = "BUSINESS_RULE_VIOLATION", HttpStatusCode statusCode = HttpStatusCode.BadRequest) 
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        public BusinessException(string message, string businessRule, string errorCode = "BUSINESS_RULE_VIOLATION", HttpStatusCode statusCode = HttpStatusCode.BadRequest) 
            : base(message)
        {
            BusinessRule = businessRule;
            ErrorCode = errorCode;
            StatusCode = statusCode;
            Details = new { BusinessRule };
        }

        public BusinessException(string message, Exception innerException, string errorCode = "BUSINESS_RULE_VIOLATION", HttpStatusCode statusCode = HttpStatusCode.BadRequest) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Creates a business exception for invalid operations.
        /// </summary>
        public static BusinessException InvalidOperation(string operation, string reason)
        {
            return new BusinessException($"Cannot {operation}: {reason}", operation, "INVALID_OPERATION");
        }

        /// <summary>
        /// Creates a business exception for external service failures.
        /// </summary>
        public static BusinessException ExternalServiceError(string serviceName, string message)
        {
            return new BusinessException($"External service '{serviceName}' error: {message}", serviceName, "EXTERNAL_SERVICE_ERROR", HttpStatusCode.BadGateway);
        }
    }
}