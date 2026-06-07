using GLMS.Web.Models;

//ST10445500 - PROG7311 - GLMS POE
//ClientDetailsViewModel

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web.ViewModels.Clients
{
    public class ClientDetailsViewModel
    {
        public Client Client { get; set; } = new();
        public int ContractCount { get; set; }
        public int ActiveContractCount { get; set; }
    }
}

//..........................................o0oEND OF FILEo0o..................................................//
