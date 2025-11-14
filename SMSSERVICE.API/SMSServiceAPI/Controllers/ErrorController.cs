using Microsoft.AspNetCore.Mvc;

namespace SMSServiceAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ErrorController : ControllerBase
    {
        [HttpGet("{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            // Don't expose sensitive information in production
            var errorMessage = statusCode switch
            {
                404 => "The requested resource was not found.",
                403 => "Access to the requested resource is forbidden.",
                500 => "An internal server error occurred.",
                _ => "An error occurred while processing your request."
            };

            return StatusCode(statusCode, new
            {
                StatusCode = statusCode,
                Message = errorMessage,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet]
        public IActionResult DefaultError()
        {
            return Error(500);
        }
    }
} 