using GLMS.Web.ApiClients;
using GLMS.Web.ApiClients.Models;
using GLMS.Web.Authorization;
using GLMS.Web.ViewModels.Mappings;
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
        private readonly IClientsApiClient _clientsApiClient;

        public ClientsController(IClientsApiClient clientsApiClient)
        {
            _clientsApiClient = clientsApiClient;
        }

        //........................................................................................//

        public async Task<IActionResult> Index(string? search)
        {
            var result = await _clientsApiClient.GetAllAsync(search);
            if (!result.IsSuccess)
            {
                ViewData["Search"] = search;
                ViewData["ErrorMessage"] = result.ErrorMessage;
                return View(new List<ClientListItemViewModel>());
            }

            ViewData["Search"] = search;
            return View(result.Data!
                .OrderBy(c => c.CompanyName)
                .Select(c => c.ToListViewModel())
                .ToList());
        }

        //........................................................................................//

        public async Task<IActionResult> Details(int id)
        {
            var result = await _clientsApiClient.GetWithContractsAsync(id);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            return View(result.Data.ToDetailsViewModel());
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

            var client = new CreateClientDto
            {
                CompanyName = vm.CompanyName,
                Email = vm.Email,
                Phone = vm.Phone,
                Region = vm.Region,
                Country = vm.Country,
                IsActive = vm.IsActive
            };

            var result = await _clientsApiClient.CreateAsync(client);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Client created successfully.";
                return RedirectToAction(nameof(Index));
            }

            AddError(result);
            return View(vm);
        }

        //........................................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _clientsApiClient.GetByIdAsync(id);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            var client = result.Data;
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

            var result = await _clientsApiClient.UpdateAsync(existing);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Client updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            AddError(result);
            return View(vm);
        }

        //........................................................................................//

        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _clientsApiClient.GetByIdAsync(id);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            return View(result.Data.ToDeleteViewModel());
        }

        //........................................................................................//

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _clientsApiClient.DeleteAsync(id);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Client deleted successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        //........................................................................................//

        private void AddError(ApiClientResult result)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "The request could not be completed.");
        }

    }
}

//.....................................o0oEND OF FILEo0o........................................//
