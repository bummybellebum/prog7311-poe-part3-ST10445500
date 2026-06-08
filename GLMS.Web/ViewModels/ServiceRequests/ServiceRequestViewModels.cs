using Microsoft.AspNetCore.Mvc.Rendering;

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestViewModels

//.....................................o0oSTART OF FILEo0o........................................//

// The view model contains only the data needed by the MVC screen.

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestViewModels

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web.ViewModels.ServiceRequests
{
    public class ServiceRequestListItemViewModel
    {
        public int ServiceRequestId { get; set; }
        public int ContractId { get; set; }
        public string ContractTitle { get; set; } = string.Empty;
        public string ServiceRequestStatusName { get; set; } = string.Empty;
        public decimal AmountOriginal { get; set; }
        public string OriginalCurrencyCode { get; set; } = string.Empty;
        public decimal AmountZAR { get; set; }
        public DateTime RequestedAt { get; set; }
    }

    public class ServiceRequestDetailsViewModel : ServiceRequestListItemViewModel
    {
        public decimal ExchangeRateToZAR { get; set; }
        public string RequestedByUserId { get; set; } = string.Empty;
        public string? RequestedByEmail { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }

    public class ServiceRequestFilterViewModel
    {
        public int? ContractId { get; set; }
        public int? StatusId { get; set; }

        public List<SelectListItem> ContractOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();

        public List<ServiceRequestListItemViewModel> Requests { get; set; } = new();
    }

    public class ServiceRequestDeleteViewModel
    {
        public int ServiceRequestId { get; set; }
        public string ContractTitle { get; set; } = string.Empty;
        public string ServiceRequestStatusName { get; set; } = string.Empty;
        public decimal AmountZAR { get; set; }
    }
}

//..........................................o0oEND OF FILEo0o..................................................//

//.....................................o0oEND OF FILEo0o..........................................//
