using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Implementation.DTOS.Authentication;
using Implementation.Interfaces.Authentication;
using Implementation.Helper;
using System.Net;
using IntegratedInfrustructure.Model.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Distributed;
using IntegratedImplementation.DTOS.Configuration;
using Microsoft.Extensions.Logging;


namespace ERPSystems.Controllers.Authentication
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        IAuthenticationService _authenticationService;
        private readonly ILogger<AuthenticationController> _logger;
        private SignInManager<ApplicationUser> _signInManager;
        private readonly IDistributedCache _cache;


        public AuthenticationController(IAuthenticationService authenticationService, SignInManager<ApplicationUser> signInManager,
            IDistributedCache cache, ILogger<AuthenticationController> logger)
        {
            _authenticationService = authenticationService;
            _signInManager = signInManager;
            _cache = cache;
            _logger = logger;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _authenticationService.Login(loginDto);
                    return Ok(result);
                }
                else
                {
                    var validationErrors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .ToList();
                    
                    return BadRequest(ErrorHandlingService.HandleValidationErrors(validationErrors));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ErrorHandlingService.HandleAuthException(ex, _logger));
            }
        }


        [HttpPost]
        public async Task<ActionResult<ResponseMessage>> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sessionId = User.FindFirst("SessionId")?.Value;
                
                if (userId != null)
                {
                    // Remove user session from cache
                    await _cache.RemoveAsync($"UserSession_{userId}");
                    
                    // Remove session data
                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        await _cache.RemoveAsync($"Session_{sessionId}");
                        await _cache.RemoveAsync($"UserToken_{sessionId}");
                    }
                    
                    // Clear all user-related cache entries
                    await _cache.RemoveAsync($"UserProfile_{userId}");
                    await _cache.RemoveAsync($"UserRoles_{userId}");
                }

                // Sign out the user
                await _signInManager.SignOutAsync();
                
                // Add security headers to prevent back button bypass
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");
                Response.Headers.Add("Clear-Site-Data", "\"cache\", \"cookies\", \"storage\"");
                
                return Ok(new ResponseMessage
                {
                    Success = true,
                    ErrorCode = 0,
                    Message = "Logged out successfully",
                    Data = new { 
                        LogoutTime = DateTime.UtcNow,
                        SecurityHeaders = "Cache control headers added to prevent back button bypass"
                    }
                });
            }
            catch (Exception ex)
            {
                // Log the error but still attempt to sign out
                await _signInManager.SignOutAsync();
                
                return Ok(new ResponseMessage
                {
                    Success = true,
                    ErrorCode = 0,
                    Message = "Logged out successfully (with warnings)",
                    Data = new { 
                        LogoutTime = DateTime.UtcNow,
                        Warning = "Some cleanup operations may have failed"
                    }
                });
            }
        }


        [HttpGet]
        [Authorize(Policy = "ValidToken")]
        [ProducesResponseType(typeof(UserListDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserList()
        {
            Console.WriteLine("DEBUG: GetUserList controller method called");
            var result = await _authenticationService.GetUserList();
            Console.WriteLine($"DEBUG: Controller returning {result.Count} users");
            return Ok(result);
        }

        [HttpGet("test-users")]
        [ProducesResponseType(typeof(UserListDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> TestUsers()
        {
            Console.WriteLine("DEBUG: TestUsers endpoint called");
            var result = await _authenticationService.GetAllUsersForTesting();
            Console.WriteLine($"DEBUG: TestUsers returning {result.Count} users");
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(RoleDropDown), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRoleCategory()
        {
            return Ok(await _authenticationService.GetRoleCategory());
        }

        [HttpGet]
        [ProducesResponseType(typeof(RoleDropDown), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetNotAssignedRole(string userId)
        {
            return Ok(await _authenticationService.GetNotAssignedRole(userId));
        }

        [HttpGet]
        [ProducesResponseType(typeof(RoleDropDown), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAssignedRoles(string userId)
        {
            return Ok(await _authenticationService.GetAssignedRoles(userId));
        }


        [HttpGet("test-organization/{organizationId}")]
        public async Task<IActionResult> TestOrganization(Guid organizationId)
        {
            try
            {
                var organizationExists = await _authenticationService.CheckOrganizationExists(organizationId);
                return Ok(new { OrganizationId = organizationId, Exists = organizationExists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddUser([FromBody] AddUSerDto addUSer)
        {
            try
            {
                Console.WriteLine($"DEBUG: AddUser controller called with UserName: {addUSer.UserName}, Email: {addUSer.Email}");
                
                if (ModelState.IsValid)
                {
                    // Server-side validation using InputValidationService
                    var (isValid, errors) = InputValidationService.ValidateUserProfile(addUSer);
                    if (!isValid)
                    {
                        return BadRequest(ErrorHandlingService.HandleValidationErrors(errors));
                    }

                    // Validate organization selection
                    if (addUSer.OrganizationId == Guid.Empty)
                    {
                        return BadRequest(ErrorHandlingService.CreateErrorResponse(
                            "RequiredField", "Please select an organization."));
                    }

                    // Validate role assignment
                    if (string.IsNullOrEmpty(addUSer.Roles))
                    {
                        return BadRequest(ErrorHandlingService.CreateErrorResponse(
                            "RequiredField", "Please assign a role to the user."));
                    }

                    // Validate role names (only Admin allowed)
                    var allowedRoles = new[] { "Admin" };
                    if (!string.IsNullOrEmpty(addUSer.Roles) && !allowedRoles.Contains(addUSer.Roles))
                    {
                        return BadRequest(ErrorHandlingService.CreateErrorResponse(
                            "InvalidFormat", "Only Admin role is allowed."));
                    }

                    var result = await _authenticationService.AddUser(addUSer);
                    return Ok(ErrorHandlingService.CreateSuccessResponse("User created successfully.", result.Data));
                }
                else
                {
                    var validationErrors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .ToList();
                    
                    return BadRequest(ErrorHandlingService.HandleValidationErrors(validationErrors));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ErrorHandlingService.HandleException(ex, _logger, "AddUser"));
            }
        }



        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AssingRole(UserRoleDto userRole)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _authenticationService.AssignRole(userRole));
            }
            else
            {
                return BadRequest();
            }
        }



        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RevokeRole(UserRoleDto userRole)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _authenticationService.RevokeRole(userRole));
            }
            else
            {
                return BadRequest();
            }
        }

        //[HttpPost]
        //[ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        //public async Task<IActionResult> ChangeStatusOfUser(string userId)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        return Ok(await _authenticationService.ChangeStatusOfUser(userId));
        //    }
        //    else
        //    {
        //        return BadRequest();
        //    }
        //}
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            if (ModelState.IsValid)
            {
                // Server-side validation using InputValidationService
                var (isValid, errors) = InputValidationService.ValidatePasswordReset(
                    model.CurrentPassword, model.NewPassword, model.ConfirmPassword);
                
                if (!isValid)
                {
                    return BadRequest(new ResponseMessage 
                    { 
                        Success = false, 
                        Message = "Password validation failed", 
                        Data = errors 
                    });
                }

                return Ok(await _authenticationService.ChangePassword(model));
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                return BadRequest(new ResponseMessage 
                { 
                    Success = false, 
                    Message = "Validation failed", 
                    Data = errors 
                });
            }
        }

        /// <summary>
        /// Update user profile information
        /// </summary>
        [HttpPut("update-profile")]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserProfileUpdateDto profile)
        {
            if (ModelState.IsValid)
            {
                // Server-side validation using InputValidationService
                var (isValid, errors) = InputValidationService.ValidateUserProfile(profile);
                if (!isValid)
                {
                    return BadRequest(new ResponseMessage 
                    { 
                        Success = false, 
                        Message = "Profile validation failed", 
                        Data = errors 
                    });
                }

                return Ok(await _authenticationService.UpdateUserProfile(profile));
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                return BadRequest(new ResponseMessage 
                { 
                    Success = false, 
                    Message = "Validation failed", 
                    Data = errors 
                });
            }
        }

        /// <summary>
        /// Reset user password (admin function)
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDto resetData)
        {
            if (ModelState.IsValid)
            {
                // Server-side validation using InputValidationService
                var (isValid, errors) = InputValidationService.ValidatePasswordReset(
                    "dummy", resetData.NewPassword, resetData.ConfirmPassword);
                
                if (!isValid)
                {
                    return BadRequest(new ResponseMessage 
                    { 
                        Success = false, 
                        Message = "Password validation failed", 
                        Data = errors 
                    });
                }

                return Ok(await _authenticationService.ResetPassword(resetData));
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                return BadRequest(new ResponseMessage 
                { 
                    Success = false, 
                    Message = "Validation failed", 
                    Data = errors 
                });
            }
        }

        /// <summary>
        /// Update user information (admin function)
        /// </summary>
        [HttpPut("update-user")]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto userUpdate)
        {
            if (ModelState.IsValid)
            {
                // Server-side validation using InputValidationService
                var (isValid, errors) = InputValidationService.ValidateUserProfile(userUpdate);
                if (!isValid)
                {
                    return BadRequest(new ResponseMessage 
                    { 
                        Success = false, 
                        Message = "User data validation failed", 
                        Data = errors 
                    });
                }

                // Validate organization selection
                if (userUpdate.OrganizationId == Guid.Empty)
                {
                    return BadRequest(new ResponseMessage 
                    { 
                        Success = false, 
                        Message = "Organization must be selected" 
                    });
                }

                // Validate role assignment
                if (userUpdate.Roles == null || userUpdate.Roles.Length == 0)
                {
                    return BadRequest(new ResponseMessage 
                    { 
                        Success = false, 
                        Message = "At least one role must be assigned" 
                    });
                }

                // Validate role names (only SuperAdmin and Admin allowed)
                var allowedRoles = new[] { "SuperAdmin", "Admin" };
                var invalidRoles = userUpdate.Roles.Where(role => !allowedRoles.Contains(role)).ToArray();
                if (invalidRoles.Any())
                {
                    return BadRequest(new ResponseMessage 
                    { 
                        Success = false, 
                        Message = $"Invalid roles: {string.Join(", ", invalidRoles)}. Only SuperAdmin and Admin are allowed." 
                    });
                }

                return Ok(await _authenticationService.UpdateUser(userUpdate));
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                return BadRequest(new ResponseMessage 
                { 
                    Success = false, 
                    Message = "Validation failed", 
                    Data = errors 
                });
            }
        }

        /// <summary>
        /// Get user profile information
        /// </summary>
        [HttpGet("user-profile/{userId}")]
        [ProducesResponseType(typeof(UserProfileDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserProfile(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new ResponseMessage 
                { 
                    Success = false, 
                    Message = "User ID is required" 
                });
            }

            return Ok(await _authenticationService.GetUserProfile(userId));
        }

        /// <summary>
        /// Get organizations for user selection
        /// </summary>
        [HttpGet("organizations")]
        [ProducesResponseType(typeof(SelectListDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetOrganizationsForUserSelection()
        {
            return Ok(await _authenticationService.GetOrganizationsForUserSelection());
        }

        /// <summary>
        /// Get available roles for user assignment
        /// </summary>
        [HttpGet("available-roles")]
        [ProducesResponseType(typeof(RoleDropDown), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAvailableRoles()
        {
            return Ok(await _authenticationService.GetAvailableRoles());
        }

        [HttpPost("setup-roles-and-users")]
        public async Task<IActionResult> SetupRolesAndUsers()
        {
            try
            {
                var result = await _authenticationService.SetupRolesAndUsers();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ErrorHandlingService.HandleException(ex, _logger, "SetupRolesAndUsers"));
            }
        }

    }
}
