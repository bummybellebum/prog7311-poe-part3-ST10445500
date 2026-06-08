using System.ComponentModel.DataAnnotations;

//ST10445500 - PROG7311 - GLMS POE
//UpdateAccountProfileDto

//.....................................o0oSTART OF FILEo0o........................................//

// The DTO keeps API input and output simple instead of exposing full EF models.

namespace GLMS.Api.DTOs.Auth
{
    public class UpdateAccountProfileDto
    {
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
