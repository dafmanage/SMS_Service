using Implementation.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace SMSServiceAPI.Middleware
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidationMiddleware> _logger;

        public ValidationMiddleware(RequestDelegate next, ILogger<ValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Validate request size
                if (context.Request.ContentLength > 10 * 1024 * 1024) // 10MB limit
                {
                    await HandleValidationError(context, "RequestTooLarge", 
                        "The request is too large. Please reduce the data size and try again.");
                    return;
                }

                // Validate content type for POST/PUT requests
                if (context.Request.Method == "POST" || context.Request.Method == "PUT")
                {
                    var contentType = context.Request.ContentType;
                    if (string.IsNullOrEmpty(contentType))
                    {
                        await HandleValidationError(context, "MissingContentType", 
                            "Content type is required for this request.");
                        return;
                    }

                    // Allow JSON and form data
                    if (!contentType.Contains("application/json") && 
                        !contentType.Contains("multipart/form-data") && 
                        !contentType.Contains("application/x-www-form-urlencoded"))
                    {
                        await HandleValidationError(context, "InvalidContentType", 
                            "Invalid content type. Please use JSON or form data.");
                        return;
                    }
                }

                // Validate query parameters for potential injection attacks
                if (context.Request.QueryString.HasValue)
                {
                    var queryString = context.Request.QueryString.Value;
                    if (ContainsSuspiciousContent(queryString))
                    {
                        await HandleValidationError(context, "InvalidQueryParameters", 
                            "Invalid query parameters detected.");
                        return;
                    }
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Validation middleware error");
                await HandleValidationError(context, "ValidationError", 
                    "A validation error occurred. Please check your request.");
            }
        }

        private async Task HandleValidationError(HttpContext context, string errorCode, string message)
        {
            var requestId = Guid.NewGuid().ToString("N")[..8];
            context.Response.Headers.Add("X-Request-ID", requestId);

            var response = ErrorHandlingService.CreateErrorResponse(errorCode, message, 
                new { RequestId = requestId }, HttpStatusCode.BadRequest);

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private bool ContainsSuspiciousContent(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var suspiciousPatterns = new[]
            {
                "<script", "</script", "javascript:", "vbscript:", "onload=", "onerror=",
                "union select", "drop table", "delete from", "insert into", "update set",
                "../", "..\\", "cmd.exe", "powershell", "bash", "sh"
            };

            var lowerInput = input.ToLowerInvariant();
            return suspiciousPatterns.Any(pattern => lowerInput.Contains(pattern));
        }
    }

    /// <summary>
    /// Extension method to register the validation middleware
    /// </summary>
    public static class ValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ValidationMiddleware>();
        }
    }
}
