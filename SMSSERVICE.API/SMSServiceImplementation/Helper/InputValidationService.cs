using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Http;
using Implementation.DTOS.Configuration;

namespace Implementation.Helper
{
    public static class InputValidationService
    {
        // Common validation patterns
        private static readonly Regex EmailPattern = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
        private static readonly Regex PhonePattern = new Regex(@"^(\+251|0)?[0-9]{9}$", RegexOptions.Compiled);
        private static readonly Regex NamePattern = new Regex(@"^[a-zA-Z\s\-'\.]+$", RegexOptions.Compiled);
        private static readonly Regex AlphanumericPattern = new Regex(@"^[a-zA-Z0-9\s\-_\.]+$", RegexOptions.Compiled);
        private static readonly Regex NumericPattern = new Regex(@"^[0-9]+$", RegexOptions.Compiled);
        
        // Maximum lengths for different field types
        private const int MaxNameLength = 100;
        private const int MaxDescriptionLength = 500;
        private const int MaxEmailLength = 254;
        private const int MaxPhoneLength = 20;
        private const int MaxAddressLength = 200;

        /// <summary>
        /// Validates and sanitizes email addresses
        /// </summary>
        public static (bool isValid, string sanitizedValue) ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, string.Empty);

            var trimmedEmail = email.Trim();
            
            if (trimmedEmail.Length > MaxEmailLength)
                return (false, string.Empty);

            if (!EmailPattern.IsMatch(trimmedEmail))
                return (false, string.Empty);

            return (true, trimmedEmail.ToLowerInvariant());
        }

        /// <summary>
        /// Validates and sanitizes phone numbers
        /// </summary>
        public static (bool isValid, string sanitizedValue) ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return (false, string.Empty);

            var trimmedPhone = phone.Trim();
            
            if (trimmedPhone.Length > MaxPhoneLength)
                return (false, string.Empty);

            if (!PhonePattern.IsMatch(trimmedPhone))
                return (false, string.Empty);

            return (true, trimmedPhone);
        }

        /// <summary>
        /// Validates and sanitizes names (first names, last names, organization names)
        /// </summary>
        public static (bool isValid, string sanitizedValue) ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, string.Empty);

            var trimmedName = name.Trim();
            
            if (trimmedName.Length > MaxNameLength)
                return (false, string.Empty);

            if (!NamePattern.IsMatch(trimmedName))
                return (false, string.Empty);

            return (true, trimmedName);
        }

        /// <summary>
        /// Validates and sanitizes alphanumeric text
        /// </summary>
        public static (bool isValid, string sanitizedValue) ValidateAlphanumeric(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return (false, string.Empty);

            var trimmedText = text.Trim();
            
            if (trimmedText.Length > MaxDescriptionLength)
                return (false, string.Empty);

            if (!AlphanumericPattern.IsMatch(trimmedText))
                return (false, string.Empty);

            return (true, trimmedText);
        }

        /// <summary>
        /// Validates and sanitizes numeric values
        /// </summary>
        public static (bool isValid, string sanitizedValue) ValidateNumeric(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return (false, string.Empty);

            var trimmedValue = value.Trim();
            
            if (!NumericPattern.IsMatch(trimmedValue))
                return (false, string.Empty);

            return (true, trimmedValue);
        }

        /// <summary>
        /// Validates and sanitizes addresses
        /// </summary>
        public static (bool isValid, string sanitizedValue) ValidateAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return (false, string.Empty);

            var trimmedAddress = address.Trim();
            
            if (trimmedAddress.Length > MaxAddressLength)
                return (false, string.Empty);

            // Allow common address characters
            var addressPattern = new Regex(@"^[a-zA-Z0-9\s\-_\.\,\#\&\(\)\/]+$", RegexOptions.Compiled);
            if (!addressPattern.IsMatch(trimmedAddress))
                return (false, string.Empty);

            return (true, trimmedAddress);
        }

        /// <summary>
        /// Sanitizes HTML content to prevent XSS attacks
        /// </summary>
        public static string SanitizeHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Remove potentially dangerous HTML tags and attributes
            var dangerousTags = new[] { "script", "iframe", "object", "embed", "form", "input", "textarea", "select", "button" };
            var dangerousAttributes = new[] { "onclick", "onload", "onerror", "onmouseover", "onfocus", "javascript:" };

            var sanitized = html;

            // Remove dangerous tags
            foreach (var tag in dangerousTags)
            {
                var pattern = $@"<{tag}[^>]*>.*?</{tag}>|<{tag}[^>]*/?>";
                sanitized = Regex.Replace(sanitized, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }

            // Remove dangerous attributes
            foreach (var attr in dangerousAttributes)
            {
                var pattern = $@"\s+{attr}\s*=\s*[""'][^""']*[""']";
                sanitized = Regex.Replace(sanitized, pattern, "", RegexOptions.IgnoreCase);
            }

            // HTML encode the result
            return HttpUtility.HtmlEncode(sanitized);
        }

        /// <summary>
        /// Validates file uploads
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateFileUpload(IFormFile file, string[] allowedExtensions, long maxSizeInBytes)
        {
            if (file == null || file.Length == 0)
                return (false, "No file provided");

            if (file.Length > maxSizeInBytes)
                return (false, $"File size exceeds maximum allowed size of {maxSizeInBytes / (1024 * 1024)} MB");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return (false, $"File type not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");

            // Check for potentially dangerous file types
            var dangerousExtensions = new[] { ".exe", ".bat", ".cmd", ".com", ".pif", ".scr", ".vbs", ".js", ".jar", ".msi" };
            if (dangerousExtensions.Contains(extension))
                return (false, "This file type is not allowed for security reasons");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validates and sanitizes URLs
        /// </summary>
        public static (bool isValid, string sanitizedValue) ValidateUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return (false, string.Empty);

            var trimmedUrl = url.Trim();
            
            if (!Uri.TryCreate(trimmedUrl, UriKind.Absolute, out var uri))
                return (false, string.Empty);

            // Only allow HTTP and HTTPS protocols
            if (uri.Scheme != "http" && uri.Scheme != "https")
                return (false, string.Empty);

            return (true, trimmedUrl);
        }

        /// <summary>
        /// Validates GUID strings
        /// </summary>
        public static bool ValidateGuid(string guidString)
        {
            if (string.IsNullOrWhiteSpace(guidString))
                return false;

            return Guid.TryParse(guidString, out _);
        }

        /// <summary>
        /// Validates date strings
        /// </summary>
        public static bool ValidateDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return false;

            return DateTime.TryParse(dateString, out _);
        }

        /// <summary>
        /// Comprehensive validation for organization data
        /// </summary>
        public static (bool isValid, List<string> errors) ValidateOrganizationData(string name, string nameLocal, string phone, string email, string address)
        {
            var errors = new List<string>();

            var (nameValid, _) = ValidateName(name);
            if (!nameValid)
                errors.Add("Invalid organization name");

            var (nameLocalValid, _) = ValidateName(nameLocal);
            if (!nameLocalValid)
                errors.Add("Invalid local organization name");

            var (phoneValid, _) = ValidatePhone(phone);
            if (!phoneValid)
                errors.Add("Invalid phone number");

            var (emailValid, _) = ValidateEmail(email);
            if (!emailValid)
                errors.Add("Invalid email address");

            var (addressValid, _) = ValidateAddress(address);
            if (!addressValid)
                errors.Add("Invalid address");

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Comprehensive validation for message data
        /// </summary>
        public static (bool isValid, List<string> errors) ValidateMessageData(string content, string? subject = null)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(content))
                errors.Add("Message content is required");
            else if (content.Length > 1000) // Reasonable message length limit
                errors.Add("Message content is too long (maximum 1000 characters)");

            if (!string.IsNullOrWhiteSpace(subject))
            {
                var (subjectValid, _) = ValidateAlphanumeric(subject);
                if (!subjectValid)
                    errors.Add("Invalid message subject");
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Validates general settings data
        /// </summary>
        public static (bool isValid, List<string> errors) ValidateGeneralSettings(object settings)
        {
            var errors = new List<string>();

            if (settings == null)
            {
                errors.Add("Settings data is required");
                return (false, errors);
            }

            // Use reflection to validate properties
            var properties = settings.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(settings);
                var propName = prop.Name;

                switch (propName)
                {
                    case "ApplicationName":
                    case "CompanyName":
                        if (string.IsNullOrWhiteSpace(value?.ToString()))
                        {
                            errors.Add($"{propName} is required");
                        }
                        else if (value.ToString()!.Length > MaxNameLength)
                        {
                            errors.Add($"{propName} cannot exceed {MaxNameLength} characters");
                        }
                        break;

                    case "CompanyEmail":
                        if (!string.IsNullOrWhiteSpace(value?.ToString()))
                        {
                            var (isValid, _) = ValidateEmail(value.ToString()!);
                            if (!isValid)
                            {
                                errors.Add("Invalid email format");
                            }
                        }
                        break;

                    case "CompanyPhone":
                        if (!string.IsNullOrWhiteSpace(value?.ToString()))
                        {
                            var (isValid, _) = ValidatePhone(value.ToString()!);
                            if (!isValid)
                            {
                                errors.Add("Invalid phone number format");
                            }
                        }
                        break;

                    case "SessionTimeoutMinutes":
                        if (value != null && (int)value < 1 || (int)value > 60)
                        {
                            errors.Add("Session timeout must be between 1 and 60 minutes");
                        }
                        break;

                    case "MaxLoginAttempts":
                        if (value != null && (int)value < 1 || (int)value > 10)
                        {
                            errors.Add("Max login attempts must be between 1 and 10");
                        }
                        break;
                }
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Validates SMS configuration data
        /// </summary>
        public static (bool isValid, List<string> errors) ValidateSmsConfiguration(object config)
        {
            var errors = new List<string>();

            if (config == null)
            {
                errors.Add("SMS configuration data is required");
                return (false, errors);
            }

            // Use reflection to validate properties
            var properties = config.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(config);
                var propName = prop.Name;

                switch (propName)
                {
                    case "ProviderName":
                    case "SenderId":
                        if (string.IsNullOrWhiteSpace(value?.ToString()))
                        {
                            errors.Add($"{propName} is required");
                        }
                        break;

                    case "ApiUrl":
                        if (string.IsNullOrWhiteSpace(value?.ToString()))
                        {
                            errors.Add("API URL is required");
                        }
                        else if (!Uri.TryCreate(value.ToString(), UriKind.Absolute, out _))
                        {
                            errors.Add("Invalid API URL format");
                        }
                        break;

                    case "MaxMessageLength":
                        if (value != null && (int)value < 1 || (int)value > 1000)
                        {
                            errors.Add("Max message length must be between 1 and 1000 characters");
                        }
                        break;

                    case "CostPerMessage":
                        if (value != null && (decimal)value < 0.01m || (decimal)value > 10.00m)
                        {
                            errors.Add("Cost per message must be between 0.01 and 10.00");
                        }
                        break;

                    case "MaxMessagesPerMinute":
                        if (value != null && (int)value < 1 || (int)value > 100)
                        {
                            errors.Add("Max messages per minute must be between 1 and 100");
                        }
                        break;

                    case "MaxMessagesPerDay":
                        if (value != null && (int)value < 1 || (int)value > 1000)
                        {
                            errors.Add("Max messages per day must be between 1 and 1000");
                        }
                        break;

                    case "SandboxPhoneNumber":
                        if (!string.IsNullOrWhiteSpace(value?.ToString()))
                        {
                            var (isValid, _) = ValidatePhone(value.ToString()!);
                            if (!isValid)
                            {
                                errors.Add("Invalid sandbox phone number format");
                            }
                        }
                        break;
                }
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Validates file upload for images and Excel only
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateImageAndExcelUpload(IFormFile file)
        {
            if (file == null)
                return (false, "No file provided");

            if (file.Length == 0)
                return (false, "File is empty");

            // Check file size (10MB limit)
            if (file.Length > 10 * 1024 * 1024)
                return (false, "File size cannot exceed 10MB");

            // Get file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            // Allowed extensions for images and Excel
            var allowedExtensions = new[]
            {
                ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", // Images
                ".xls", ".xlsx", ".csv" // Excel/CSV files
            };

            if (!allowedExtensions.Contains(extension))
                return (false, "Only image files (JPG, PNG, GIF, BMP, WEBP) and Excel files (XLS, XLSX, CSV) are allowed");

            // Check MIME type for additional security
            var allowedMimeTypes = new[]
            {
                "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp",
                "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "text/csv", "application/csv"
            };

            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return (false, "Invalid file type detected");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validates password reset data
        /// </summary>
        public static (bool isValid, List<string> errors) ValidatePasswordReset(string currentPassword, string newPassword, string confirmPassword)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(currentPassword))
                errors.Add("Current password is required");

            if (string.IsNullOrWhiteSpace(newPassword))
                errors.Add("New password is required");
            else if (newPassword.Length < 8)
                errors.Add("New password must be at least 8 characters long");

            if (string.IsNullOrWhiteSpace(confirmPassword))
                errors.Add("Password confirmation is required");
            else if (newPassword != confirmPassword)
                errors.Add("Passwords do not match");

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Validates user profile update data
        /// </summary>
        public static (bool isValid, List<string> errors) ValidateUserProfile(object profile)
        {
            var errors = new List<string>();

            if (profile == null)
            {
                errors.Add("Profile data is required");
                return (false, errors);
            }

            // Use reflection to validate properties
            var properties = profile.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(profile);
                var propName = prop.Name;

                switch (propName)
                {
                    case "FirstName":
                    case "LastName":
                        if (string.IsNullOrWhiteSpace(value?.ToString()))
                        {
                            errors.Add($"{propName} is required");
                        }
                        else
                        {
                            var (isValid, _) = ValidateName(value.ToString()!);
                            if (!isValid)
                            {
                                errors.Add($"Invalid {propName} format");
                            }
                        }
                        break;

                    case "Email":
                        if (string.IsNullOrWhiteSpace(value?.ToString()))
                        {
                            errors.Add("Email is required");
                        }
                        else
                        {
                            var (isValid, _) = ValidateEmail(value.ToString()!);
                            if (!isValid)
                            {
                                errors.Add("Invalid email format");
                            }
                        }
                        break;

                    case "PhoneNumber":
                        if (!string.IsNullOrWhiteSpace(value?.ToString()))
                        {
                            var (isValid, _) = ValidatePhone(value.ToString()!);
                            if (!isValid)
                            {
                                errors.Add("Invalid phone number format");
                            }
                        }
                        break;
                }
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Validates security settings
        /// </summary>
        public static (bool isValid, List<string> errors) ValidateSecuritySettings(SecuritySettingsDto settings)
        {
            var errors = new List<string>();

            if (settings == null)
            {
                errors.Add("Security settings cannot be null");
                return (false, errors);
            }

            // Validate password settings
            if (settings.PasswordMinLength < 6 || settings.PasswordMinLength > 20)
                errors.Add("Password minimum length must be between 6 and 20 characters");

            if (settings.PasswordExpiryDays < 30 || settings.PasswordExpiryDays > 365)
                errors.Add("Password expiry must be between 30 and 365 days");

            // Validate login attempt settings
            if (settings.MaxLoginAttempts < 3 || settings.MaxLoginAttempts > 10)
                errors.Add("Max login attempts must be between 3 and 10");

            if (settings.LockoutDurationMinutes < 5 || settings.LockoutDurationMinutes > 60)
                errors.Add("Lockout duration must be between 5 and 60 minutes");

            // Validate session settings
            if (settings.SessionTimeoutMinutes < 5 || settings.SessionTimeoutMinutes > 120)
                errors.Add("Session timeout must be between 5 and 120 minutes");

            // Validate rate limiting settings
            if (settings.RateLimitRequestsPerMinute < 10 || settings.RateLimitRequestsPerMinute > 1000)
                errors.Add("Rate limit requests must be between 10 and 1000");

            if (settings.RateLimitWindowMinutes < 1 || settings.RateLimitWindowMinutes > 3600)
                errors.Add("Rate limit window must be between 1 and 3600 seconds");

            // Validate IP addresses if whitelist is enabled
            if (settings.EnableIpWhitelist && settings.AllowedIpAddresses != null)
            {
                foreach (var ip in settings.AllowedIpAddresses)
                {
                    if (!string.IsNullOrWhiteSpace(ip) && !ValidateIpAddress(ip))
                    {
                        errors.Add($"Invalid IP address format: {ip}");
                    }
                }
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Validates IP address format
        /// </summary>
        public static bool ValidateIpAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            var parts = ipAddress.Split('.');
            if (parts.Length != 4)
                return false;

            foreach (var part in parts)
            {
                if (!int.TryParse(part, out var num) || num < 0 || num > 255)
                    return false;
            }

            return true;
        }
    }
} 