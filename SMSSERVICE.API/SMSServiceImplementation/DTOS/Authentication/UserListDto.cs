using IntegratedImplementation.DTOS.HRM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Implementation.DTOS.Authentication
{
    public class UserListDto
    {
        public string Id { get; set; } = null!;
        public Guid OrganizationId { get; set; }
        public string Name { get; set; } = null!;
        
        public string ImagePath { get; set; }

        public List<RoleDropDown> Roles { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; } = null!;
        public string Status { get; set; } = null!;
        

    

    }

    public class RoleDropDown
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }



    public class AddUSerDto
    {
        public Guid OrganizationId { get; set; }
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;

        public string ? Roles { get; set; }  
    }

    public class UserRoleDto
    {
        public string UserId { get; set; } = null!;
        public string[] RoleName { get; set; } = null!;
    }

    public class ChangePasswordDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }

}
