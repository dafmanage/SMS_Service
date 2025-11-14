using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Implementation.Services.Message;
using Implementation.Helper;
using System.Net;
using ERPSystems.Helper;

namespace SMSServiceAPI.Controllers.Message
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SmsSandboxController : ControllerBase
    {
        private readonly SmsSandboxService _sandboxService;
        private readonly ILogger<SmsSandboxController> _logger;

        public SmsSandboxController(
            SmsSandboxService sandboxService,
            ILogger<SmsSandboxController> logger)
        {
            _sandboxService = sandboxService;
            _logger = logger;
        }

        /// <summary>
        /// Send a single sandbox SMS message
        /// </summary>
        [HttpPost("send-message")]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SendSandboxMessage([FromBody] SmsSandboxRequest request)
        {
            if (!ModelState.IsValid)
            {
                return this.HandleValidationErrors(ModelState);
            }

            try
            {
                // Validate phone number
                var phoneValidation = this.ValidatePhone(request.PhoneNumber);
                if (phoneValidation != null) return phoneValidation;

                // Validate message content
                var (isValid, errors) = InputValidationService.ValidateMessageData(request.Message);
                if (!isValid)
                {
                    return BadRequest(ErrorHandlingService.HandleValidationErrors(errors));
                }

                var result = await _sandboxService.SendSandboxMessageAsync(request);
                
                if (result.Success)
                {
                    return this.CreateSuccessResponse("Sandbox message sent successfully.", new
                    {
                        MessageId = result.MessageId,
                        Cost = result.Cost,
                        DeliveryStatus = result.DeliveryStatus,
                        SentAt = result.SentAt,
                        ProviderResponse = result.ProviderResponse
                    });
                }
                else
                {
                    return BadRequest(ErrorHandlingService.CreateErrorResponse(
                        "SmsServiceUnavailable", result.Message));
                }
            }
            catch (Exception ex)
            {
                return await this.HandleControllerAction<object>(
                    () => throw ex,
                    _logger,
                    "SendSandboxMessage");
            }
        }

        /// <summary>
        /// Send bulk sandbox SMS messages
        /// </summary>
        [HttpPost("send-bulk-messages")]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SendBulkSandboxMessages([FromBody] SmsBulkSandboxRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseMessage 
                { 
                    Success = false, 
                    Message = "Invalid request data" 
                });
            }

            try
            {
                // Validate phone numbers
                var invalidPhones = request.PhoneNumbers.Where(phone => 
                    !InputValidationService.ValidatePhone(phone).isValid).ToArray();
                
                if (invalidPhones.Any())
                {
                    return BadRequest(new ResponseMessage 
                    { 
                        Success = false, 
                        Message = $"Invalid phone numbers: {string.Join(", ", invalidPhones)}" 
                    });
                }

                // Validate message content
                var (isValid, errors) = InputValidationService.ValidateMessageData(request.Message);
                if (!isValid)
                {
                    return BadRequest(new ResponseMessage 
                    { 
                        Success = false, 
                        Message = "Message validation failed", 
                        Data = errors 
                    });
                }

                var result = await _sandboxService.SendBulkSandboxMessagesAsync(request);
                
                return Ok(new ResponseMessage
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = new
                    {
                        TotalCost = result.TotalCost,
                        SuccessCount = result.SuccessCount,
                        FailureCount = result.FailureCount,
                        Results = result.Results
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk sandbox SMS messages");
                return StatusCode(500, new ResponseMessage 
                { 
                    Success = false, 
                    Message = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Get sandbox statistics
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSandboxStatistics()
        {
            try
            {
                var statistics = await _sandboxService.GetSandboxStatisticsAsync();
                
                return Ok(new ResponseMessage
                {
                    Success = true,
                    Data = statistics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sandbox statistics");
                return StatusCode(500, new ResponseMessage 
                { 
                    Success = false, 
                    Message = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Test sandbox configuration
        /// </summary>
        [HttpPost("test-configuration")]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> TestSandboxConfiguration([FromBody] SmsSandboxTestRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseMessage 
                { 
                    Success = false, 
                    Message = "Invalid request data" 
                });
            }

            try
            {
                // Validate test phone number
                if (!InputValidationService.ValidatePhone(request.TestPhoneNumber).isValid)
                {
                    return BadRequest(new ResponseMessage 
                    { 
                        Success = false, 
                        Message = "Invalid test phone number format" 
                    });
                }

                var testRequest = new SmsSandboxRequest
                {
                    PhoneNumber = request.TestPhoneNumber,
                    Message = request.TestMessage ?? "Test message from SMS Service Sandbox",
                    OrganizationId = request.OrganizationId
                };

                var result = await _sandboxService.SendSandboxMessageAsync(testRequest);
                
                return Ok(new ResponseMessage
                {
                    Success = result.Success,
                    Message = result.Success ? "Sandbox configuration test successful" : "Sandbox configuration test failed",
                    Data = new
                    {
                        TestResult = result,
                        TestTime = DateTime.UtcNow,
                        ConfigurationValid = result.Success
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing sandbox configuration");
                return StatusCode(500, new ResponseMessage 
                { 
                    Success = false, 
                    Message = "Internal server error" 
                });
            }
        }
    }

    public class SmsSandboxTestRequest
    {
        public string TestPhoneNumber { get; set; } = string.Empty;
        public string? TestMessage { get; set; }
        public Guid? OrganizationId { get; set; }
    }
}
