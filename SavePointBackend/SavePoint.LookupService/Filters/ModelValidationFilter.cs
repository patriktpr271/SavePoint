using Microsoft.AspNetCore.Mvc.Filters;
using SavePoint.Common.Exceptions;

namespace SavePoint.LookupService.Filters
{
    public class ModelValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var validationErrors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>());

                throw ValidationException.WithErrors(validationErrors);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
