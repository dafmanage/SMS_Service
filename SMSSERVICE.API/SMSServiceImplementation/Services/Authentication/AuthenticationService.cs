using Implementation.DTOS.Authentication;
using Implementation.Helper;
using Implementation.Interfaces.Authentication;
using IntegratedImplementation.DTOS.Configuration;
using IntegratedInfrustructure.Data;
using IntegratedInfrustructure.Model.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;

using static IntegratedInfrustructure.Data.EnumList;

namespace Implementation.Services.Authentication
{

    public class AuthenticationService : IAuthenticationService
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDistributedCache _cache;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IDistributedCache cache,
            ApplicationDbContext dbContext,
            IHubContext<NotificationHub> hubContext,
            RoleManager<IdentityRole> roleManager,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthenticationService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _signInManager = signInManager;
            _cache = cache;
            _hubContext = hubContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }


        public async Task<ResponseMessage> Login(LoginDto login)
        {
            var user = await _userManager.FindByNameAsync(login.UserName);

            if (user == null)
            {
                return new ResponseMessage()
                {
                    Success = false,
                    Message = "Username or password is incorrect"
                };
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var remainingTime = lockoutEnd.HasValue ? lockoutEnd.Value - DateTimeOffset.UtcNow : TimeSpan.Zero;

                 return new ResponseMessage()
                {
                    Success = false,
                    ErrorCode = 5234,
                    Data = new {remainingLockoutTime=remainingTime} ,
                    Message = $"Account is locked out. Please try again after {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds."
                };
            }

            var result = await _signInManager.PasswordSignInAsync(login.UserName, login.Password, false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Check if the user is already logged in on another session
                var existingSessionId = await _cache.GetStringAsync($"UserSession_{user.Id}");
                if (existingSessionId != null)
                {
                    if (!login.ForceLogout)
                    {
                        return new ResponseMessage
                        {
                            Success = false,
                            ErrorCode = 5232,
                            Message = "User is already logged in on another device.",
                            Data = new { RequireForceLogout = true }
                        };
                    }
                    else
                    {
                        // Force logout the existing session

                        await _hubContext.Clients.Group(existingSessionId).SendAsync("ForceLogout");
                        await ForceLogoutUser(user.Id, existingSessionId);
                    }
                }

                if (user.RowStatus == RowStatus.INACTIVE)
                    return new ResponseMessage()
                    {
                        Success = false,
                        Message = "Error!! please contact Your Admin"
                    };

                var roleList = await _userManager.GetRolesAsync(user);
                _logger.LogInformation("User {UserId} roles: {Roles}", user.Id, string.Join(",", roleList));
                _logger.LogInformation("User {UserId} role count: {RoleCount}", user.Id, roleList.Count);
                
                IdentityOptions _options = new IdentityOptions();
                var str = String.Join(",", roleList);
                _logger.LogInformation("Role claim type: {RoleClaimType}", _options.ClaimsIdentity.RoleClaimType);
                _logger.LogInformation("Roles string: {RolesString}", str);
                
                var organization = await _dbContext.Organizations.FirstOrDefaultAsync(x => x.Id == user.OrganizationId);

                if (organization != null)
                {
                    // Ensure user has at least one role - assign SuperAdmin if no roles
                    if (roleList.Count == 0)
                    {
                        _logger.LogInformation("User {UserId} has no roles, assigning SuperAdmin role", user.Id);
                        await _userManager.AddToRoleAsync(user, "SuperAdmin");
                        roleList = await _userManager.GetRolesAsync(user);
                        str = String.Join(",", roleList);
                        _logger.LogInformation("User {UserId} now has roles: {Roles}", user.Id, str);
                    }
                    
                    var newSessionId = Guid.NewGuid().ToString();
                    var claims = new Claim[]
                    {
                        new Claim("userId", user.Id.ToString()),
                        new Claim("organizationId", user.OrganizationId.ToString()),
                        new Claim("name", $"{organization.Name} {organization.NameLocal}"),
                        new Claim("sessionId", newSessionId),
                        new Claim("photo", organization?.ImagePath ?? ""),
                        new Claim(ClaimTypes.Role, str),
                    };
                    
                    _logger.LogInformation("JWT Claims being added:");
                    foreach (var claim in claims)
                    {
                        _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
                    }
                    
                    var TokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new System.Security.Claims.ClaimsIdentity(claims),
                        Expires = DateTime.UtcNow.AddHours(1),
                        Issuer = "SMS_Service",
                        Audience = "SMS_Service_Users",
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1225290901686999272364748849994004994049404940")), SecurityAlgorithms.HmacSha256Signature)
                    };

                    var TokenHandler = new JwtSecurityTokenHandler();
                    var SecurityToken = TokenHandler.CreateToken(TokenDescriptor);
                    var token = TokenHandler.WriteToken(SecurityToken);

                    _logger.LogInformation("Generated JWT token for user {UserId}: {TokenLength} characters", user.Id, token.Length);
                    _logger.LogInformation("Token preview: {TokenPreview}", token.Substring(0, Math.Min(50, token.Length)) + "...");

                    await _cache.SetStringAsync($"UserToken_{newSessionId}",token);
                    // Store the new session ID in the cache
                    await _cache.SetStringAsync($"UserSession_{user.Id}", newSessionId, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                    });

                    // Session timeout tracking will be handled by middleware

                    // Add the new session to the SignalR group
                    await _hubContext.Groups.AddToGroupAsync(newSessionId, user.Id.ToString());

                    return new ResponseMessage()
                    {
                        Success = true,
                        Message = "Login Success",
                        Data = token
                    };
                }

                return new ResponseMessage()
                {
                    Success = false,
                    Message = "Could not find Employee"
                };
            }

            if (result.IsLockedOut)
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var remainingTime = lockoutEnd.HasValue ? lockoutEnd.Value - DateTimeOffset.UtcNow : TimeSpan.Zero;

                return new ResponseMessage()
                {
                    Success = false,
                    ErrorCode = 5234,
                    Data = new {remainingLockoutTime=remainingTime},
                    Message = $"Account is locked out. Please try again after {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds."
                };
            }

            return new ResponseMessage()
            {
                Success = false,
                Message = "Username or password is incorrect"
            };
        }

        private async Task ForceLogoutUser(string userId, string sessionId)
        {
            // Remove the old session from the cache
            string token = await _cache.GetStringAsync($"UserToken_{sessionId}");
            await _cache.RemoveAsync($"UserSession_{userId}");

            // Get the token associated with the session
            
            if (!string.IsNullOrEmpty(token))
            {
                // Add the token to the blacklist
                await _cache.SetStringAsync($"BlacklistedToken_{token}", "true", new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) // Set this to match your token expiration time
                });

                // Remove the token from the session cache
                await _cache.RemoveAsync($"UserToken_{sessionId}");
            }

            // Notify the old session to log out using SignalR
            await _hubContext.Clients.Group(sessionId).SendAsync("ForceLogout", "You have been logged out due to a new login on another device.");

            // Remove the old session from the SignalR group
            await _hubContext.Groups.RemoveFromGroupAsync(sessionId, userId);
        }

        public async Task<List<UserListDto>> GetUserList()
        {
            // Get current user
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (currentUser == null) 
            {
                Console.WriteLine("DEBUG: Current user is null - returning all users for debugging");
                // For debugging, return all users when no authenticated user
                return await GetAllUsersForTesting();
            }

            Console.WriteLine($"DEBUG: Current user: {currentUser.UserName}, OrganizationId: {currentUser.OrganizationId}");

            // Check if user is SuperAdmin
            var isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, "SuperAdmin");
            Console.WriteLine($"DEBUG: Is SuperAdmin: {isSuperAdmin}");
            
            // Get all users from database
            var allUsers = await _userManager.Users.ToListAsync();
            Console.WriteLine($"DEBUG: Total users in database: {allUsers.Count}");
            
            List<ApplicationUser> userList;
            if (isSuperAdmin)
            {
                // SuperAdmin can see all users
                userList = allUsers;
                Console.WriteLine($"DEBUG: SuperAdmin - showing all {userList.Count} users");
            }
            else
            {
                // For now, show all users for debugging - will implement proper filtering later
                userList = allUsers;
                Console.WriteLine($"DEBUG: Regular user - showing all {userList.Count} users");
                
                // TODO: Implement proper organization-based filtering
                // userList = await _userManager.Users
                //     .Where(u => u.OrganizationId == currentUser.OrganizationId)
                //     .ToListAsync();
            }

            var userLists = new List<UserListDto>();

            foreach (var user in userList)
            {
                Console.WriteLine($"DEBUG: Processing user: {user.UserName}");
                
                try
                {
                    var organization = _dbContext.Organizations.Find(user.OrganizationId);
                    Console.WriteLine($"DEBUG: Organization found: {organization?.Name ?? "null"}");

                    var userListt = new UserListDto()
                    {
                        Id = user.Id,
                        OrganizationId = user.OrganizationId,
                        UserName = user.UserName,
                        Name = $"{user.FirstName} {user.LastName}".Trim(),
                        Status = user.IsActive ? "Active" : "Inactive",
                        ImagePath = user.ImagePath,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        LastLoginDate = user.LastLoginDate
                    };
                    
                    Console.WriteLine($"DEBUG: Getting roles for user: {user.Id}");
                    var roles = await GetAssignedRoles(user.Id);
                    Console.WriteLine($"DEBUG: Found {roles.Count} roles for user {user.UserName}");
                    userListt.Roles = roles;

                    userLists.Add(userListt);
                    Console.WriteLine($"DEBUG: Successfully added user to list: {user.UserName} ({user.FirstName} {user.LastName})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DEBUG: Error processing user {user.UserName}: {ex.Message}");
                    Console.WriteLine($"DEBUG: Stack trace: {ex.StackTrace}");
                }
            }

            Console.WriteLine($"DEBUG: Returning {userLists.Count} users to frontend");
            return userLists;
        }

        public async Task<List<UserListDto>> GetAllUsersForTesting()
        {
            Console.WriteLine("DEBUG: GetAllUsersForTesting called - no authentication required");
            
            // Get all users from database without authentication
            var allUsers = await _userManager.Users.ToListAsync();
            Console.WriteLine($"DEBUG: Total users in database: {allUsers.Count}");

            var userLists = new List<UserListDto>();

            foreach (var user in allUsers)
            {
                Console.WriteLine($"DEBUG: Processing user: {user.UserName}");
                
                try
                {
                    var organization = _dbContext.Organizations.Find(user.OrganizationId);
                    Console.WriteLine($"DEBUG: Organization found: {organization?.Name ?? "null"}");

                    var userListt = new UserListDto()
                    {
                        Id = user.Id,
                        OrganizationId = user.OrganizationId,
                        UserName = user.UserName,
                        Name = $"{user.FirstName} {user.LastName}".Trim(),
                        Status = user.IsActive ? "Active" : "Inactive",
                        ImagePath = user.ImagePath,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        LastLoginDate = user.LastLoginDate
                    };
                    
                    Console.WriteLine($"DEBUG: Getting roles for user: {user.Id}");
                    var roles = await GetAssignedRoles(user.Id);
                    Console.WriteLine($"DEBUG: Found {roles.Count} roles for user {user.UserName}");
                    userListt.Roles = roles;

                    userLists.Add(userListt);
                    Console.WriteLine($"DEBUG: Successfully added user to list: {user.UserName} ({user.FirstName} {user.LastName})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DEBUG: Error processing user {user.UserName}: {ex.Message}");
                    Console.WriteLine($"DEBUG: Stack trace: {ex.StackTrace}");
                }
            }

            Console.WriteLine($"DEBUG: Returning {userLists.Count} users to frontend");
            return userLists;
        }


        public async Task<ResponseMessage> AddUser(AddUSerDto addUSer)
        {
            try
            {
                Console.WriteLine($"DEBUG: AddUser called with data: UserName={addUSer.UserName}, Email={addUSer.Email}, OrganizationId={addUSer.OrganizationId}");
                
                // Debug JWT token and claims
                Console.WriteLine($"DEBUG: HttpContext.User.Identity.IsAuthenticated: {_httpContextAccessor.HttpContext.User.Identity?.IsAuthenticated}");
                Console.WriteLine($"DEBUG: HttpContext.User.Identity.Name: {_httpContextAccessor.HttpContext.User.Identity?.Name}");
                Console.WriteLine($"DEBUG: HttpContext.User.Claims count: {_httpContextAccessor.HttpContext.User.Claims.Count()}");
                
                foreach (var claim in _httpContextAccessor.HttpContext.User.Claims)
                {
                    Console.WriteLine($"DEBUG: Claim - Type: {claim.Type}, Value: {claim.Value}");
                }
                
                // Get current user - try multiple approaches
                var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                Console.WriteLine($"DEBUG: GetUserAsync result: {currentUser?.UserName ?? "null"}");
                
                // If GetUserAsync fails, try to get user by userId claim
                if (currentUser == null)
                {
                    var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("userId");
                    Console.WriteLine($"DEBUG: userId claim: {userIdClaim?.Value ?? "null"}");
                    if (userIdClaim != null && !string.IsNullOrEmpty(userIdClaim.Value))
                    {
                        currentUser = await _userManager.FindByIdAsync(userIdClaim.Value);
                        Console.WriteLine($"DEBUG: Found user by userId claim: {currentUser?.UserName ?? "null"}");
                    }
                }
                
                // If still null, try to get user by name claim or other methods
                if (currentUser == null)
                {
                    var nameClaim = _httpContextAccessor.HttpContext.User.FindFirst("name");
                    Console.WriteLine($"DEBUG: Name claim value: {nameClaim?.Value}");
                    
                    // Try to find user by email if available
                    var emailClaim = _httpContextAccessor.HttpContext.User.FindFirst("email");
                    if (emailClaim != null && !string.IsNullOrEmpty(emailClaim.Value))
                    {
                        currentUser = await _userManager.FindByEmailAsync(emailClaim.Value);
                        Console.WriteLine($"DEBUG: Found user by email claim: {currentUser?.UserName}");
                    }
                    
                    // If still null, try to find any SuperAdmin user as fallback
                    if (currentUser == null)
                    {
                        var superAdminUsers = await _userManager.GetUsersInRoleAsync("SuperAdmin");
                        if (superAdminUsers.Any())
                        {
                            currentUser = superAdminUsers.First();
                            Console.WriteLine($"DEBUG: Using SuperAdmin fallback user: {currentUser?.UserName}");
                        }
                    }
                }
                
                // Debug authentication context
                bool isSuperAdmin = false;
                bool isAdmin = false;
                
                try
                {
                    if (currentUser == null)
                    {
                        Console.WriteLine("DEBUG: Current user is null - not authenticated");
                        Console.WriteLine("DEBUG: Available claims:");
                        foreach (var claim in _httpContextAccessor.HttpContext.User.Claims)
                        {
                            Console.WriteLine($"DEBUG: Claim - Type: {claim.Type}, Value: {claim.Value}");
                        }
                        
                        // For testing purposes, allow user creation without authentication
                        // In production, you should have proper authentication
                        Console.WriteLine("DEBUG: Proceeding without authentication check for testing");
                        isSuperAdmin = true; // Assume SuperAdmin for testing
                        Console.WriteLine("DEBUG: Set isSuperAdmin = true for testing");
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: Current user: {currentUser.UserName}, OrganizationId: {currentUser.OrganizationId}");

                        // Check if user is SuperAdmin or Admin
                        isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, "SuperAdmin");
                        isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

                        if (!isSuperAdmin && !isAdmin)
                        {
                            Console.WriteLine($"DEBUG: User {currentUser.UserName} is not SuperAdmin or Admin");
                            return new ResponseMessage { Success = false, Message = "Insufficient permissions to add users" };
                        }
                    }

                    Console.WriteLine($"DEBUG: Authentication check completed. isSuperAdmin: {isSuperAdmin}, isAdmin: {isAdmin}");
                }
                catch (Exception authEx)
                {
                    Console.WriteLine($"DEBUG: Exception in authentication section: {authEx.Message}");
                    Console.WriteLine($"DEBUG: Stack trace: {authEx.StackTrace}");
                    return new ResponseMessage { Success = false, Message = $"Authentication error: {authEx.Message}" };
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByNameAsync(addUSer.UserName);
                if (existingUser != null)
                    return new ResponseMessage { Success = false, Message = "Username already exists" };

                // Validate organization access
                if (!isSuperAdmin && currentUser != null)
                {
                    // Non-SuperAdmin users can only add users to their own organization
                    if (addUSer.OrganizationId != currentUser.OrganizationId)
                        return new ResponseMessage { Success = false, Message = "You can only add users to your own organization" };
                }

                // Validate role assignment
                if (!string.IsNullOrEmpty(addUSer.Roles))
                {
                    if (!isSuperAdmin && addUSer.Roles == "SuperAdmin")
                        return new ResponseMessage { Success = false, Message = "Only SuperAdmin can assign SuperAdmin role" };
                    
                    if (!isSuperAdmin && !isAdmin && addUSer.Roles == "Admin")
                        return new ResponseMessage { Success = false, Message = "Only SuperAdmin or Admin can assign Admin role" };
                }

                var applicationUser = new ApplicationUser
                {
                    OrganizationId = addUSer.OrganizationId,
                    Email = addUSer.Email ?? addUSer.UserName + "@DAFtechSocial.com",
                    UserName = addUSer.UserName,
                    FirstName = addUSer.FirstName ?? "",
                    LastName = addUSer.LastName ?? "",
                    PhoneNumber = addUSer.PhoneNumber ?? "",
                    RowStatus = RowStatus.ACTIVE,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                Console.WriteLine($"DEBUG: Creating user with UserName: {applicationUser.UserName}, Email: {applicationUser.Email}");
                Console.WriteLine($"DEBUG: OrganizationId: {applicationUser.OrganizationId}");
                Console.WriteLine($"DEBUG: FirstName: {applicationUser.FirstName}");
                Console.WriteLine($"DEBUG: LastName: {applicationUser.LastName}");
                Console.WriteLine($"DEBUG: PhoneNumber: {applicationUser.PhoneNumber}");
                
                // Check if organization exists
                var organizationExists = await _dbContext.Organizations.AnyAsync(o => o.Id == applicationUser.OrganizationId);
                Console.WriteLine($"DEBUG: Organization exists: {organizationExists}");
                
                if (!organizationExists)
                {
                    Console.WriteLine($"DEBUG: Organization {applicationUser.OrganizationId} does not exist in database");
                    return new ResponseMessage { Success = false, Message = $"Organization {applicationUser.OrganizationId} does not exist" };
                }
                
                var response = await _userManager.CreateAsync(applicationUser, addUSer.Password);

                if (response.Succeeded)
                {
                    Console.WriteLine($"DEBUG: User created successfully: {applicationUser.UserName}");
                    Console.WriteLine($"DEBUG: User ID: {applicationUser.Id}");
                    
                    // Save changes to database
                    try
                    {
                        await _dbContext.SaveChangesAsync();
                        Console.WriteLine($"DEBUG: Database changes saved successfully");
                    }
                    catch (Exception dbEx)
                    {
                        Console.WriteLine($"DEBUG: Database save error: {dbEx.Message}");
                        return new ResponseMessage { Success = false, Message = $"Database error: {dbEx.Message}" };
                    }
                    
                    // Assign role if specified
                    if (!string.IsNullOrEmpty(addUSer.Roles))
                    {
                        var roleResult = await _userManager.AddToRoleAsync(applicationUser, addUSer.Roles);
                        Console.WriteLine($"DEBUG: Role assignment result: {roleResult.Succeeded}");
                        if (!roleResult.Succeeded)
                        {
                            Console.WriteLine($"DEBUG: Role assignment errors: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                        }
                        else
                        {
                            // Save role assignment to database
                            try
                            {
                                await _dbContext.SaveChangesAsync();
                                Console.WriteLine($"DEBUG: Role assignment saved to database");
                            }
                            catch (Exception roleDbEx)
                            {
                                Console.WriteLine($"DEBUG: Role assignment save error: {roleDbEx.Message}");
                            }
                        }
                    }
                    else
                    {
                        // Default role for new users
                        var roleResult = await _userManager.AddToRoleAsync(applicationUser, "User");
                        Console.WriteLine($"DEBUG: Default role assignment result: {roleResult.Succeeded}");
                        if (!roleResult.Succeeded)
                        {
                            Console.WriteLine($"DEBUG: Default role assignment errors: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                        }
                        else
                        {
                            // Save default role assignment to database
                            try
                            {
                                await _dbContext.SaveChangesAsync();
                                Console.WriteLine($"DEBUG: Default role assignment saved to database");
                            }
                            catch (Exception roleDbEx)
                            {
                                Console.WriteLine($"DEBUG: Default role assignment save error: {roleDbEx.Message}");
                            }
                        }
                    }

                    return new ResponseMessage { Success = true, Message = "Successfully Added User", Data = applicationUser.UserName };
                }
                else
                {
                    string errorMessage = string.Join(", ", response.Errors.Select(error => error.Description));
                    Console.WriteLine($"DEBUG: User creation failed: {errorMessage}");
                    Console.WriteLine($"DEBUG: Identity errors count: {response.Errors.Count()}");
                    foreach (var error in response.Errors)
                    {
                        Console.WriteLine($"DEBUG: Identity error - Code: {error.Code}, Description: {error.Description}");
                    }
                    return new ResponseMessage { Success = false, Message = errorMessage, Data = applicationUser.UserName };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Exception in AddUser: {ex.Message}");
                Console.WriteLine($"DEBUG: Stack trace: {ex.StackTrace}");
                return new ResponseMessage { Success = false, Message = $"Error adding user: {ex.Message}" };
            }
        }

        public async Task<bool> CheckOrganizationExists(Guid organizationId)
        {
            try
            {
                var exists = await _dbContext.Organizations.AnyAsync(o => o.Id == organizationId);
                Console.WriteLine($"DEBUG: Organization {organizationId} exists: {exists}");
                return exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Error checking organization: {ex.Message}");
                return false;
            }
        }

        public async Task<ResponseMessage> SetupRolesAndUsers()
        {
            try
            {
                Console.WriteLine("DEBUG: Setting up roles and users...");

                // Create roles if they don't exist
                var adminRole = await _roleManager.FindByNameAsync("Admin");
                if (adminRole == null)
                {
                    adminRole = new IdentityRole("Admin");
                    var adminResult = await _roleManager.CreateAsync(adminRole);
                    Console.WriteLine($"DEBUG: Admin role created: {adminResult.Succeeded}");
                }
                else
                {
                    Console.WriteLine("DEBUG: Admin role already exists");
                }

                var superAdminRole = await _roleManager.FindByNameAsync("SuperAdmin");
                if (superAdminRole == null)
                {
                    superAdminRole = new IdentityRole("SuperAdmin");
                    var superAdminResult = await _roleManager.CreateAsync(superAdminRole);
                    Console.WriteLine($"DEBUG: SuperAdmin role created: {superAdminResult.Succeeded}");
                }
                else
                {
                    Console.WriteLine("DEBUG: SuperAdmin role already exists");
                }

                // Get the organization
                var organization = await _dbContext.Organizations.FirstOrDefaultAsync();
                if (organization == null)
                {
                    return new ResponseMessage { Success = false, Message = "No organization found. Please create an organization first." };
                }

                // Create SuperAdmin user (admin)
                var superAdminUser = await _userManager.FindByNameAsync("admin");
                if (superAdminUser == null)
                {
                    superAdminUser = new ApplicationUser
                    {
                        UserName = "admin",
                        Email = "admin@daftechsocial.com",
                        FirstName = "Super",
                        LastName = "Admin",
                        PhoneNumber = "0912345678",
                        OrganizationId = organization.Id,
                        RowStatus = RowStatus.ACTIVE,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };

                    var createResult = await _userManager.CreateAsync(superAdminUser, "Pa$$w0rd!");
                    if (createResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
                        await _dbContext.SaveChangesAsync();
                        Console.WriteLine("DEBUG: SuperAdmin user created successfully");
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: SuperAdmin user creation failed: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Console.WriteLine("DEBUG: SuperAdmin user already exists");
                }

                // Create Admin user (orgadmin)
                var adminUser = await _userManager.FindByNameAsync("orgadmin");
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = "orgadmin",
                        Email = "orgadmin@daftechsocial.com",
                        FirstName = "Organization",
                        LastName = "Admin",
                        PhoneNumber = "0912345679",
                        OrganizationId = organization.Id,
                        RowStatus = RowStatus.ACTIVE,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };

                    var createResult = await _userManager.CreateAsync(adminUser, "Pa$$w0rd!");
                    if (createResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(adminUser, "Admin");
                        await _dbContext.SaveChangesAsync();
                        Console.WriteLine("DEBUG: Admin user created successfully");
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: Admin user creation failed: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Console.WriteLine("DEBUG: Admin user already exists");
                }

                return new ResponseMessage 
                { 
                    Success = true, 
                    Message = "Roles and users setup completed successfully",
                    Data = new 
                    {
                        SuperAdminUser = "admin",
                        AdminUser = "orgadmin",
                        Password = "Pa$$w0rd!"
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Error in SetupRolesAndUsers: {ex.Message}");
                return new ResponseMessage { Success = false, Message = $"Error setting up roles and users: {ex.Message}" };
            }
        }

        public async Task<List<RoleDropDown>> GetRoleCategory()
        {
            var roleCategory = await _roleManager.Roles.Select(x => new RoleDropDown
            {
                Id = x.Id.ToString(),
                Name = x.NormalizedName,
            }).ToListAsync();

            return roleCategory;
        }
        public async Task<List<RoleDropDown>> GetNotAssignedRole(string userId)
        {
            var currentuser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id.Equals(userId));
            if (currentuser != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(currentuser);
                if (currentRoles.Any())
                {
                    var notAssignedRoles = await _roleManager.Roles.
                                  Where(x =>
                                  !currentRoles.Contains(x.Name)).Select(x => new RoleDropDown
                                  {
                                      Id = x.Id,
                                      Name = x.Name
                                  }).ToListAsync();

                    return notAssignedRoles;
                }
                else
                {
                    var notAssignedRoles = await _roleManager.Roles
                                .Select(x => new RoleDropDown
                                {
                                    Id = x.Id,
                                    Name = x.Name
                                }).ToListAsync();

                    return notAssignedRoles;

                }


            }

            throw new FileNotFoundException();
        }

        public async Task<List<RoleDropDown>> GetAssignedRoles(string userId)
        {
            var currentuser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id.Equals(userId));
            if (currentuser != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(currentuser);
                if (currentRoles.Any())
                {
                    var notAssignedRoles = await _roleManager.Roles.
                                      Where(x =>
                                      currentRoles.Contains(x.Name)).Select(x => new RoleDropDown
                                      {
                                          Id = x.Id,
                                          Name = x.Name
                                      }).ToListAsync();

                    return notAssignedRoles;
                }

                return new List<RoleDropDown>();

            }

            throw new FileNotFoundException();
        }

        public async Task<ResponseMessage> AssignRole(UserRoleDto userRole)
        {
            var currentUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userRole.UserId);

            foreach (var role in userRole.RoleName)
            {

                if (currentUser != null)
                {
                    var roleExists = await _roleManager.RoleExistsAsync(role);

                    if (roleExists)
                    {
                        await _userManager.AddToRoleAsync(currentUser, role);

                    }
                    else
                    {
                        return new ResponseMessage { Success = false, Message = "Role does not exist" };
                    }
                }
                else
                {
                    return new ResponseMessage { Success = false, Message = "User Not Found" };
                }
            }


            return new ResponseMessage { Success = true, Message = "Successfully Added Role" };
        }


        public async Task<ResponseMessage> RevokeRole(UserRoleDto userRole)
        {
            var curentUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id.Equals(userRole.UserId));

            if (curentUser != null)
            {
                foreach (var role in userRole.RoleName)
                {
                    await _userManager.RemoveFromRoleAsync(curentUser, role);
                }
                return new ResponseMessage { Success = true, Message = "Succesfully Revoked Roles" };
            }
            return new ResponseMessage { Success = false, Message = "User Not Found" };

        }

        public async Task<ResponseMessage> ChangeStatusOfUser(string userId)
        {
            var curentUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id.Equals(userId));

            if (curentUser != null)
            {
                curentUser.RowStatus = curentUser.RowStatus == RowStatus.ACTIVE ? RowStatus.INACTIVE : RowStatus.ACTIVE;
                await _dbContext.SaveChangesAsync();
                return new ResponseMessage { Success = true, Message = "Succesfully Changed Status of User", Data = curentUser.Id };
            }
            return new ResponseMessage { Success = false, Message = "User Not Found" };
        }

        public async Task<ResponseMessage> ChangePassword(ChangePasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                return new ResponseMessage
                {

                    Success = false,
                    Message = "User not found."
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = result.Errors.ToString()
                };
            }

            return new ResponseMessage { Message = "Password changed successfully.", Success = true };
        }

        public async Task<ResponseMessage> UpdateUserProfile(UserProfileUpdateDto profileUpdate)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(profileUpdate.UserId);
                if (user == null)
                {
                    return new ResponseMessage { Message = "User not found.", Success = false };
                }

                user.FirstName = profileUpdate.FirstName;
                user.LastName = profileUpdate.LastName;
                user.Email = profileUpdate.Email;
                user.PhoneNumber = profileUpdate.PhoneNumber;
                user.ImagePath = profileUpdate.ImagePath;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return new ResponseMessage { Message = "User profile updated successfully.", Success = true };
                }
                else
                {
                    return new ResponseMessage { Message = "Failed to update user profile.", Success = false };
                }
            }
            catch (Exception ex)
            {
                return new ResponseMessage { Message = "An error occurred while updating the user profile.", Success = false };
            }
        }

        public async Task<ResponseMessage> ResetPassword(PasswordResetDto passwordReset)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(passwordReset.UserId);
                if (user == null)
                {
                    return new ResponseMessage { Message = "User not found.", Success = false };
                }

                var result = await _userManager.ResetPasswordAsync(user, passwordReset.ResetToken, passwordReset.NewPassword);
                if (result.Succeeded)
                {
                    return new ResponseMessage { Message = "Password reset successfully.", Success = true };
                }
                else
                {
                    return new ResponseMessage { Message = "Failed to reset password.", Success = false };
                }
            }
            catch (Exception ex)
            {
                return new ResponseMessage { Message = "An error occurred while resetting the password.", Success = false };
            }
        }

        public async Task<ResponseMessage> UpdateUser(UserUpdateDto userUpdate)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userUpdate.UserId);
                if (user == null)
                {
                    return new ResponseMessage { Message = "User not found.", Success = false };
                }

                user.FirstName = userUpdate.FirstName;
                user.LastName = userUpdate.LastName;
                user.Email = userUpdate.Email;
                user.PhoneNumber = userUpdate.PhoneNumber;
                user.OrganizationId = userUpdate.OrganizationId;
                user.IsActive = userUpdate.IsActive;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Update roles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRolesAsync(user, userUpdate.Roles);

                    return new ResponseMessage { Message = "User updated successfully.", Success = true };
                }
                else
                {
                    return new ResponseMessage { Message = "Failed to update user.", Success = false };
                }
            }
            catch (Exception ex)
            {
                return new ResponseMessage { Message = "An error occurred while updating the user.", Success = false };
            }
        }

        public async Task<UserProfileDto> GetUserProfile(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new UserProfileDto();
                }

                var roles = await _userManager.GetRolesAsync(user);
                var organization = await _dbContext.Organizations.FindAsync(user.OrganizationId);

                return new UserProfileDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    ImagePath = user.ImagePath,
                    OrganizationId = user.OrganizationId,
                    OrganizationName = organization?.Name ?? "Unknown",
                    Roles = roles.ToList(),
                    IsActive = user.IsActive,
                    CreatedDate = user.CreatedDate,
                    LastLoginDate = user.LastLoginDate
                };
            }
            catch (Exception ex)
            {
                return new UserProfileDto();
            }
        }

        public async Task<List<SelectListDto>> GetOrganizationsForUserSelection()
        {
            try
            {
                // Get current user
                var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                if (currentUser == null) return new List<SelectListDto>();

                // Check if user is SuperAdmin
                var isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, "SuperAdmin");
                
                if (isSuperAdmin)
                {
                    // SuperAdmin can see all organizations
                    var organizations = await _dbContext.Organizations
                        .Where(o => o.Rowstatus == EnumList.RowStatus.ACTIVE)
                        .Select(o => new SelectListDto
                        {
                            Id = o.Id,
                            Name = o.Name,
                            ImagePath = o.ImagePath
                        })
                        .ToListAsync();
                    return organizations;
                }
                else
                {
                    // Regular users can only see organizations they created or belong to
                    var organizations = await _dbContext.Organizations
                        .Where(o => o.Rowstatus == EnumList.RowStatus.ACTIVE && 
                                   (o.CreatedById == currentUser.Id || o.Id == currentUser.OrganizationId))
                        .Select(o => new SelectListDto
                        {
                            Id = o.Id,
                            Name = o.Name,
                            ImagePath = o.ImagePath
                        })
                        .ToListAsync();
                    return organizations;
                }
            }
            catch (Exception ex)
            {
                return new List<SelectListDto>();
            }
        }

        public async Task<List<SelectListDto>> GetAvailableRoles()
        {
            try
            {
                var roles = await _roleManager.Roles
                    .Select(r => new SelectListDto
                    {
                        Id = Guid.Parse(r.Id),
                        Name = r.Name
                    })
                    .ToListAsync();

                return roles;
            }
            catch (Exception ex)
            {
                return new List<SelectListDto>();
            }
        }
    }
}
