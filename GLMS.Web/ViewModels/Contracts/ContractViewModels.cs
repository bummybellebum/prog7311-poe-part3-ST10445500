using GLMS.Web.ViewModels.ServiceRequests;
using Microsoft.AspNetCore.Mvc.Rendering;

//ST10445500 - PROG7311 - GLMS POE
//ContractViewModels

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web.ViewModels.Contracts
{
    public class ContractListItemViewModel
    {
        public int ContractId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ContractStatusName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ContractDetailViewModel
    {
        public int ContractId { get; set; }
        public int ClientId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ContractStatusId { get; set; }
        public string ContractStatusName { get; set; } = string.Empty;
        public string? ServiceLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class ContractDocumentItemViewModel
    {
        public int ContractDocumentId { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    public class ContractDetailsViewModel
    {
        public ContractDetailViewModel Contract { get; set; } = new();
        public ContractDocumentItemViewModel? CurrentSignedAgreement { get; set; }
        public List<ContractDocumentItemViewModel> Documents { get; set; } = new();
        public List<ServiceRequestListItemViewModel> ServiceRequests { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }

    public class ContractFilterViewModel
    {
        public int? StatusId { get; set; }
        public int? ClientId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<SelectListItem> StatusOptions { get; set; } = new();
        public List<SelectListItem> ClientOptions { get; set; } = new();

        public List<ContractListItemViewModel> Contracts { get; set; } = new();
    }

    public class ContractDeleteViewModel
    {
        public int ContractId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ContractStatusName { get; set; } = string.Empty;
    }
}

//..........................................o0oEND OF FILEo0o..................................................//
