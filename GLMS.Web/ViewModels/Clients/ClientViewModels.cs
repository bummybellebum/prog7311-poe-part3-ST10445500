using GLMS.Web.ViewModels.Contracts;

//ST10445500 - PROG7311 - GLMS POE
//ClientViewModels

//.....................................o0oSTART OF FILEo0o........................................//

// The view model contains only the data needed by the MVC screen.

//ST10445500 - PROG7311 - GLMS POE
//ClientViewModels

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web.ViewModels.Clients
{
    public class ClientListItemViewModel
    {
        public int ClientId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Region { get; set; }
        public string? Country { get; set; }
        public bool IsActive { get; set; }
    }

    public class ClientDetailsViewModel
    {
        public int ClientId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public bool IsActive { get; set; }
        public int ContractCount { get; set; }
        public int ActiveContractCount { get; set; }
        public List<ContractListItemViewModel> Contracts { get; set; } = new();
    }

    public class ClientDeleteViewModel
    {
        public int ClientId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}

//..........................................o0oEND OF FILEo0o..................................................//

//.....................................o0oEND OF FILEo0o..........................................//
