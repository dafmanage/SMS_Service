using Implementation.DTOS.Authentication;

using Implementation.Helper;
using Implementation.Interfaces.Authentication;
using IntegratedInfrustructure.Data;
using IntegratedInfrustructure.Model.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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


        private readonly IDistributedCache _cache;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IDistributedCache cache,

            ApplicationDbContext dbContext,
            IHubContext<NotificationHub> hubContext,

              RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _signInManager = signInManager;
            _cache = cache;
            _hubContext = hubContext;

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
                IdentityOptions _options = new IdentityOptions();
                var str = String.Join(",", roleList);
                var organization = await _dbContext.Organizations.FirstOrDefaultAsync(x => x.Id == user.OrganizationId);

                if (organization != null)
                {
                    var newSessionId = Guid.NewGuid().ToString();
                    var TokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new System.Security.Claims.ClaimsIdentity(new Claim[]
                        {
                    new Claim("userId", user.Id.ToString()),
                    new Claim("organizationId", user.OrganizationId.ToString()),
                    new Claim("name", $"{organization.Name} {organization.NameLocal}"),
                    new Claim("sessionId", newSessionId),
                    new Claim("photo", organization?.ImagePath),
                    new Claim(_options.ClaimsIdentity.RoleClaimType, str),
                        }),
                        Expires = DateTime.UtcNow.AddHours(1),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1225290901686999272364748849994004994049404940")), SecurityAlgorithms.HmacSha256Signature)
                    };

                    var TokenHandler = new JwtSecurityTokenHandler();
                    var SecurityToken = TokenHandler.CreateToken(TokenDescriptor);
                    var token = TokenHandler.WriteToken(SecurityToken);


                    await _cache.SetStringAsync($"UserToken_{newSessionId}",token);
                    // Store the new session ID in the cache
                    await _cache.SetStringAsync($"UserSession_{user.Id}", newSessionId, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                    });

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
            var userList = await _userManager.Users.ToListAsync();
            var userLists = new List<UserListDto>();

            foreach (var user in userList)
            {

                var employee = _dbContext.Organizations.Find(user.OrganizationId);

                var userListt = new UserListDto()
                {
                    Id = user.Id,
                    OrganizationId = user.OrganizationId,
                    UserName = user.UserName,
                    Name = $"{employee.Name}",
                    Status = user.RowStatus.ToString(),
                    ImagePath = employee.ImagePath,
                    Email = employee.Email,


                };
                userListt.Roles = await GetAssignedRoles(user.Id);

                userLists.Add(userListt);

            }



            return userLists;
        }


        public async Task<ResponseMessage> AddUser(AddUSerDto addUSer)
        {
            var currentEmployee = _userManager.Users.Any(x => x.OrganizationId.Equals(addUSer.OrganizationId));
            if (currentEmployee)
                return new ResponseMessage { Success = false, Message = "Employee Already Exists" };

            var applicationUser = new ApplicationUser
            {
                OrganizationId = addUSer.OrganizationId,
                Email = addUSer.UserName + "@DAFtechSocial.com",
                UserName = addUSer.UserName,
                RowStatus = RowStatus.ACTIVE,
            };

            var response = await _userManager.CreateAsync(applicationUser, addUSer.Password);

            if (response.Succeeded)
            {
                var currentEmployee1 = _userManager.Users.Where(x => x.OrganizationId.Equals(addUSer.OrganizationId)).FirstOrDefault();



                //if ((!addUSer.Roles.IsNullOrEmpty()) && currentEmployee1 != null)
                //{
                //    var userRoles = new UserRoleDto();
                //    userRoles.UserId = currentEmployee1.Id;
                //    userRoles.RoleName = addUSer.Roles ;

                //    await _userManager.AddToRoleAsync(currentEmployee1, userRoles.RoleName);
                //}
                return new ResponseMessage { Success = true, Message = "Succesfully Added User", Data = applicationUser.UserName };
            }
            else
            {

                string errorMessage = string.Join(", ", response.Errors.Select(error => error.Code));
                return new ResponseMessage { Success = false, Message = errorMessage, Data = applicationUser.UserName };
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
    }
}
