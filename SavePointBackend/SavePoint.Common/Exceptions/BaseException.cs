using System.Net;

namespace SavePoint.Common.Exceptions
{
    /// <summary>
    /// Base exception class for all custom exceptions in the SavePoint application.
    /// Provides common properties and functionality for consistent error handling.
    /// </summary>
    public abstract class BaseException : Exception
    {
        /// <summary>
        /// Gets the HTTP status code that should be returned for this exception.
        /// </summary>
        public abstract HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the error code for this exception type.
        /// Used for client-side error handling and localization.
        /// </summary>
        public abstract string ErrorCode { get; }

        /// <summary>
        /// Gets additional details about the error.
        /// Can contain validation errors, field-specific messages, etc.
        /// </summary>
        public virtual object? Details { get; set; }

        protected BaseException(string message) : base(message)
        {
        }

        protected BaseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BaseException(string message, object? details) : base(message)
        {
            Details = details;
        }
    }
}