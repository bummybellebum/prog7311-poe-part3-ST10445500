using GLMS.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

//ST10445500 - PROG7311 - GLMS POE
//ContractFilterViewModel

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web.ViewModels.Contracts
{
    public class ContractFilterViewModel
    {
        public int? StatusId { get; set; }
        public int? ClientId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<SelectListItem> StatusOptions { get; set; } = new();
        public List<SelectListItem> ClientOptions { get; set; } = new();

        public List<Contract> Contracts { get; set; } = new();
    }
}

//..........................................o0oEND OF FILEo0o..................................................//