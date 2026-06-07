using GLMS.Web.Models;
using GLMS.Web.Services;
using GLMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

//ST10445500 - PROG7311 - GLMS POE
//HomeController

//.....................................o0oSTART OF FILEo0o........................................//
namespace GLMS.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IClientService _clientService;
        private readonly IContractService _contractService;
        private readonly IServiceRequestService _serviceRequestService;

        public HomeController(
            IClientService clientService,
            IContractService contractService,
            IServiceRequestService serviceRequestService)
        {
            _clientService = clientService;
            _contractService = contractService;
            _serviceRequestService = serviceRequestService;
        }

        //............................................................................................//

        public async Task<IActionResult> Index()
        {
            try
            {
                var clients = await _clientService.GetAllAsync();
                var contracts = await _contractService.GetAllAsync();
                var serviceRequests = await _serviceRequestService.GetAllAsync();

                var vm = new DashboardViewModel
                {
                    TotalClients = clients.Count,
                    TotalContracts = contracts.Count,
                    ActiveContracts = contracts.Count(c => string.Equals(c.ContractStatusName, "Active", StringComparison.OrdinalIgnoreCase)),
                    ExpiredOrOnHoldContracts = contracts.Count(c =>
                        string.Equals(c.ContractStatusName, "Expired", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(c.ContractStatusName, "On Hold", StringComparison.OrdinalIgnoreCase)),
                    TotalServiceRequests = serviceRequests.Count,
                    PendingServiceRequests = serviceRequests.Count(sr => string.Equals(sr.ServiceRequestStatusName, "Pending", StringComparison.OrdinalIgnoreCase)),
                    RecentServiceRequests = serviceRequests
                        .OrderByDescending(sr => sr.RequestedAt)
                        .Take(5)
                        .Select(sr => new RecentServiceRequestItemViewModel
                        {
                            ServiceRequestId = sr.ServiceRequestId,
                            ContractId = sr.ContractId,
                            ContractTitle = sr.ContractTitle,
                            StatusName = sr.ServiceRequestStatusName,
                            RequestedAt = sr.RequestedAt
                        })
                        .ToList()
                };

                return View(vm);
            }
            catch (ApiUnavailableException ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
                return View(new DashboardViewModel());
            }
        }

        //............................................................................................//

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //............................................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
