using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

//ST10445500 - PROG7311 - GLMS POE
//ContractFormViewModel

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.ViewModels.Contracts
{
    public class ContractFormViewModel
    {
        public int? ContractId { get; set; }

        [Required]
        [Display(Name = "Client")]
        public int ClientId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Display(Name = "Status")]
        public int ContractStatusId { get; set; }

        [StringLength(100)]
        [Display(Name = "Service Level")]
        public string? ServiceLevel { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Display(Name = "Signed Agreement PDF (optional)")]
        public IFormFile? SignedAgreementFile { get; set; }

        public List<SelectListItem> ClientOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }
}

//..........................................o0oEND OF FILEo0o..................................................//