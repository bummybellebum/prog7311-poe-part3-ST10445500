using System.ComponentModel.DataAnnotations;

//ST10445500 - PROG7311 - GLMS POE
//AdminResetPasswordViewModel

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.ViewModels.Account
{
    public class AdminResetPasswordViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

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
    }
}

//.....................................o0oEND OF FILEo0o........................................//

