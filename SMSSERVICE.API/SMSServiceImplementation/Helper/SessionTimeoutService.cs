using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Implementation.Helper
{
    public class SessionTimeoutService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<SessionTimeoutService> _logger;
        private const int SessionTimeoutMinutes = 15; // Set to 15 minutes as requested
        private const int WarningMinutes = 3; // Show warning 3 minutes before timeout

        public SessionTimeoutService(IDistributedCache cache, ILogger<SessionTimeoutService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new user session with timeout tracking
        /// </summary>
        public async Task<string> CreateSessionAsync(string userId, string sessionId)
        {
            var sessionData = new SessionData
            {
                UserId = userId,
                SessionId = sessionId,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                IsActive = true
            };

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(SessionTimeoutMinutes + 5), // Buffer time
                SlidingExpiration = TimeSpan.FromMinutes(SessionTimeoutMinutes)
            };

            await _cache.SetStringAsync($"Session_{sessionId}", JsonSerializer.Serialize(sessionData), options);
            
            _logger.LogInformation("Session created for user {UserId} with session {SessionId}", userId, sessionId);
            
            return sessionId;
        }

        /// <summary>
        /// Updates the last activity time for a session
        /// </summary>
        public async Task<bool> UpdateSessionActivityAsync(string sessionId)
        {
            var sessionJson = await _cache.GetStringAsync($"Session_{sessionId}");
            if (string.IsNullOrEmpty(sessionJson))
                return false;

            try
            {
                var sessionData = JsonSerializer.Deserialize<SessionData>(sessionJson);
                if (sessionData == null || !sessionData.IsActive)
                    return false;

                sessionData.LastActivity = DateTime.UtcNow;

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(SessionTimeoutMinutes + 5),
                    SlidingExpiration = TimeSpan.FromMinutes(SessionTimeoutMinutes)
                };

                await _cache.SetStringAsync($"Session_{sessionId}", JsonSerializer.Serialize(sessionData), options);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session activity for session {SessionId}", sessionId);
                return false;
            }
        }

        /// <summary>
        /// Checks if a session is still valid
        /// </summary>
        public async Task<SessionValidationResult> ValidateSessionAsync(string sessionId)
        {
            var sessionJson = await _cache.GetStringAsync($"Session_{sessionId}");
            if (string.IsNullOrEmpty(sessionJson))
                return new SessionValidationResult { IsValid = false, Reason = "Session not found" };

            try
            {
                var sessionData = JsonSerializer.Deserialize<SessionData>(sessionJson);
                if (sessionData == null)
                    return new SessionValidationResult { IsValid = false, Reason = "Invalid session data" };

                if (!sessionData.IsActive)
                    return new SessionValidationResult { IsValid = false, Reason = "Session inactive" };

                var timeSinceLastActivity = DateTime.UtcNow - sessionData.LastActivity;
                var timeUntilTimeout = TimeSpan.FromMinutes(SessionTimeoutMinutes) - timeSinceLastActivity;

                if (timeSinceLastActivity >= TimeSpan.FromMinutes(SessionTimeoutMinutes))
                {
                    // Session has expired
                    await InvalidateSessionAsync(sessionId);
                    return new SessionValidationResult 
                    { 
                        IsValid = false, 
                        Reason = "Session expired",
                        TimeUntilTimeout = TimeSpan.Zero
                    };
                }

                // Check if warning should be shown
                var shouldShowWarning = timeUntilTimeout <= TimeSpan.FromMinutes(WarningMinutes);

                return new SessionValidationResult
                {
                    IsValid = true,
                    TimeUntilTimeout = timeUntilTimeout,
                    ShouldShowWarning = shouldShowWarning,
                    LastActivity = sessionData.LastActivity
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating session {SessionId}", sessionId);
                return new SessionValidationResult { IsValid = false, Reason = "Validation error" };
            }
        }

        /// <summary>
        /// Invalidates a session
        /// </summary>
        public async Task InvalidateSessionAsync(string sessionId)
        {
            await _cache.RemoveAsync($"Session_{sessionId}");
            await _cache.RemoveAsync($"UserToken_{sessionId}");
            
            _logger.LogInformation("Session {SessionId} invalidated", sessionId);
        }

        /// <summary>
        /// Gets all active sessions for a user
        /// </summary>
        public async Task<List<string>> GetUserSessionsAsync(string userId)
        {
            // This is a simplified implementation
            // In a production environment, you might want to maintain a separate index
            var sessions = new List<string>();
            
            // Note: This method would need to be implemented based on your specific caching strategy
            // For now, we'll return an empty list as this is primarily for demonstration
            
            return sessions;
        }

        /// <summary>
        /// Extends a session by the specified amount of time
        /// </summary>
        public async Task<bool> ExtendSessionAsync(string sessionId, TimeSpan extension)
        {
            var sessionJson = await _cache.GetStringAsync($"Session_{sessionId}");
            if (string.IsNullOrEmpty(sessionJson))
                return false;

            try
            {
                var sessionData = JsonSerializer.Deserialize<SessionData>(sessionJson);
                if (sessionData == null || !sessionData.IsActive)
                    return false;

                var newTimeout = SessionTimeoutMinutes + (int)extension.TotalMinutes;
                
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(newTimeout + 5),
                    SlidingExpiration = TimeSpan.FromMinutes(newTimeout)
                };

                await _cache.SetStringAsync($"Session_{sessionId}", JsonSerializer.Serialize(sessionData), options);
                
                _logger.LogInformation("Session {SessionId} extended by {Extension} minutes", sessionId, extension.TotalMinutes);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending session {SessionId}", sessionId);
                return false;
            }
        }

        /// <summary>
        /// Gets session statistics
        /// </summary>
        public async Task<SessionStatistics> GetSessionStatisticsAsync()
        {
            // This would typically query your cache or database for session statistics
            // For now, returning a basic structure
            return new SessionStatistics
            {
                TotalActiveSessions = 0, // Would be calculated from actual data
                AverageSessionDuration = TimeSpan.Zero,
                SessionsExpiredToday = 0
            };
        }
    }

    public class SessionData
    {
        public string UserId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public bool IsActive { get; set; }
    }

    public class SessionValidationResult
    {
        public bool IsValid { get; set; }
        public string Reason { get; set; } = string.Empty;
        public TimeSpan TimeUntilTimeout { get; set; }
        public bool ShouldShowWarning { get; set; }
        public DateTime LastActivity { get; set; }
    }

    public class SessionStatistics
    {
        public int TotalActiveSessions { get; set; }
        public TimeSpan AverageSessionDuration { get; set; }
        public int SessionsExpiredToday { get; set; }
    }
} 