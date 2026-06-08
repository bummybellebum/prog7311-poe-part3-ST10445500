using System.ComponentModel.DataAnnotations;

//ST10445500 - PROG7311 - GLMS POE
//CreateClientDto

//.....................................o0oSTART OF FILEo0o........................................//

// The DTO keeps API input and output simple instead of exposing full EF models.

namespace GLMS.Api.DTOs.Clients
{
    public class CreateClientDto
    {
        [Required]
        [StringLength(255)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Region { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
