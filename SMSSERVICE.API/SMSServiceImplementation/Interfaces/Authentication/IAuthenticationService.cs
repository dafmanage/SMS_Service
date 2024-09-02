using Implementation.DTOS.Authentication;
using Implementation.Helper;

namespace Implementation.Interfaces.Authentication
{
    public interface IAuthenticationService
    {
        Task<ResponseMessage> Login(LoginDto login);
        Task<List<UserListDto>> GetUserList();
        Task<List<RoleDropDown>> GetRoleCategory();
        Task<List<RoleDropDown>> GetNotAssignedRole(string userId);
        Task<List<RoleDropDown>> GetAssignedRoles(string userId);
        Task<ResponseMessage> AssignRole(UserRoleDto userRole);
        Task<ResponseMessage> RevokeRole(UserRoleDto userRole);
        //Task<ResponseMessage> ChangeStatusOfUser(string userId);
        Task<ResponseMessage> AddUser(AddUSerDto addUSer);
        Task<ResponseMessage> ChangePassword(ChangePasswordDto model);
    }
}
