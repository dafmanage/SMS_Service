using System.Security.Cryptography;
using System.Text;

namespace Implementation.Helper
{
    public static class SecurePasswordService
    {
        // Password complexity requirements
        private const int MinLength = 12;
        private const int MaxLength = 128;
        private const int MinUniqueChars = 3;
        
        // Password strength scoring
        private const int MinStrengthScore = 70; // 0-100 scale

        /// <summary>
        /// Validates password complexity requirements
        /// </summary>
        public static (bool isValid, List<string> errors) ValidatePasswordComplexity(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("Password cannot be empty");
                return (false, errors);
            }

            // Check length requirements
            if (password.Length < MinLength)
                errors.Add($"Password must be at least {MinLength} characters long");

            if (password.Length > MaxLength)
                errors.Add($"Password cannot exceed {MaxLength} characters");

            // Check for unique characters
            var uniqueChars = password.Distinct().Count();
            if (uniqueChars < MinUniqueChars)
                errors.Add($"Password must contain at least {MinUniqueChars} different characters");

            // Check character type requirements
            var hasUpperCase = password.Any(char.IsUpper);
            var hasLowerCase = password.Any(char.IsLower);
            var hasDigit = password.Any(char.IsDigit);
            var hasSpecialChar = password.Any(c => !char.IsLetterOrDigit(c));

            if (!hasUpperCase)
                errors.Add("Password must contain at least one uppercase letter");

            if (!hasLowerCase)
                errors.Add("Password must contain at least one lowercase letter");

            if (!hasDigit)
                errors.Add("Password must contain at least one digit");

            if (!hasSpecialChar)
                errors.Add("Password must contain at least one special character");

            // Check for common weak patterns
            if (HasWeakPatterns(password))
                errors.Add("Password contains weak patterns (e.g., repeated characters, sequences)");

            // Check for common weak passwords
            if (IsCommonWeakPassword(password))
                errors.Add("Password is too common and easily guessable");

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Calculates password strength score (0-100)
        /// </summary>
        public static int CalculatePasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return 0;

            var score = 0;

            // Length contribution (up to 25 points)
            score += Math.Min(25, password.Length * 2);

            // Character variety contribution (up to 25 points)
            var charTypes = 0;
            if (password.Any(char.IsUpper)) charTypes++;
            if (password.Any(char.IsLower)) charTypes++;
            if (password.Any(char.IsDigit)) charTypes++;
            if (password.Any(c => !char.IsLetterOrDigit(c))) charTypes++;
            score += charTypes * 6;

            // Complexity contribution (up to 25 points)
            var uniqueChars = password.Distinct().Count();
            score += Math.Min(25, uniqueChars * 2);

            // Entropy contribution (up to 25 points)
            var entropy = CalculateEntropy(password);
            score += Math.Min(25, (int)(entropy * 2));

            return Math.Min(100, score);
        }

        /// <summary>
        /// Checks if password meets minimum strength requirements
        /// </summary>
        public static bool MeetsStrengthRequirements(string password)
        {
            var (isValid, _) = ValidatePasswordComplexity(password);
            if (!isValid)
                return false;

            var strength = CalculatePasswordStrength(password);
            return strength >= MinStrengthScore;
        }

        /// <summary>
        /// Generates a secure random password
        /// </summary>
        public static string GenerateSecurePassword(int length = 16)
        {
            if (length < MinLength)
                length = MinLength;

            const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            var allChars = upperCase + lowerCase + digits + specialChars;
            var password = new StringBuilder(length);

            // Ensure at least one character from each category
            password.Append(GetRandomChar(upperCase));
            password.Append(GetRandomChar(lowerCase));
            password.Append(GetRandomChar(digits));
            password.Append(GetRandomChar(specialChars));

            // Fill the rest with random characters
            for (int i = 4; i < length; i++)
            {
                password.Append(GetRandomChar(allChars));
            }

            // Shuffle the password to avoid predictable patterns
            return ShuffleString(password.ToString());
        }

        /// <summary>
        /// Checks if password has been compromised (basic check)
        /// </summary>
        public static bool IsCompromisedPassword(string password)
        {
            // This is a basic implementation
            // In production, you should integrate with services like HaveIBeenPwned API
            
            var commonPasswords = new[]
            {
                "password", "123456", "123456789", "qwerty", "abc123",
                "password123", "admin", "letmein", "welcome", "monkey",
                "dragon", "master", "sunshine", "princess", "qwertyuiop"
            };

            return commonPasswords.Contains(password.ToLowerInvariant());
        }

        /// <summary>
        /// Provides password strength feedback
        /// </summary>
        public static string GetPasswordStrengthFeedback(string password)
        {
            var strength = CalculatePasswordStrength(password);

            if (strength >= 90)
                return "Excellent password strength";
            else if (strength >= 80)
                return "Very good password strength";
            else if (strength >= 70)
                return "Good password strength";
            else if (strength >= 60)
                return "Fair password strength";
            else if (strength >= 40)
                return "Weak password strength";
            else
                return "Very weak password strength";
        }

        #region Private Methods

        private static bool HasWeakPatterns(string password)
        {
            // Check for repeated characters
            for (int i = 0; i < password.Length - 2; i++)
            {
                if (password[i] == password[i + 1] && password[i] == password[i + 2])
                    return true;
            }

            // Check for sequential characters
            for (int i = 0; i < password.Length - 2; i++)
            {
                if (char.IsLetter(password[i]) && char.IsLetter(password[i + 1]) && char.IsLetter(password[i + 2]))
                {
                    if (password[i + 1] == password[i] + 1 && password[i + 2] == password[i] + 2)
                        return true;
                }
            }

            // Check for keyboard patterns
            var keyboardPatterns = new[] { "qwerty", "asdfgh", "zxcvbn", "123456", "654321" };
            foreach (var pattern in keyboardPatterns)
            {
                if (password.ToLowerInvariant().Contains(pattern))
                    return true;
            }

            return false;
        }

        private static bool IsCommonWeakPassword(string password)
        {
            var weakPasswords = new[]
            {
                "password", "123456", "123456789", "qwerty", "abc123",
                "password123", "admin", "letmein", "welcome", "monkey",
                "dragon", "master", "sunshine", "princess", "qwertyuiop",
                "admin123", "root", "toor", "password1", "12345678"
            };

            return weakPasswords.Contains(password.ToLowerInvariant());
        }

        private static double CalculateEntropy(string password)
        {
            var charSet = new HashSet<char>(password);
            var possibleChars = charSet.Count;
            
            if (possibleChars == 0)
                return 0;

            return Math.Log(Math.Pow(possibleChars, password.Length), 2);
        }

        private static char GetRandomChar(string chars)
        {
            return chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }

        private static string ShuffleString(string input)
        {
            var chars = input.ToCharArray();
            var random = new Random();
            
            for (int i = chars.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }
            
            return new string(chars);
        }

        #endregion
    }
} 