namespace SavePoint.Host.Middleware
{
    /// <summary>
    /// Extension methods for registering custom middleware.
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adds the global exception handling middleware to the application pipeline.
        /// This should be added early in the pipeline to catch all exceptions.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder for chaining.</returns>
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}