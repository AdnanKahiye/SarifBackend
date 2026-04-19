using Backend.Wrapper;
using System.Net;
using System.Text.Json;

namespace Backend.Middlewares
{
    /// <summary>
    /// Global middleware that handles all unhandled exceptions in the pipeline.
    /// Returns standardized ResponseWrapper JSON for consistent enterprise responses.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware for the current HTTP request.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        /// <summary>
        /// Handles exceptions globally and returns a standardized JSON response.
        /// </summary>
        private static async Task HandleExceptionAsync(HttpContext context, Exception ex, ILogger logger)
        {
            //  Log full details internally
            logger.LogError(ex, "❌ Unhandled exception occurred while processing the request.");

            context.Response.ContentType = "application/json";

            //  Map known exception types to appropriate HTTP status codes
            var statusCode = ex switch
            {
                ArgumentNullException or ArgumentException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                _ => (int)HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = statusCode;

            //  Use ResponseWrapper.Fail() factory to avoid constructor ambiguity
            var response = ResponseWrapper<string>.Fail(
                ex.Message,
                $"An unhandled {ex.GetType().Name} occurred.",
                statusCode
            );

            //  Optionally include stack trace (only for development or internal logs)
            response.Errors.Add(ex.StackTrace ?? "No stack trace available.");

            //  JSON serialization options
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            // 🔹 Write standardized JSON response
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }

    /// <summary>
    /// Extension method to easily register the global exception middleware.
    /// </summary>
    public static class ExceptionHandlingMiddlewareExtensions
    {
        /// <summary>
        /// Adds the global exception-handling middleware to the application pipeline.
        /// </summary>
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
