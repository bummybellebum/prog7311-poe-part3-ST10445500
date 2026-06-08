using GLMS.Web.ApiModels;
using Microsoft.AspNetCore.Mvc.Rendering;

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestDetailsViewModel

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web.ViewModels.ServiceRequests
{
    public class ServiceRequestDetailsViewModel
    {
        public ServiceRequestDetailDto Request { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }
}

//..........................................o0oEND OF FILEo0o..................................................//
