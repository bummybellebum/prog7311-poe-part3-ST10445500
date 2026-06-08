using System.ComponentModel.DataAnnotations;
using GLMS.Web.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

//ST10445500 - PROG7311 - GLMS POE
//AdminUserCreateViewModel

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.ViewModels.Account
{
    public class AdminUserCreateViewModel
    {
        [Display(Name = "First name")]
        [StringLength(100)]
        public string? FirstName { get; set; }

        [Display(Name = "Last name")]
        [StringLength(100)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = ApplicationRoles.LogisticsManager;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Temporary password")]
        public string TemporaryPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm temporary password")]
        [Compare(nameof(TemporaryPassword), ErrorMessage = "The temporary password and confirmation password do not match.")]
        public string ConfirmTemporaryPassword { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public List<SelectListItem> RoleOptions { get; set; } = [];
    }
}

//.....................................o0oEND OF FILEo0o........................................//
