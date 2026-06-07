using System.ComponentModel.DataAnnotations;

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
