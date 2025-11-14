using System.ComponentModel.DataAnnotations;

namespace Implementation.DTOS.Configuration
{
    public class GeneralSettingsDto
    {
        [Required]
        [StringLength(100)]
        public string ApplicationName { get; set; } = string.Empty;

        [StringLength(200)]
        public string ApplicationDescription { get; set; } = string.Empty;

        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(200)]
        public string CompanyAddress { get; set; } = string.Empty;

        [StringLength(20)]
        public string CompanyPhone { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string CompanyEmail { get; set; } = string.Empty;

        [StringLength(50)]
        public string DefaultLanguage { get; set; } = "en";

        [StringLength(10)]
        public string DefaultCurrency { get; set; } = "ETB";

        [StringLength(50)]
        public string TimeZone { get; set; } = "Africa/Addis_Ababa";

        [Range(1, 60)]
        public int SessionTimeoutMinutes { get; set; } = 15;

        [Range(1, 10)]
        public int MaxLoginAttempts { get; set; } = 5;

        [Range(1, 60)]
        public int LockoutDurationMinutes { get; set; } = 15;

        public bool EnableEmailNotifications { get; set; } = true;

        public bool EnableSmsNotifications { get; set; } = true;

        public bool EnableAuditLogging { get; set; } = true;

        public bool EnableMaintenanceMode { get; set; } = false;

        [StringLength(500)]
        public string MaintenanceMessage { get; set; } = string.Empty;
    }

    public class SmsConfigurationDto
    {
        [Required]
        [StringLength(100)]
        public string ProviderName { get; set; } = string.Empty;

        [StringLength(200)]
        public string ApiUrl { get; set; } = string.Empty;

        [StringLength(100)]
        public string ApiKey { get; set; } = string.Empty;

        [StringLength(100)]
        public string ApiSecret { get; set; } = string.Empty;

        [StringLength(50)]
        public string SenderId { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int MaxMessageLength { get; set; } = 160;

        [Range(0.01, 10.00)]
        public decimal CostPerMessage { get; set; } = 0.50m;

        [Range(1, 100)]
        public int MaxMessagesPerMinute { get; set; } = 10;

        [Range(1, 1000)]
        public int MaxMessagesPerDay { get; set; } = 100;

        public bool EnableSandboxMode { get; set; } = true;

        [StringLength(100)]
        public string SandboxPhoneNumber { get; set; } = string.Empty;

        public bool EnableDeliveryReports { get; set; } = true;

        public bool EnableUnicodeSupport { get; set; } = true;

        [StringLength(500)]
        public string DefaultMessageTemplate { get; set; } = string.Empty;

        public bool EnableBulkMessaging { get; set; } = true;

        [Range(1, 10000)]
        public int MaxBulkMessageSize { get; set; } = 1000;
    }

    public class SmsTestDto
    {
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(160)]
        public string TestMessage { get; set; } = "Test message from SMS Service";

        public bool UseSandbox { get; set; } = true;
    }

    public class SystemInfoDto
    {
        public string ApplicationVersion { get; set; } = string.Empty;
        public string BuildNumber { get; set; } = string.Empty;
        public DateTime BuildDate { get; set; }
        public string Environment { get; set; } = string.Empty;
        public string DatabaseVersion { get; set; } = string.Empty;
        public string ServerName { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string DotNetVersion { get; set; } = string.Empty;
        public long TotalMemory { get; set; }
        public int ProcessorCount { get; set; }
        public DateTime SystemUptime { get; set; }
        public int ActiveSessions { get; set; }
        public int TotalUsers { get; set; }
        public int TotalOrganizations { get; set; }
        public long TotalMessagesSent { get; set; }
    }

    public class ConfigurationResetResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> ResetSections { get; set; } = new List<string>();
        public DateTime ResetTime { get; set; }
    }
}
