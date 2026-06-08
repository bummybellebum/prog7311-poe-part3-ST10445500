using GLMS.Web.ApiClients;
using GLMS.Web.Models;
using GLMS.Web.Security;
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
        private readonly IClientsApiClient _clientsApiClient;
        private readonly IContractsApiClient _contractsApiClient;
        private readonly IServiceRequestsApiClient _serviceRequestsApiClient;

        public HomeController(
            IClientsApiClient clientsApiClient,
            IContractsApiClient contractsApiClient,
            IServiceRequestsApiClient serviceRequestsApiClient)
        {
            _clientsApiClient = clientsApiClient;
            _contractsApiClient = contractsApiClient;
            _serviceRequestsApiClient = serviceRequestsApiClient;
        }

        //............................................................................................//

        public async Task<IActionResult> Index()
        {
            var clientsResult = await _clientsApiClient.GetAllAsync();
            var contractsResult = await _contractsApiClient.GetAllAsync();
            var serviceRequestsResult = await _serviceRequestsApiClient.GetAllAsync();

            if (!clientsResult.IsSuccess || !contractsResult.IsSuccess || !serviceRequestsResult.IsSuccess)
            {
                ViewData["ErrorMessage"] = clientsResult.ErrorMessage ?? contractsResult.ErrorMessage ?? serviceRequestsResult.ErrorMessage;
                return View(new DashboardViewModel());
            }

            var clients = clientsResult.Data!;
            var contracts = contractsResult.Data!;
            var serviceRequests = serviceRequestsResult.Data!;

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
                CompletedServiceRequests = serviceRequests.Count(sr => string.Equals(sr.ServiceRequestStatusName, "Completed", StringComparison.OrdinalIgnoreCase)),
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
