using System.ComponentModel.DataAnnotations;

//ST10445500 - PROG7311 - GLMS POE
//LoginRequestDto

//.....................................o0oSTART OF FILEo0o........................................//

// The DTO keeps API input and output simple instead of exposing full EF models.

namespace GLMS.Api.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
