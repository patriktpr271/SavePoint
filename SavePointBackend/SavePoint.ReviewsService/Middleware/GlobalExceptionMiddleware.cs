using System.Diagnostics;
using System.Net;
using System.Text.Json;
using SavePoint.Common.Dtos.Errors;
using SavePoint.Common.Exceptions;

namespace SavePoint.ReviewsService.Middleware
{
	public class GlobalExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<GlobalExceptionMiddleware> _logger;
		private readonly IWebHostEnvironment _environment;

		public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment environment)
		{
			_next = next;
			_logger = logger;
			_environment = environment;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception exception)
			{
				await HandleExceptionAsync(context, exception);
			}
		}

		private async Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
			var errorResponse = CreateErrorResponse(exception, traceId);
			LogException(exception, traceId, context);

			context.Response.ContentType = "application/json";
			context.Response.StatusCode = errorResponse.StatusCode;

			var jsonOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
			};

			var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
			await context.Response.WriteAsync(jsonResponse);
		}

		private ApiErrorResponse CreateErrorResponse(Exception exception, string traceId)
		{
			return exception switch
			{
				BaseException baseEx => CreateCustomErrorResponse(baseEx, traceId),
				ArgumentNullException argNullEx => new ApiErrorResponse
				{
					Message = "A required parameter was not provided",
					ErrorCode = "INVALID_ARGUMENT",
					StatusCode = (int)HttpStatusCode.BadRequest,
					TraceId = traceId,
					Details = _environment.IsDevelopment() ? new { Parameter = argNullEx.ParamName } : null,
					StackTrace = _environment.IsDevelopment() ? argNullEx.StackTrace : null
				},
				ArgumentException argEx => new ApiErrorResponse
				{
					Message = argEx.Message,
					ErrorCode = "INVALID_ARGUMENT",
					StatusCode = (int)HttpStatusCode.BadRequest,
					TraceId = traceId,
					Details = _environment.IsDevelopment() ? new { Parameter = argEx.ParamName } : null,
					StackTrace = _environment.IsDevelopment() ? argEx.StackTrace : null
				},
				UnauthorizedAccessException => new ApiErrorResponse
				{
					Message = "Access denied",
					ErrorCode = "ACCESS_DENIED",
					StatusCode = (int)HttpStatusCode.Forbidden,
					TraceId = traceId
				},
				TimeoutException => new ApiErrorResponse
				{
					Message = "The operation timed out",
					ErrorCode = "TIMEOUT",
					StatusCode = (int)HttpStatusCode.RequestTimeout,
					TraceId = traceId
				},
				NotImplementedException => new ApiErrorResponse
				{
					Message = "This feature is not yet implemented",
					ErrorCode = "NOT_IMPLEMENTED",
					StatusCode = (int)HttpStatusCode.NotImplemented,
					TraceId = traceId
				},
				_ => new ApiErrorResponse
				{
					Message = _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred. Please try again later.",
					ErrorCode = "INTERNAL_SERVER_ERROR",
					StatusCode = (int)HttpStatusCode.InternalServerError,
					TraceId = traceId,
					StackTrace = _environment.IsDevelopment() ? exception.StackTrace : null,
					InnerException = _environment.IsDevelopment() && exception.InnerException != null ? CreateErrorResponse(exception.InnerException, traceId) : null
				}
			};
		}

		private ApiErrorResponse CreateCustomErrorResponse(BaseException exception, string traceId)
		{
			var response = new ApiErrorResponse
			{
				Message = exception.Message,
				ErrorCode = exception.ErrorCode,
				StatusCode = (int)exception.StatusCode,
				TraceId = traceId,
				Details = exception.Details
			};

			if (_environment.IsDevelopment())
			{
				response.StackTrace = exception.StackTrace;
				if (exception.InnerException != null)
				{
					response.InnerException = CreateErrorResponse(exception.InnerException, traceId);
				}
			}

			if (exception is ValidationException validationEx && validationEx.ValidationErrors != null)
			{
				return new ValidationErrorResponse
				{
					Message = response.Message,
					ErrorCode = response.ErrorCode,
					StatusCode = response.StatusCode,
					TraceId = response.TraceId,
					Details = response.Details,
					StackTrace = response.StackTrace,
					InnerException = response.InnerException,
					ValidationErrors = validationEx.ValidationErrors
				};
			}

			return response;
		}

		private void LogException(Exception exception, string traceId, HttpContext context)
		{
			var requestPath = context.Request.Path;
			var requestMethod = context.Request.Method;
			var userId = context.User?.Identity?.Name ?? "Anonymous";

			using var scope = _logger.BeginScope(new Dictionary<string, object>
			{
				["TraceId"] = traceId,
				["UserId"] = userId,
				["RequestPath"] = requestPath,
				["RequestMethod"] = requestMethod
			});

			switch (exception)
			{
				case BaseException baseEx when baseEx.StatusCode == HttpStatusCode.NotFound:
					_logger.LogWarning(exception, "Resource not found: {Message}", exception.Message);
					break;
				case BaseException baseEx when baseEx.StatusCode == HttpStatusCode.BadRequest:
					_logger.LogWarning(exception, "Validation error: {Message}", exception.Message);
					break;
				case BaseException baseEx when baseEx.StatusCode == HttpStatusCode.Unauthorized:
					_logger.LogWarning(exception, "Unauthorized access attempt: {Message}", exception.Message);
					break;
				case UnauthorizedAccessException:
					_logger.LogWarning(exception, "Unauthorized access attempt: {Message}", exception.Message);
					break;
				case BaseException baseEx when baseEx.StatusCode == HttpStatusCode.Forbidden:
					_logger.LogWarning(exception, "Forbidden access attempt: {Message}", exception.Message);
					break;
				case BaseException baseEx when baseEx.StatusCode == HttpStatusCode.Conflict:
					_logger.LogWarning(exception, "Resource conflict: {Message}", exception.Message);
					break;
				case ArgumentNullException:
				case ArgumentException:
					_logger.LogWarning(exception, "Invalid argument: {Message}", exception.Message);
					break;
				case TimeoutException:
					_logger.LogWarning(exception, "Operation timeout: {Message}", exception.Message);
					break;
				case NotImplementedException:
					_logger.LogWarning(exception, "Not implemented: {Message}", exception.Message);
					break;
				default:
					_logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
					break;
			}
		}
	}
}