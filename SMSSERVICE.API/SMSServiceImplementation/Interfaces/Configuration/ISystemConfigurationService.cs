using Implementation.DTOS.Configuration;

namespace Implementation.Interfaces.Configuration
{
    public interface ISystemConfigurationService
    {
        /// <summary>
        /// Get general system settings
        /// </summary>
        Task<GeneralSettingsDto> GetGeneralSettingsAsync();

        /// <summary>
        /// Update general system settings
        /// </summary>
        Task<GeneralSettingsDto> UpdateGeneralSettingsAsync(GeneralSettingsDto settings);

        /// <summary>
        /// Get SMS configuration settings
        /// </summary>
        Task<SmsConfigurationDto> GetSmsConfigurationAsync();

        /// <summary>
        /// Update SMS configuration settings
        /// </summary>
        Task<SmsConfigurationDto> UpdateSmsConfigurationAsync(SmsConfigurationDto config);

        /// <summary>
        /// Test SMS configuration with a test message
        /// </summary>
        Task<SmsTestResult> TestSmsConfigurationAsync(SmsTestDto testData);

        /// <summary>
        /// Get system information
        /// </summary>
        Task<SystemInfoDto> GetSystemInfoAsync();

        /// <summary>
        /// Reset configuration to defaults
        /// </summary>
        Task<ConfigurationResetResult> ResetToDefaultsAsync();

        /// <summary>
        /// Validate SMS configuration
        /// </summary>
        Task<bool> ValidateSmsConfigurationAsync(SmsConfigurationDto config);

        /// <summary>
        /// Get configuration backup
        /// </summary>
        Task<ConfigurationBackup> GetConfigurationBackupAsync();

        /// <summary>
        /// Restore configuration from backup
        /// </summary>
        Task<bool> RestoreConfigurationAsync(ConfigurationBackup backup);

        /// <summary>
        /// Get security settings
        /// </summary>
        Task<SecuritySettingsDto> GetSecuritySettingsAsync();

        /// <summary>
        /// Update security settings
        /// </summary>
        Task<SecuritySettingsDto> UpdateSecuritySettingsAsync(SecuritySettingsDto settings);
    }

    public class SmsTestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ProviderResponse { get; set; } = string.Empty;
        public DateTime TestTime { get; set; }
        public decimal EstimatedCost { get; set; }
    }

    public class ConfigurationBackup
    {
        public GeneralSettingsDto GeneralSettings { get; set; } = new();
        public SmsConfigurationDto SmsConfiguration { get; set; } = new();
        public DateTime BackupDate { get; set; }
        public string BackupVersion { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }
}
