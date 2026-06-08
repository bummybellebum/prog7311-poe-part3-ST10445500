using GLMS.Web.Security;
using GLMS.Web.Services;
using GLMS.Web.ApiModels;
using GLMS.Web.ViewModels.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//ST10445500 - PROG7311 - GLMS POE
//ClientsController

//.....................................o0oSTART OF FILEo0o........................................//
namespace GLMS.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.AllRoles)]
    public class ClientsController : Controller
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        //........................................................................................//

        public async Task<IActionResult> Index(string? search)
        {
            try
            {
                var clients = await _clientService.GetAllAsync(search);

                ViewData["Search"] = search;
                return View(clients.OrderBy(c => c.CompanyName).ToList());
            }
            catch (ApiUnavailableException ex)
            {
                ViewData["Search"] = search;
                ViewData["ErrorMessage"] = ex.Message;
                return View(new List<ClientListDto>());
            }
        }

        //........................................................................................//

        public async Task<IActionResult> Details(int id)
        {
            var client = await _clientService.GetWithContractsAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            var activeCount = client.Contracts.Count(c => string.Equals(c.ContractStatusName, "Active", StringComparison.OrdinalIgnoreCase));

            var vm = new ClientDetailsViewModel
            {
                Client = client,
                ContractCount = client.Contracts.Count,
                ActiveContractCount = activeCount
            };

            return View(vm);
        }

        //........................................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        public IActionResult Create()
        {
            return View(new ClientFormViewModel());
        }

        //........................................................................................//

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        public async Task<IActionResult> Create(ClientFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                var client = new CreateClientDto
                {
                    CompanyName = vm.CompanyName,
                    Email = vm.Email,
                    Phone = vm.Phone,
                    Region = vm.Region,
                    Country = vm.Country,
                    IsActive = vm.IsActive
                };

                await _clientService.CreateAsync(client);
                TempData["SuccessMessage"] = "Client created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }

        //........................................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            var vm = new ClientFormViewModel
            {
                ClientId = client.ClientId,
                CompanyName = client.CompanyName,
                Email = client.Email,
                Phone = client.Phone,
                Region = client.Region,
                Country = client.Country,
                IsActive = client.IsActive
            };

            return View(vm);
        }

        //........................................................................................//

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        public async Task<IActionResult> Edit(int id, ClientFormViewModel vm)
        {
            if (id != vm.ClientId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                var existing = new UpdateClientDto
                {
                    ClientId = id,
                    CompanyName = vm.CompanyName,
                    Email = vm.Email,
                    Phone = vm.Phone,
                    Region = vm.Region,
                    Country = vm.Country,
                    IsActive = vm.IsActive
                };

                await _clientService.UpdateAsync(existing);
                TempData["SuccessMessage"] = "Client updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }

        //........................................................................................//

        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        //........................................................................................//

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _clientService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Client deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is KeyNotFoundException || ex is InvalidOperationException)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        //........................................................................................//

    }
}

//.....................................o0oEND OF FILEo0o........................................//
