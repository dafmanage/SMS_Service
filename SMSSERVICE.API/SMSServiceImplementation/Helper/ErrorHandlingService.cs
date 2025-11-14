using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Implementation.Helper
{
    public static class ErrorHandlingService
    {
        /// <summary>
        /// User-friendly error messages for common scenarios
        /// </summary>
        private static readonly Dictionary<string, string> UserFriendlyMessages = new()
        {
            // Authentication errors
            { "InvalidCredentials", "The username or password you entered is incorrect. Please try again." },
            { "AccountLocked", "Your account has been temporarily locked due to multiple failed login attempts. Please try again later." },
            { "AccountDisabled", "Your account has been disabled. Please contact your administrator." },
            { "SessionExpired", "Your session has expired. Please log in again to continue." },
            { "InvalidToken", "Your session is invalid. Please log in again." },
            
            // Validation errors
            { "RequiredField", "This field is required and cannot be empty." },
            { "InvalidEmail", "Please enter a valid email address." },
            { "InvalidPhone", "Please enter a valid phone number." },
            { "InvalidFormat", "The format of this field is incorrect." },
            { "TooShort", "This field is too short. Please enter more characters." },
            { "TooLong", "This field is too long. Please enter fewer characters." },
            { "InvalidCharacters", "This field contains invalid characters." },
            
            // File upload errors
            { "FileTooLarge", "The file you uploaded is too large. Please choose a smaller file." },
            { "InvalidFileType", "The file type is not supported. Please upload an image or Excel file." },
            { "FileUploadFailed", "Failed to upload the file. Please try again." },
            
            // Database errors
            { "RecordNotFound", "The requested information could not be found." },
            { "DuplicateRecord", "This information already exists. Please check and try again." },
            { "DatabaseError", "A database error occurred. Please try again later." },
            { "ConcurrencyError", "The information has been modified by another user. Please refresh and try again." },
            
            // Permission errors
            { "AccessDenied", "You don't have permission to perform this action." },
            { "InsufficientPermissions", "Your account doesn't have the required permissions for this operation." },
            { "OrganizationAccessDenied", "You don't have access to this organization's data." },
            
            // SMS service errors
            { "SmsServiceUnavailable", "SMS service is currently unavailable. Please try again later." },
            { "InvalidPhoneNumber", "The phone number format is invalid." },
            { "MessageTooLong", "Your message is too long. Please shorten it and try again." },
            { "DailyLimitExceeded", "You have reached your daily SMS limit. Please try again tomorrow." },
            { "InsufficientBalance", "Insufficient balance to send SMS messages." },
            
            // System errors
            { "InternalServerError", "An unexpected error occurred. Please try again later." },
            { "ServiceUnavailable", "The service is temporarily unavailable. Please try again later." },
            { "NetworkError", "A network error occurred. Please check your connection and try again." },
            { "TimeoutError", "The request timed out. Please try again." },
            
            // Configuration errors
            { "ConfigurationError", "System configuration error. Please contact support." },
            { "InvalidConfiguration", "Invalid system configuration. Please contact your administrator." }
        };

        /// <summary>
        /// Get user-friendly error message for a specific error code
        /// </summary>
        public static string GetUserFriendlyMessage(string errorCode, string? customMessage = null)
        {
            if (!string.IsNullOrEmpty(customMessage))
                return customMessage;

            return UserFriendlyMessages.TryGetValue(errorCode, out var message) 
                ? message 
                : "An unexpected error occurred. Please try again.";
        }

        /// <summary>
        /// Create a user-friendly error response
        /// </summary>
        public static ResponseMessage CreateErrorResponse(
            string errorCode, 
            string? customMessage = null, 
            object? additionalData = null,
            HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new ResponseMessage
            {
                Success = false,
                Message = GetUserFriendlyMessage(errorCode, customMessage),
                ErrorCode = (int)statusCode,
                Data = additionalData,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Handle validation errors and convert to user-friendly messages
        /// </summary>
        public static ResponseMessage HandleValidationErrors(IEnumerable<string> validationErrors)
        {
            var userFriendlyErrors = validationErrors.Select(error => 
            {
                // Convert technical validation errors to user-friendly messages
                if (error.Contains("required", StringComparison.OrdinalIgnoreCase))
                    return "This field is required.";
                if (error.Contains("email", StringComparison.OrdinalIgnoreCase))
                    return "Please enter a valid email address.";
                if (error.Contains("phone", StringComparison.OrdinalIgnoreCase))
                    return "Please enter a valid phone number.";
                if (error.Contains("length", StringComparison.OrdinalIgnoreCase))
                    return "The length of this field is incorrect.";
                if (error.Contains("format", StringComparison.OrdinalIgnoreCase))
                    return "The format of this field is incorrect.";
                
                return error; // Return original if no specific conversion found
            }).ToList();

            return new ResponseMessage
            {
                Success = false,
                Message = "Please correct the following errors:",
                ErrorCode = 400,
                Data = userFriendlyErrors,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Handle exceptions and convert to user-friendly responses
        /// </summary>
        public static ResponseMessage HandleException(Exception ex, ILogger logger, string? context = null)
        {
            // Log the technical error for debugging
            logger.LogError(ex, "Error occurred in {Context}", context ?? "Unknown context");

            // Determine user-friendly message based on exception type
            return ex switch
            {
                ArgumentNullException => CreateErrorResponse("RequiredField", "Required information is missing."),
                ArgumentException => CreateErrorResponse("InvalidFormat", "The provided information is invalid."),
                UnauthorizedAccessException => CreateErrorResponse("AccessDenied", "You don't have permission to perform this action."),
                FileNotFoundException => CreateErrorResponse("RecordNotFound", "The requested file could not be found."),
                DirectoryNotFoundException => CreateErrorResponse("RecordNotFound", "The requested directory could not be found."),
                InvalidOperationException => CreateErrorResponse("InvalidOperation", "This operation cannot be performed at this time."),
                TimeoutException => CreateErrorResponse("TimeoutError", "The operation timed out. Please try again."),
                HttpRequestException => CreateErrorResponse("NetworkError", "A network error occurred. Please check your connection."),
                JsonException => CreateErrorResponse("InvalidFormat", "The data format is invalid."),
                NotSupportedException => CreateErrorResponse("ServiceUnavailable", "This feature is not currently supported."),
                _ => CreateErrorResponse("InternalServerError", "An unexpected error occurred. Please try again later.")
            };
        }

        /// <summary>
        /// Handle database-specific exceptions
        /// </summary>
        public static ResponseMessage HandleDatabaseException(Exception ex, ILogger logger)
        {
            logger.LogError(ex, "Database error occurred");

            return ex.Message.ToLowerInvariant() switch
            {
                var msg when msg.Contains("duplicate") || msg.Contains("unique") => 
                    CreateErrorResponse("DuplicateRecord", "This information already exists. Please check and try again."),
                var msg when msg.Contains("not found") || msg.Contains("does not exist") => 
                    CreateErrorResponse("RecordNotFound", "The requested information could not be found."),
                var msg when msg.Contains("concurrency") || msg.Contains("conflict") => 
                    CreateErrorResponse("ConcurrencyError", "The information has been modified by another user. Please refresh and try again."),
                var msg when msg.Contains("timeout") => 
                    CreateErrorResponse("TimeoutError", "The database operation timed out. Please try again."),
                var msg when msg.Contains("connection") => 
                    CreateErrorResponse("ServiceUnavailable", "Database connection error. Please try again later."),
                _ => CreateErrorResponse("DatabaseError", "A database error occurred. Please try again later.")
            };
        }

        /// <summary>
        /// Handle file operation exceptions
        /// </summary>
        public static ResponseMessage HandleFileException(Exception ex, ILogger logger)
        {
            logger.LogError(ex, "File operation error occurred");

            return ex switch
            {
                FileNotFoundException => CreateErrorResponse("RecordNotFound", "The requested file could not be found."),
                DirectoryNotFoundException => CreateErrorResponse("RecordNotFound", "The requested directory could not be found."),
                UnauthorizedAccessException => CreateErrorResponse("AccessDenied", "You don't have permission to access this file."),
                IOException when ex.Message.Contains("space") => 
                    CreateErrorResponse("FileUploadFailed", "Insufficient disk space. Please contact support."),
                IOException when ex.Message.Contains("locked") => 
                    CreateErrorResponse("FileUploadFailed", "The file is currently in use. Please try again later."),
                _ => CreateErrorResponse("FileUploadFailed", "A file operation error occurred. Please try again.")
            };
        }

        /// <summary>
        /// Handle authentication/authorization exceptions
        /// </summary>
        public static ResponseMessage HandleAuthException(Exception ex, ILogger logger)
        {
            logger.LogError(ex, "Authentication/Authorization error occurred");

            return ex.Message.ToLowerInvariant() switch
            {
                var msg when msg.Contains("invalid") && msg.Contains("credentials") => 
                    CreateErrorResponse("InvalidCredentials", "The username or password you entered is incorrect."),
                var msg when msg.Contains("locked") || msg.Contains("disabled") => 
                    CreateErrorResponse("AccountLocked", "Your account has been temporarily locked. Please try again later."),
                var msg when msg.Contains("expired") || msg.Contains("invalid") && msg.Contains("token") => 
                    CreateErrorResponse("SessionExpired", "Your session has expired. Please log in again."),
                var msg when msg.Contains("access") && msg.Contains("denied") => 
                    CreateErrorResponse("AccessDenied", "You don't have permission to perform this action."),
                _ => CreateErrorResponse("AccessDenied", "Authentication error. Please log in again.")
            };
        }

        /// <summary>
        /// Create a success response with user-friendly message
        /// </summary>
        public static ResponseMessage CreateSuccessResponse(string message, object? data = null)
        {
            return new ResponseMessage
            {
                Success = true,
                Message = message,
                ErrorCode = 0,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Validate and sanitize error messages to prevent information disclosure
        /// </summary>
        public static string SanitizeErrorMessage(string errorMessage)
        {
            // Remove technical details that shouldn't be exposed to users
            var sanitized = errorMessage
                .Replace("System.", "", StringComparison.OrdinalIgnoreCase)
                .Replace("Microsoft.", "", StringComparison.OrdinalIgnoreCase)
                .Replace("Exception", "Error", StringComparison.OrdinalIgnoreCase)
                .Replace("StackTrace", "", StringComparison.OrdinalIgnoreCase)
                .Replace("InnerException", "", StringComparison.OrdinalIgnoreCase);

            // Remove file paths and technical identifiers
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[A-Za-z]:\\[^\\]+", "[PATH]");
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"\b[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}\b", "[ID]");

            return sanitized;
        }
    }

}
