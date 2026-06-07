using System.ComponentModel.DataAnnotations;
using GLMS.Api.Models;

namespace GLMS.Api.DTOs.Auth
{
    public class AdminUserListDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DisplayName => string.IsNullOrWhiteSpace($"{FirstName} {LastName}".Trim())
            ? Email
            : $"{FirstName} {LastName}".Trim();
    }

    public class AdminUserDetailDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Role { get; set; } = ApplicationRoles.LogisticsManager;
        public bool IsActive { get; set; }
    }

    public class CreateAdminUserDto
    {
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = ApplicationRoles.LogisticsManager;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string TemporaryPassword { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(TemporaryPassword), ErrorMessage = "The temporary password and confirmation password do not match.")]
        public string ConfirmTemporaryPassword { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateAdminUserDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    public class ResetAdminPasswordDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string TemporaryPassword { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(TemporaryPassword), ErrorMessage = "The temporary password and confirmation password do not match.")]
        public string ConfirmTemporaryPassword { get; set; } = string.Empty;
    }

    public class UpdateUserActiveDto
    {
        public bool IsActive { get; set; }
    }
}
