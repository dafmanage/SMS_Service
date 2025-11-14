using Implementation.DTOS.Authentication;
using Implementation.Helper;
using IntegratedImplementation.DTOS.Configuration;

namespace Implementation.Interfaces.Authentication
{
    public interface IAuthenticationService
    {
        Task<ResponseMessage> Login(LoginDto login);
        Task<List<UserListDto>> GetUserList();
        Task<List<UserListDto>> GetAllUsersForTesting();
        Task<List<RoleDropDown>> GetRoleCategory();
        Task<List<RoleDropDown>> GetNotAssignedRole(string userId);
        Task<List<RoleDropDown>> GetAssignedRoles(string userId);
        Task<ResponseMessage> AssignRole(UserRoleDto userRole);
        Task<ResponseMessage> RevokeRole(UserRoleDto userRole);
        //Task<ResponseMessage> ChangeStatusOfUser(string userId);
        Task<ResponseMessage> AddUser(AddUSerDto addUSer);
        Task<ResponseMessage> ChangePassword(ChangePasswordDto model);
        Task<ResponseMessage> UpdateUserProfile(UserProfileUpdateDto profileUpdate);
        Task<ResponseMessage> ResetPassword(PasswordResetDto passwordReset);
        Task<ResponseMessage> UpdateUser(UserUpdateDto userUpdate);
        Task<UserProfileDto> GetUserProfile(string userId);
        Task<List<SelectListDto>> GetOrganizationsForUserSelection();
        Task<List<SelectListDto>> GetAvailableRoles();
        Task<bool> CheckOrganizationExists(Guid organizationId);
        Task<ResponseMessage> SetupRolesAndUsers();
    }
}
