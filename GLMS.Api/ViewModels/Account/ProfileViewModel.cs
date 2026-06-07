using System.ComponentModel.DataAnnotations;

//ST10445500 - PROG7311 - GLMS POE
//ProfileViewModel

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.ViewModels.Account
{
    public class ProfileViewModel
    {
        [Display(Name = "First name")]
        [StringLength(100)]
        public string? FirstName { get; set; }

        [Display(Name = "Last name")]
        [StringLength(100)]
        public string? LastName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}

//.....................................o0oEND OF FILEo0o........................................//

