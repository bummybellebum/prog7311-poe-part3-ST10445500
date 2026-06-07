using GLMS.Models;

//ST10445500 - PROG7311 - GLMS POE
//ContractDetailsViewModel

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.ViewModels.Contracts
{
    public class ContractDetailsViewModel
    {
        public Contract Contract { get; set; } = new();
        public ContractDocument? CurrentSignedAgreement { get; set; }
        public List<ContractDocument> Documents { get; set; } = new();
        public List<ServiceRequest> ServiceRequests { get; set; } = new();
    }
}

//..........................................o0oEND OF FILEo0o..................................................//
