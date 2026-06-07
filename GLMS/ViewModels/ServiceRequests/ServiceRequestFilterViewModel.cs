using GLMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestFilterViewModel

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.ViewModels.ServiceRequests
{
    public class ServiceRequestFilterViewModel
    {
        public int? ContractId { get; set; }
        public int? StatusId { get; set; }

        public List<SelectListItem> ContractOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();

        public List<ServiceRequest> Requests { get; set; } = new();
    }
}

//..........................................o0oEND OF FILEo0o..................................................//