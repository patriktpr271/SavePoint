using System.Text.Json.Serialization;

namespace SavePoint.Common.Dtos.Errors
{
    /// <summary>
    /// Specific error response for validation errors.
    /// Extends ApiErrorResponse with validation-specific information.
    /// </summary>
    public class ValidationErrorResponse : ApiErrorResponse
    {
        /// <summary>
        /// Dictionary of field-specific validation errors.
        /// Key: field name, Value: array of error messages.
        /// </summary>
        [JsonPropertyName("validationErrors")]
        public Dictionary<string, string[]> ValidationErrors { get; set; } = new();

        /// <summary>
        /// Creates a validation error response from ModelState errors.
        /// </summary>
        public static ValidationErrorResponse FromModelState(IDictionary<string, string[]> modelStateErrors)
        {
            return new ValidationErrorResponse
            {
                Message = "One or more validation errors occurred",
                ErrorCode = "VALIDATION_ERROR",
                StatusCode = 400,
                ValidationErrors = new Dictionary<string, string[]>(modelStateErrors),
                Details = new { ValidationErrors = modelStateErrors }
            };
        }

        /// <summary>
        /// Creates a validation error response from a single field error.
        /// </summary>
        public static ValidationErrorResponse FromFieldError(string field, string error)
        {
            var validationErrors = new Dictionary<string, string[]>
            {
                [field] = new[] { error }
            };

            return new ValidationErrorResponse
            {
                Message = $"Validation failed for {field}",
                ErrorCode = "VALIDATION_ERROR",
                StatusCode = 400,
                ValidationErrors = validationErrors,
                Details = new { ValidationErrors = validationErrors }
            };
        }

        /// <summary>
        /// Creates a validation error response from multiple field errors.
        /// </summary>
        public static ValidationErrorResponse FromErrors(Dictionary<string, string[]> errors)
        {
            return new ValidationErrorResponse
            {
                Message = "Validation failed for one or more fields",
                ErrorCode = "VALIDATION_ERROR",
                StatusCode = 400,
                ValidationErrors = errors,
                Details = new { ValidationErrors = errors }
            };
        }
    }
}