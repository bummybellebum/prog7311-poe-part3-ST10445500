//ST10445500 - PROG7311 - GLMS POE
//DashboardViewModel

//.....................................o0oSTART OF FILEo0o........................................//

// The view model contains only the data needed by the MVC screen.



//ST10445500 - PROG7311 - GLMS POE
//DashboardViewModel

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalClients { get; set; }
        public int TotalContracts { get; set; }
        public int ActiveContracts { get; set; }
        public int ExpiredOrOnHoldContracts { get; set; }
        public int TotalServiceRequests { get; set; }
        public int PendingServiceRequests { get; set; }
        public int CompletedServiceRequests { get; set; }
        public List<RecentServiceRequestItemViewModel> RecentServiceRequests { get; set; } = new();
    }

    public class RecentServiceRequestItemViewModel
    {
        public int ServiceRequestId { get; set; }
        public int ContractId { get; set; }
        public string ContractTitle { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
    }
}

//..........................................o0oEND OF FILEo0o..................................................//

//.....................................o0oEND OF FILEo0o..........................................//
