namespace SavePoint.ReviewsService.Middleware
{
	public static class MiddlewareExtensions
	{
		public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
		{
			return app.UseMiddleware<GlobalExceptionMiddleware>();
		}
	}
}