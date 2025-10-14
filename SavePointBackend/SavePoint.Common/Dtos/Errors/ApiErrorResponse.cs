using System.Text.Json.Serialization;

namespace SavePoint.Common.Dtos.Errors
{
    /// <summary>
    /// Standardized error response for API endpoints.
    /// Provides consistent error information to clients.
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>
        /// The error message for display to users.
        /// </summary>
        [JsonPropertyName("message")]
        public required string Message { get; set; }

        /// <summary>
        /// Machine-readable error code for client-side handling.
        /// </summary>
        [JsonPropertyName("errorCode")]
        public required string ErrorCode { get; set; }

        /// <summary>
        /// HTTP status code.
        /// </summary>
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        /// <summary>
        /// Timestamp when the error occurred.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Unique identifier for this error occurrence (for logging/debugging).
        /// </summary>
        [JsonPropertyName("traceId")]
        public string? TraceId { get; set; }

        /// <summary>
        /// Additional error details (validation errors, etc.).
        /// </summary>
        [JsonPropertyName("details")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Details { get; set; }

        /// <summary>
        /// Stack trace (only included in development environment).
        /// </summary>
        [JsonPropertyName("stackTrace")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? StackTrace { get; set; }

        /// <summary>
        /// Inner exception information (only in development).
        /// </summary>
        [JsonPropertyName("innerException")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ApiErrorResponse? InnerException { get; set; }
    }
}