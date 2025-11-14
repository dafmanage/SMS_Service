using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Implementation.Helper;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ERPSystems.Helper
{
    public static class ControllerErrorHandlingExtensions
    {
        public static async Task<IActionResult> HandleControllerAction<T>(
            this ControllerBase controller,
            Func<Task<T>> action,
            ILogger logger,
            string context,
            string? successMessage = null)
        {
            try
            {
                var result = await action();
                if (successMessage != null)
                {
                    return controller.Ok(ErrorHandlingService.CreateSuccessResponse(successMessage, result));
                }
                return controller.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in {Context}", context);
                return controller.StatusCode(500, ErrorHandlingService.HandleException(ex, logger, context));
            }
        }

        public static IActionResult HandleValidationErrors(
            this ControllerBase controller,
            ModelStateDictionary modelState)
        {
            var validationErrors = modelState.Values
                .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                .ToList();

            return controller.BadRequest(ErrorHandlingService.HandleValidationErrors(validationErrors));
        }

        public static IActionResult CreateSuccessResponse(
            this ControllerBase controller,
            string message,
            object? data = null)
        {
            return controller.Ok(ErrorHandlingService.CreateSuccessResponse(message, data));
        }

        public static IActionResult HandleDatabaseError(
            this ControllerBase controller,
            Exception ex,
            ILogger logger)
        {
            logger.LogError(ex, "Database error occurred");
            return controller.StatusCode(500, ErrorHandlingService.HandleDatabaseException(ex, logger));
        }

        public static IActionResult? ValidatePhone(
            this ControllerBase controller,
            string phoneNumber)
        {
            var (isValid, error) = InputValidationService.ValidatePhone(phoneNumber);
            if (!isValid)
            {
                return controller.BadRequest(ErrorHandlingService.CreateErrorResponse(
                    "InvalidFormat", error));
            }
            return null;
        }
    }
}
