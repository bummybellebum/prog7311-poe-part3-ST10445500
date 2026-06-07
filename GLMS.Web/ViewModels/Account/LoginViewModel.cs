using System.ComponentModel.DataAnnotations;

//ST10445500 - PROG7311 - GLMS POE
//LoginViewModel

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}

//.....................................o0oEND OF FILEo0o........................................//
