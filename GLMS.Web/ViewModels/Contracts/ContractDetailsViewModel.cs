using GLMS.Web.ApiModels;
using Microsoft.AspNetCore.Mvc.Rendering;

//ST10445500 - PROG7311 - GLMS POE
//ContractDetailsViewModel

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web.ViewModels.Contracts
{
    public class ContractDetailsViewModel
    {
        public ContractDetailDto Contract { get; set; } = new();
        public ContractDocumentDto? CurrentSignedAgreement { get; set; }
        public List<ContractDocumentDto> Documents { get; set; } = new();
        public List<ServiceRequestListDto> ServiceRequests { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }
}

//..........................................o0oEND OF FILEo0o..................................................//
