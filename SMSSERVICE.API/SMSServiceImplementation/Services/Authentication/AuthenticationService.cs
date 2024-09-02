using Implementation.DTOS.Authentication;

using Implementation.Helper;
using Implementation.Interfaces.Authentication;
using IntegratedInfrustructure.Data;
using IntegratedInfrustructure.Model.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        private RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _dbContext;
      
        public AuthenticationService(UserManager<ApplicationUser> userManager,
            
            ApplicationDbContext dbContext,
         
              RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
     
        }


        public async Task<ResponseMessage> Login(LoginDto login)
        {
            var user = await _userManager.FindByNameAsync(login.UserName);

            if (user != null && await _userManager.CheckPasswordAsync(user, login.Password))
            {
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

                    var TokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new System.Security.Claims.ClaimsIdentity(new Claim[]
                        {
                        new Claim("userId", user.Id.ToString()),
                        new Claim("organizationId", user.OrganizationId.ToString()),
                        new Claim("name", $"{organization.Name} {organization.NameLocal}"),
                        new Claim("photo",organization?.ImagePath),
                        new Claim(_options.ClaimsIdentity.RoleClaimType, str),

                        }),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1225290901686999272364748849994004994049404940")), SecurityAlgorithms.HmacSha256Signature)
                    };

                    var TokenHandler = new JwtSecurityTokenHandler();
                    var SecurityToken = TokenHandler.CreateToken(TokenDescriptor);
                    var token = TokenHandler.WriteToken(SecurityToken);
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
                    Message = "could not find Employee"
                };
            }
            else
                return new ResponseMessage()
                {
                    Success = false,
                    Message = "Invalid User Name or Password"
                };

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
            var currentUser = await _userManager.Users.FirstOrDefaultAsync(x=>x.Id==userRole.UserId);

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
                foreach (var role in userRole.RoleName) { 
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
