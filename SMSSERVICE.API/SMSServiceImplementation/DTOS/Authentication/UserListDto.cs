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
        public string PhoneNumber { get; set; }

        public string UserName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime? LastLoginDate { get; set; }
    }

    public class RoleDropDown
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }



    public class AddUSerDto
    {
        [Required(ErrorMessage = "Organization is required")]
        public Guid OrganizationId { get; set; }
        
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string UserName { get; set; } = null!;
        
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "Password must be at least 12 characters long")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = null!;

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        public string? Roles { get; set; }
        
        public bool IsActive { get; set; } = true;
    }

    public class UserRoleDto
    {
        public string UserId { get; set; } = null!;
        public string[] RoleName { get; set; } = null!;
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "New password must be at least 8 characters long")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }

    public class UserProfileDto
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? ImagePath { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; } = null!;
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    public class UserProfileUpdateDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        public string? ImagePath { get; set; }
    }

    public class PasswordResetDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "Password must be at least 12 characters long")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "Reset token is required")]
        public string ResetToken { get; set; } = null!;
    }

    public class UserUpdateDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Organization is required")]
        public Guid OrganizationId { get; set; }

        [Required(ErrorMessage = "At least one role must be selected")]
        public string[] Roles { get; set; } = new string[0];

        public bool IsActive { get; set; } = true;
    }

    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "New password must be at least 12 characters long")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Reset token is required")]
        public string ResetToken { get; set; }
    }



}
