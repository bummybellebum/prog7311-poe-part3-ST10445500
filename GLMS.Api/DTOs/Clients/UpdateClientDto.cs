using System.ComponentModel.DataAnnotations;

namespace GLMS.Api.DTOs.Clients
{
    public class UpdateClientDto
    {
        [Required]
        public int ClientId { get; set; }

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

        public bool IsActive { get; set; }
    }
}
