using Implementation.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace SMSServiceAPI.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Generate a unique request ID for tracking
            var requestId = Guid.NewGuid().ToString("N")[..8];
            context.Response.Headers.Add("X-Request-ID", requestId);

            // Log the exception with request details
            _logger.LogError(exception, 
                "Unhandled exception occurred. RequestId: {RequestId}, Path: {Path}, Method: {Method}", 
                requestId, context.Request.Path, context.Request.Method);

            // Determine the appropriate response
            var response = DetermineResponse(exception, requestId);
            
            // Set response properties
            context.Response.StatusCode = response.ErrorCode;
            context.Response.ContentType = "application/json";

            // Add security headers
            context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
            context.Response.Headers.TryAdd("X-Frame-Options", "DENY");

            // Serialize and send response
            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private ResponseMessage DetermineResponse(Exception exception, string requestId)
        {
            return exception switch
            {
                ArgumentNullException => ErrorHandlingService.CreateErrorResponse(
                    "RequiredField", 
                    "Required information is missing.",
                    new { RequestId = requestId },
                    HttpStatusCode.BadRequest),

                ArgumentException => ErrorHandlingService.CreateErrorResponse(
                    "InvalidFormat", 
                    "The provided information is invalid.",
                    new { RequestId = requestId },
                    HttpStatusCode.BadRequest),

                UnauthorizedAccessException => ErrorHandlingService.CreateErrorResponse(
                    "AccessDenied", 
                    "You don't have permission to perform this action.",
                    new { RequestId = requestId },
                    HttpStatusCode.Forbidden),

                FileNotFoundException => ErrorHandlingService.CreateErrorResponse(
                    "RecordNotFound", 
                    "The requested file could not be found.",
                    new { RequestId = requestId },
                    HttpStatusCode.NotFound),

                DirectoryNotFoundException => ErrorHandlingService.CreateErrorResponse(
                    "RecordNotFound", 
                    "The requested directory could not be found.",
                    new { RequestId = requestId },
                    HttpStatusCode.NotFound),

                InvalidOperationException => ErrorHandlingService.CreateErrorResponse(
                    "InvalidOperation", 
                    "This operation cannot be performed at this time.",
                    new { RequestId = requestId },
                    HttpStatusCode.BadRequest),

                TimeoutException => ErrorHandlingService.CreateErrorResponse(
                    "TimeoutError", 
                    "The operation timed out. Please try again.",
                    new { RequestId = requestId },
                    HttpStatusCode.RequestTimeout),

                HttpRequestException => ErrorHandlingService.CreateErrorResponse(
                    "NetworkError", 
                    "A network error occurred. Please check your connection.",
                    new { RequestId = requestId },
                    HttpStatusCode.BadGateway),

                JsonException => ErrorHandlingService.CreateErrorResponse(
                    "InvalidFormat", 
                    "The data format is invalid.",
                    new { RequestId = requestId },
                    HttpStatusCode.BadRequest),

                NotSupportedException => ErrorHandlingService.CreateErrorResponse(
                    "ServiceUnavailable", 
                    "This feature is not currently supported.",
                    new { RequestId = requestId },
                    HttpStatusCode.NotImplemented),

                // Database exceptions
                var ex when ex.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                           ex.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) => 
                    ErrorHandlingService.CreateErrorResponse(
                        "DuplicateRecord", 
                        "This information already exists. Please check and try again.",
                        new { RequestId = requestId },
                        HttpStatusCode.Conflict),

                var ex when ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                           ex.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase) => 
                    ErrorHandlingService.CreateErrorResponse(
                        "RecordNotFound", 
                        "The requested information could not be found.",
                        new { RequestId = requestId },
                        HttpStatusCode.NotFound),

                var ex when ex.Message.Contains("concurrency", StringComparison.OrdinalIgnoreCase) ||
                           ex.Message.Contains("conflict", StringComparison.OrdinalIgnoreCase) => 
                    ErrorHandlingService.CreateErrorResponse(
                        "ConcurrencyError", 
                        "The information has been modified by another user. Please refresh and try again.",
                        new { RequestId = requestId },
                        HttpStatusCode.Conflict),

                // Default case
                _ => ErrorHandlingService.CreateErrorResponse(
                    "InternalServerError", 
                    "An unexpected error occurred. Please try again later.",
                    new { RequestId = requestId },
                    HttpStatusCode.InternalServerError)
            };
        }
    }

    /// <summary>
    /// Extension method to register the global exception middleware
    /// </summary>
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}
