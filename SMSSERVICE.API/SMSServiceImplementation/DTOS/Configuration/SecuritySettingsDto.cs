using System.ComponentModel.DataAnnotations;

namespace Implementation.DTOS.Configuration
{
    public class SecuritySettingsDto
    {
        [Required]
        [Range(6, 20, ErrorMessage = "Password minimum length must be between 6 and 20 characters")]
        public int PasswordMinLength { get; set; } = 8;

        [Required]
        public bool RequireUppercase { get; set; } = true;

        [Required]
        public bool RequireLowercase { get; set; } = true;

        [Required]
        public bool RequireNumbers { get; set; } = true;

        [Required]
        public bool RequireSpecialCharacters { get; set; } = true;

        [Required]
        [Range(30, 365, ErrorMessage = "Password expiry must be between 30 and 365 days")]
        public int PasswordExpiryDays { get; set; } = 90;

        [Required]
        [Range(3, 10, ErrorMessage = "Max login attempts must be between 3 and 10")]
        public int MaxLoginAttempts { get; set; } = 5;

        [Required]
        [Range(5, 60, ErrorMessage = "Lockout duration must be between 5 and 60 minutes")]
        public int LockoutDurationMinutes { get; set; } = 15;

        [Required]
        public bool EnableTwoFactorAuthentication { get; set; } = false;

        [Required]
        [Range(5, 120, ErrorMessage = "Session timeout must be between 5 and 120 minutes")]
        public int SessionTimeoutMinutes { get; set; } = 15;

        [Required]
        public bool EnableAuditLogging { get; set; } = true;

        [Required]
        public bool EnableIpWhitelist { get; set; } = false;

        public string[] AllowedIpAddresses { get; set; } = new string[0];

        [Required]
        public bool EnableHttpsOnly { get; set; } = true;

        [Required]
        public bool EnableSecurityHeaders { get; set; } = true;

        [Required]
        public bool EnableRateLimiting { get; set; } = true;

        [Required]
        [Range(10, 1000, ErrorMessage = "Rate limit requests must be between 10 and 1000")]
        public int RateLimitRequestsPerMinute { get; set; } = 100;

        [Required]
        [Range(1, 3600, ErrorMessage = "Rate limit window must be between 1 and 3600 seconds")]
        public int RateLimitWindowMinutes { get; set; } = 60;
    }
}
