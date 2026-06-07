using GLMS.Models;
using GLMS.Services;
using GLMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

//ST10445500 - PROG7311 - GLMS POE
//HomeController

//.....................................o0oSTART OF FILEo0o........................................//
namespace GLMS.Controllers
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
            var clients = await _clientService.GetAllAsync();
            var contracts = await _contractService.GetAllAsync();
            var serviceRequests = await _serviceRequestService.GetAllAsync();

            var vm = new DashboardViewModel
            {
                TotalClients = clients.Count,
                TotalContracts = contracts.Count,
                ActiveContracts = contracts.Count(c => string.Equals(c.ContractStatus?.StatusName, "Active", StringComparison.OrdinalIgnoreCase)),
                TotalServiceRequests = serviceRequests.Count,
                RecentServiceRequests = serviceRequests
                    .OrderByDescending(sr => sr.RequestedAt)
                    .Take(5)
                    .Select(sr => new RecentServiceRequestItemViewModel
                    {
                        ServiceRequestId = sr.ServiceRequestId,
                        ContractId = sr.ContractId,
                        ContractTitle = sr.Contract?.Title ?? "-",
                        StatusName = sr.ServiceRequestStatus?.StatusName ?? "Unknown",
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
