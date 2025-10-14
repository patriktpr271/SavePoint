using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SavePoint.Common.Exceptions;

namespace SavePoint.Host.Filters
{
    /// <summary>
    /// Action filter that automatically validates model state and throws ValidationException if invalid.
    /// This removes the need for manual ModelState.IsValid checks in controllers.
    /// </summary>
    public class ModelValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                // Convert ModelState errors to our custom validation exception
                var validationErrors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? new string[0]
                    );

                throw ValidationException.WithErrors(validationErrors);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Nothing to do after action execution
        }
    }
}