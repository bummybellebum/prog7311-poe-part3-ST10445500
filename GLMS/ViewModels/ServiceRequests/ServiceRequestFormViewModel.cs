using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestFormViewModel

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.ViewModels.ServiceRequests
{
    public class ServiceRequestFormViewModel
    {
        public int? ServiceRequestId { get; set; }

        [Required]
        [Display(Name = "Contract")]
        public int ContractId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 999999999)]
        [Display(Name = "Original Amount")]
        public decimal AmountOriginal { get; set; }

        [Required]
        [StringLength(3)]
        [Display(Name = "Currency")]
        public string OriginalCurrencyCode { get; set; } = "USD";

        [Display(Name = "Exchange Rate to ZAR")]
        public decimal ExchangeRateToZar { get; set; }

        [Display(Name = "Amount in ZAR")]
        public decimal AmountZar { get; set; }

        [Required]
        [Display(Name = "Status")]
        public int ServiceRequestStatusId { get; set; }

        public List<SelectListItem> ContractOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
        public List<SelectListItem> CurrencyOptions { get; set; } = new();
    }
}

//..........................................o0oEND OF FILEo0o..................................................//