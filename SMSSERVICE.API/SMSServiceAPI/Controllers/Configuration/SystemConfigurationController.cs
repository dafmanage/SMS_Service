using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Implementation.DTOS.Configuration;
using Implementation.Interfaces.Configuration;
using Implementation.Helper;
using System.ComponentModel.DataAnnotations;
using ERPSystems.Helper;

namespace SMSServiceAPI.Controllers.Configuration
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SystemConfigurationController : ControllerBase
    {
        private readonly ISystemConfigurationService _systemConfigService;
        private readonly ILogger<SystemConfigurationController> _logger;

        public SystemConfigurationController(
            ISystemConfigurationService systemConfigService,
            ILogger<SystemConfigurationController> logger)
        {
            _systemConfigService = systemConfigService;
            _logger = logger;
        }

        /// <summary>
        /// Get general system settings
        /// </summary>
        [HttpGet("general-settings")]
        public async Task<IActionResult> GetGeneralSettings()
        {
            return await this.HandleControllerAction(
                () => _systemConfigService.GetGeneralSettingsAsync(),
                _logger,
                "GetGeneralSettings",
                "General settings retrieved successfully.");
        }

        /// <summary>
        /// Update general system settings
        /// </summary>
        [HttpPut("general-settings")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateGeneralSettings([FromBody] GeneralSettingsDto settings)
        {
            if (!ModelState.IsValid)
            {
                return this.HandleValidationErrors(ModelState);
            }

            try
            {
                // Validate input
                var (isValid, errors) = InputValidationService.ValidateGeneralSettings(settings);
                if (!isValid)
                {
                    return BadRequest(ErrorHandlingService.HandleValidationErrors(errors));
                }

                var result = await _systemConfigService.UpdateGeneralSettingsAsync(settings);
                return this.CreateSuccessResponse("General settings updated successfully.", result);
            }
            catch (Exception ex)
            {
                return this.HandleDatabaseError(ex, _logger);
            }
        }

        /// <summary>
        /// Get SMS configuration settings
        /// </summary>
        [HttpGet("sms-configuration")]
        public async Task<IActionResult> GetSmsConfiguration()
        {
            try
            {
                var config = await _systemConfigService.GetSmsConfigurationAsync();
                return Ok(new ResponseMessage { Success = true, Data = config });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SMS configuration");
                return StatusCode(500, new ResponseMessage { Success = false, Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update SMS configuration settings
        /// </summary>
        [HttpPut("sms-configuration")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateSmsConfiguration([FromBody] SmsConfigurationDto config)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseMessage { Success = false, Message = "Invalid model state" });
            }

            try
            {
                // Validate input
                var (isValid, errors) = InputValidationService.ValidateSmsConfiguration(config);
                if (!isValid)
                {
                    return BadRequest(new ResponseMessage { Success = false, Message = "Validation failed", Data = errors });
                }

                var result = await _systemConfigService.UpdateSmsConfigurationAsync(config);
                return Ok(new ResponseMessage { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating SMS configuration");
                return StatusCode(500, new ResponseMessage { Success = false, Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Test SMS configuration
        /// </summary>
        [HttpPost("sms-configuration/test")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> TestSmsConfiguration([FromBody] SmsTestDto testData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseMessage { Success = false, Message = "Invalid model state" });
            }

            try
            {
                // Validate phone number
                var (isValid, error) = InputValidationService.ValidatePhone(testData.PhoneNumber);
                if (!isValid)
                {
                    return BadRequest(new ResponseMessage { Success = false, Message = error });
                }

                var result = await _systemConfigService.TestSmsConfigurationAsync(testData);
                return Ok(new ResponseMessage { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing SMS configuration");
                return StatusCode(500, new ResponseMessage { Success = false, Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get system information (version, build info, etc.)
        /// </summary>
        [HttpGet("system-info")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetSystemInfo()
        {
            try
            {
                var info = await _systemConfigService.GetSystemInfoAsync();
                return Ok(new ResponseMessage { Success = true, Data = info });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system information");
                return StatusCode(500, new ResponseMessage { Success = false, Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get security settings
        /// </summary>
        [HttpGet("security-settings")]
        public async Task<IActionResult> GetSecuritySettings()
        {
            try
            {
                var settings = await _systemConfigService.GetSecuritySettingsAsync();
                return Ok(new ResponseMessage { Success = true, Data = settings });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security settings");
                return StatusCode(500, new ResponseMessage { Success = false, Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update security settings
        /// </summary>
        [HttpPut("security-settings")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateSecuritySettings([FromBody] SecuritySettingsDto settings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseMessage { Success = false, Message = "Invalid model state" });
            }

            try
            {
                // Validate input
                var (isValid, errors) = InputValidationService.ValidateSecuritySettings(settings);
                if (!isValid)
                {
                    return BadRequest(new ResponseMessage { Success = false, Message = "Validation failed", Data = errors });
                }

                var result = await _systemConfigService.UpdateSecuritySettingsAsync(settings);
                return Ok(new ResponseMessage { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating security settings");
                return StatusCode(500, new ResponseMessage { Success = false, Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Reset system configuration to defaults
        /// </summary>
        [HttpPost("reset-to-defaults")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ResetToDefaults()
        {
            try
            {
                var result = await _systemConfigService.ResetToDefaultsAsync();
                return Ok(new ResponseMessage { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting configuration to defaults");
                return StatusCode(500, new ResponseMessage { Success = false, Message = "Internal server error" });
            }
        }
    }
}
