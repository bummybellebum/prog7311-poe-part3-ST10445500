using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

//ST10445500 - PROG7311 - GLMS POE
//AdminUserEditViewModel

//.....................................o0oSTART OF FILEo0o........................................//

// The view model contains only the data needed by the MVC screen.

namespace GLMS.Web.ViewModels.Account
{
    public class AdminUserEditViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

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
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public List<SelectListItem> RoleOptions { get; set; } = [];
    }
}

//.....................................o0oEND OF FILEo0o........................................//

//.....................................o0oEND OF FILEo0o..........................................//
