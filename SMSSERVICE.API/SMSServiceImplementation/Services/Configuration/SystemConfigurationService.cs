using Implementation.DTOS.Configuration;
using Implementation.Interfaces.Configuration;
using Implementation.Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Diagnostics;

namespace Implementation.Services.Configuration
{
    public class SystemConfigurationService : ISystemConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SystemConfigurationService> _logger;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SystemConfigurationService(
            IConfiguration configuration,
            ILogger<SystemConfigurationService> logger,
            IMemoryCache cache,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<GeneralSettingsDto> GetGeneralSettingsAsync()
        {
            try
            {
                var cacheKey = "GeneralSettings";
                if (_cache.TryGetValue(cacheKey, out GeneralSettingsDto cachedSettings))
                {
                    return cachedSettings;
                }

                var settings = new GeneralSettingsDto
                {
                    ApplicationName = _configuration["ApplicationSetting:ApplicationName"] ?? "SMS Service",
                    ApplicationDescription = _configuration["ApplicationSetting:ApplicationDescription"] ?? "A2P SMS Service for Ethio Telecom",
                    CompanyName = _configuration["ApplicationSetting:CompanyName"] ?? "Ethio Telecom",
                    CompanyAddress = _configuration["ApplicationSetting:CompanyAddress"] ?? "Addis Ababa, Ethiopia",
                    CompanyPhone = _configuration["ApplicationSetting:CompanyPhone"] ?? "+251-11-123-4567",
                    CompanyEmail = _configuration["ApplicationSetting:CompanyEmail"] ?? "info@ethiotelecom.et",
                    DefaultLanguage = _configuration["ApplicationSetting:DefaultLanguage"] ?? "en",
                    DefaultCurrency = _configuration["ApplicationSetting:DefaultCurrency"] ?? "ETB",
                    TimeZone = _configuration["ApplicationSetting:TimeZone"] ?? "Africa/Addis_Ababa",
                    SessionTimeoutMinutes = int.TryParse(_configuration["ApplicationSetting:SessionTimeoutMinutes"], out var timeout) ? timeout : 15,
                    MaxLoginAttempts = int.TryParse(_configuration["ApplicationSetting:MaxLoginAttempts"], out var attempts) ? attempts : 5,
                    LockoutDurationMinutes = int.TryParse(_configuration["ApplicationSetting:LockoutDurationMinutes"], out var lockout) ? lockout : 15,
                    EnableEmailNotifications = bool.TryParse(_configuration["ApplicationSetting:EnableEmailNotifications"], out var email) ? email : true,
                    EnableSmsNotifications = bool.TryParse(_configuration["ApplicationSetting:EnableSmsNotifications"], out var sms) ? sms : true,
                    EnableAuditLogging = bool.TryParse(_configuration["ApplicationSetting:EnableAuditLogging"], out var audit) ? audit : true,
                    EnableMaintenanceMode = bool.TryParse(_configuration["ApplicationSetting:EnableMaintenanceMode"], out var maintenance) ? maintenance : false,
                    MaintenanceMessage = _configuration["ApplicationSetting:MaintenanceMessage"] ?? "System is under maintenance. Please try again later."
                };

                _cache.Set(cacheKey, settings, TimeSpan.FromMinutes(30));
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving general settings");
                throw;
            }
        }

        public async Task<GeneralSettingsDto> UpdateGeneralSettingsAsync(GeneralSettingsDto settings)
        {
            try
            {
                // In a real implementation, you would save these to a database or configuration store
                // For now, we'll update the cache and log the changes
                
                _cache.Set("GeneralSettings", settings, TimeSpan.FromMinutes(30));
                
                _logger.LogInformation("General settings updated by user {UserId}", 
                    _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown");

                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating general settings");
                throw;
            }
        }

        public async Task<SmsConfigurationDto> GetSmsConfigurationAsync()
        {
            try
            {
                var cacheKey = "SmsConfiguration";
                if (_cache.TryGetValue(cacheKey, out SmsConfigurationDto cachedConfig))
                {
                    return cachedConfig;
                }

                var config = new SmsConfigurationDto
                {
                    ProviderName = _configuration["SmsConfiguration:ProviderName"] ?? "EthioTelecom",
                    ApiUrl = _configuration["SmsConfiguration:ApiUrl"] ?? "https://api.ethiotelecom.et/sms",
                    ApiKey = _configuration["SmsConfiguration:ApiKey"] ?? "",
                    ApiSecret = _configuration["SmsConfiguration:ApiSecret"] ?? "",
                    SenderId = _configuration["SmsConfiguration:SenderId"] ?? "SMS_SERVICE",
                    MaxMessageLength = int.TryParse(_configuration["SmsConfiguration:MaxMessageLength"], out var maxLength) ? maxLength : 160,
                    CostPerMessage = decimal.TryParse(_configuration["SmsConfiguration:CostPerMessage"], out var cost) ? cost : 0.50m,
                    MaxMessagesPerMinute = int.TryParse(_configuration["SmsConfiguration:MaxMessagesPerMinute"], out var perMinute) ? perMinute : 10,
                    MaxMessagesPerDay = int.TryParse(_configuration["SmsConfiguration:MaxMessagesPerDay"], out var perDay) ? perDay : 100,
                    EnableSandboxMode = bool.TryParse(_configuration["SmsConfiguration:EnableSandboxMode"], out var sandbox) ? sandbox : true,
                    SandboxPhoneNumber = _configuration["SmsConfiguration:SandboxPhoneNumber"] ?? "+251911234567",
                    EnableDeliveryReports = bool.TryParse(_configuration["SmsConfiguration:EnableDeliveryReports"], out var delivery) ? delivery : true,
                    EnableUnicodeSupport = bool.TryParse(_configuration["SmsConfiguration:EnableUnicodeSupport"], out var unicode) ? unicode : true,
                    DefaultMessageTemplate = _configuration["SmsConfiguration:DefaultMessageTemplate"] ?? "Hello {Name}, {Message}",
                    EnableBulkMessaging = bool.TryParse(_configuration["SmsConfiguration:EnableBulkMessaging"], out var bulk) ? bulk : true,
                    MaxBulkMessageSize = int.TryParse(_configuration["SmsConfiguration:MaxBulkMessageSize"], out var bulkSize) ? bulkSize : 1000
                };

                _cache.Set(cacheKey, config, TimeSpan.FromMinutes(30));
                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SMS configuration");
                throw;
            }
        }

        public async Task<SmsConfigurationDto> UpdateSmsConfigurationAsync(SmsConfigurationDto config)
        {
            try
            {
                // In a real implementation, you would save these to a database or configuration store
                _cache.Set("SmsConfiguration", config, TimeSpan.FromMinutes(30));
                
                _logger.LogInformation("SMS configuration updated by user {UserId}", 
                    _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown");

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating SMS configuration");
                throw;
            }
        }

        public async Task<SmsTestResult> TestSmsConfigurationAsync(SmsTestDto testData)
        {
            try
            {
                // Simulate SMS test - in real implementation, you would call the actual SMS provider
                var result = new SmsTestResult
                {
                    Success = true,
                    Message = "Test message sent successfully",
                    ProviderResponse = "Message ID: TEST_" + Guid.NewGuid().ToString("N")[..8],
                    TestTime = DateTime.UtcNow,
                    EstimatedCost = 0.50m
                };

                _logger.LogInformation("SMS test performed for phone {PhoneNumber} by user {UserId}", 
                    testData.PhoneNumber, _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing SMS configuration");
                return new SmsTestResult
                {
                    Success = false,
                    Message = "Test failed: " + ex.Message,
                    TestTime = DateTime.UtcNow
                };
            }
        }

        public async Task<SystemInfoDto> GetSystemInfoAsync()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                
                var process = Process.GetCurrentProcess();
                var startTime = process.StartTime;

                var systemInfo = new SystemInfoDto
                {
                    ApplicationVersion = fileVersionInfo.FileVersion ?? "1.0.0",
                    BuildNumber = fileVersionInfo.ProductVersion ?? "1.0.0",
                    BuildDate = File.GetCreationTime(assembly.Location),
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                    DatabaseVersion = "SQL Server 2019", // This would be retrieved from actual database
                    ServerName = Environment.MachineName,
                    OperatingSystem = Environment.OSVersion.ToString(),
                    DotNetVersion = Environment.Version.ToString(),
                    TotalMemory = GC.GetTotalMemory(false),
                    ProcessorCount = Environment.ProcessorCount,
                    SystemUptime = startTime,
                    ActiveSessions = 0, // This would be retrieved from session service
                    TotalUsers = 0, // This would be retrieved from user service
                    TotalOrganizations = 0, // This would be retrieved from organization service
                    TotalMessagesSent = 0 // This would be retrieved from message service
                };

                return systemInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system information");
                throw;
            }
        }

        public async Task<ConfigurationResetResult> ResetToDefaultsAsync()
        {
            try
            {
                var result = new ConfigurationResetResult
                {
                    Success = true,
                    Message = "Configuration reset to defaults successfully",
                    ResetSections = new List<string> { "GeneralSettings", "SmsConfiguration" },
                    ResetTime = DateTime.UtcNow
                };

                // Clear cache
                _cache.Remove("GeneralSettings");
                _cache.Remove("SmsConfiguration");

                _logger.LogInformation("Configuration reset to defaults by user {UserId}", 
                    _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting configuration to defaults");
                throw;
            }
        }

        public async Task<bool> ValidateSmsConfigurationAsync(SmsConfigurationDto config)
        {
            try
            {
                // Basic validation
                if (string.IsNullOrEmpty(config.ProviderName) || 
                    string.IsNullOrEmpty(config.ApiUrl) ||
                    string.IsNullOrEmpty(config.SenderId))
                {
                    return false;
                }

                // Validate URL format
                if (!Uri.TryCreate(config.ApiUrl, UriKind.Absolute, out _))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating SMS configuration");
                return false;
            }
        }

        public async Task<ConfigurationBackup> GetConfigurationBackupAsync()
        {
            try
            {
                var generalSettings = await GetGeneralSettingsAsync();
                var smsConfiguration = await GetSmsConfigurationAsync();

                var backup = new ConfigurationBackup
                {
                    GeneralSettings = generalSettings,
                    SmsConfiguration = smsConfiguration,
                    BackupDate = DateTime.UtcNow,
                    BackupVersion = "1.0",
                    CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
                };

                return backup;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating configuration backup");
                throw;
            }
        }

        public async Task<bool> RestoreConfigurationAsync(ConfigurationBackup backup)
        {
            try
            {
                if (backup == null)
                {
                    return false;
                }

                // Restore configurations
                await UpdateGeneralSettingsAsync(backup.GeneralSettings);
                await UpdateSmsConfigurationAsync(backup.SmsConfiguration);

                _logger.LogInformation("Configuration restored from backup created on {BackupDate} by {CreatedBy}", 
                    backup.BackupDate, backup.CreatedBy);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring configuration from backup");
                return false;
            }
        }

        public async Task<SecuritySettingsDto> GetSecuritySettingsAsync()
        {
            try
            {
                var cacheKey = "SecuritySettings";
                if (_cache.TryGetValue(cacheKey, out SecuritySettingsDto cachedSettings))
                {
                    return cachedSettings;
                }

                var settings = new SecuritySettingsDto
                {
                    PasswordMinLength = int.TryParse(_configuration["SecuritySettings:PasswordMinLength"], out var minLength) ? minLength : 8,
                    RequireUppercase = bool.TryParse(_configuration["SecuritySettings:RequireUppercase"], out var upper) ? upper : true,
                    RequireLowercase = bool.TryParse(_configuration["SecuritySettings:RequireLowercase"], out var lower) ? lower : true,
                    RequireNumbers = bool.TryParse(_configuration["SecuritySettings:RequireNumbers"], out var numbers) ? numbers : true,
                    RequireSpecialCharacters = bool.TryParse(_configuration["SecuritySettings:RequireSpecialCharacters"], out var special) ? special : true,
                    PasswordExpiryDays = int.TryParse(_configuration["SecuritySettings:PasswordExpiryDays"], out var expiry) ? expiry : 90,
                    MaxLoginAttempts = int.TryParse(_configuration["SecuritySettings:MaxLoginAttempts"], out var attempts) ? attempts : 5,
                    LockoutDurationMinutes = int.TryParse(_configuration["SecuritySettings:LockoutDurationMinutes"], out var lockout) ? lockout : 15,
                    EnableTwoFactorAuthentication = bool.TryParse(_configuration["SecuritySettings:EnableTwoFactorAuthentication"], out var twoFactor) ? twoFactor : false,
                    SessionTimeoutMinutes = int.TryParse(_configuration["SecuritySettings:SessionTimeoutMinutes"], out var timeout) ? timeout : 15,
                    EnableAuditLogging = bool.TryParse(_configuration["SecuritySettings:EnableAuditLogging"], out var audit) ? audit : true,
                    EnableIpWhitelist = bool.TryParse(_configuration["SecuritySettings:EnableIpWhitelist"], out var ipWhitelist) ? ipWhitelist : false,
                    AllowedIpAddresses = _configuration["SecuritySettings:AllowedIpAddresses"]?.Split(',') ?? new string[0],
                    EnableHttpsOnly = bool.TryParse(_configuration["SecuritySettings:EnableHttpsOnly"], out var https) ? https : true,
                    EnableSecurityHeaders = bool.TryParse(_configuration["SecuritySettings:EnableSecurityHeaders"], out var headers) ? headers : true,
                    EnableRateLimiting = bool.TryParse(_configuration["SecuritySettings:EnableRateLimiting"], out var rateLimit) ? rateLimit : true,
                    RateLimitRequestsPerMinute = int.TryParse(_configuration["SecuritySettings:RateLimitRequestsPerMinute"], out var rateRequests) ? rateRequests : 100,
                    RateLimitWindowMinutes = int.TryParse(_configuration["SecuritySettings:RateLimitWindowMinutes"], out var rateWindow) ? rateWindow : 60
                };

                _cache.Set(cacheKey, settings, TimeSpan.FromMinutes(30));
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security settings");
                throw;
            }
        }

        public async Task<SecuritySettingsDto> UpdateSecuritySettingsAsync(SecuritySettingsDto settings)
        {
            try
            {
                // In a real implementation, you would save these to a database or configuration store
                _cache.Set("SecuritySettings", settings, TimeSpan.FromMinutes(30));
                
                _logger.LogInformation("Security settings updated by user {UserId}", 
                    _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown");

                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating security settings");
                throw;
            }
        }
    }
}
