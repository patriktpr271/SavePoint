using System.Net;

namespace SavePoint.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when business logic validation fails.
    /// Maps to HTTP 400 Bad Request.
    /// </summary>
    public class ValidationException : BaseException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
        public override string ErrorCode => "VALIDATION_ERROR";

        /// <summary>
        /// Dictionary containing field-specific validation errors.
        /// Key: field name, Value: array of error messages for that field.
        /// </summary>
        public Dictionary<string, string[]>? ValidationErrors { get; }

        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Dictionary<string, string[]> validationErrors) 
            : base(message)
        {
            ValidationErrors = validationErrors;
            Details = new { ValidationErrors = validationErrors };
        }

        public ValidationException(string field, string errorMessage) 
            : base($"Validation failed for {field}: {errorMessage}")
        {
            ValidationErrors = new Dictionary<string, string[]>
            {
                [field] = new[] { errorMessage }
            };
            Details = new { ValidationErrors };
        }

        /// <summary>
        /// Creates a validation exception for multiple field errors.
        /// </summary>
        public static ValidationException WithErrors(Dictionary<string, string[]> errors)
        {
            return new ValidationException("Validation failed for one or more fields", errors);
        }

        /// <summary>
        /// Creates a validation exception for a single field error.
        /// </summary>
        public static ValidationException ForField(string field, string error)
        {
            return new ValidationException(field, error);
        }

        /// <summary>
        /// Creates a validation exception for multiple errors on a single field.
        /// </summary>
        public static ValidationException ForField(string field, params string[] errors)
        {
            var validationErrors = new Dictionary<string, string[]> { [field] = errors };
            return new ValidationException($"Validation failed for {field}", validationErrors);
        }
    }
}