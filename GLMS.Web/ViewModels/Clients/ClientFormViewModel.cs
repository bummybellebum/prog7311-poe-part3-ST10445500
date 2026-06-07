using System.ComponentModel.DataAnnotations;

//ST10445500 - PROG7311 - GLMS POE
//ClientFormViewModel

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web.ViewModels.Clients
{
    public class ClientFormViewModel
    {
        public int? ClientId { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Company Name")]
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

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}

//..........................................o0oEND OF FILEo0o..................................................//
