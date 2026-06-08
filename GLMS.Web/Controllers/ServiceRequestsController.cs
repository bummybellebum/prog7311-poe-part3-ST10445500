using GLMS.Web.ApiClients;
using GLMS.Web.ApiClients.Models;
using GLMS.Web.Authorization;
using GLMS.Web.ViewModels.Mappings;
using GLMS.Web.ViewModels.ServiceRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestsController

//.....................................o0oSTART OF FILEo0o........................................//

// The MVC controller handles screen flow and calls the API instead of using SQL directly.

namespace GLMS.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.AllRoles)]
    public class ServiceRequestsController : Controller
    {
        private readonly IServiceRequestsApiClient _serviceRequestsApiClient;
        private readonly IContractsApiClient _contractsApiClient;
        private readonly ILookupsApiClient _lookupsApiClient;

        public ServiceRequestsController(
            IServiceRequestsApiClient serviceRequestsApiClient,
            IContractsApiClient contractsApiClient,
            ILookupsApiClient lookupsApiClient)
        {
            _serviceRequestsApiClient = serviceRequestsApiClient;
            _contractsApiClient = contractsApiClient;
            _lookupsApiClient = lookupsApiClient;
        }

        //............................................................................................//

        public async Task<IActionResult> Index(ServiceRequestFilterViewModel filter)
        {
            var result = await _serviceRequestsApiClient.GetAllAsync(filter.ContractId, filter.StatusId);
            if (!result.IsSuccess)
            {
                ViewData["ErrorMessage"] = result.ErrorMessage;
                filter.Requests = [];
                await PopulateFilterOptionsAsync(filter);
                return View(filter);
            }

            filter.Requests = result.Data!
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => r.ToListViewModel())
                .ToList();
            await PopulateFilterOptionsAsync(filter);

            return View(filter);
        }

        //............................................................................................//

        public async Task<IActionResult> Details(int id)
        {
            var result = await _serviceRequestsApiClient.GetDetailsAsync(id);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            var request = result.Data;
            var statuses = await _lookupsApiClient.GetServiceRequestStatusesAsync();
            var vm = request.ToDetailsViewModel();
            vm.StatusOptions = (statuses.Data ?? [])
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem(s.Name, s.Id.ToString(), s.Id == request.ServiceRequestStatusId))
                .ToList();

            return View(vm);
        }

        //............................................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrLogisticsManager)]
        public async Task<IActionResult> Create(int? contractId)
        {
            var vm = new ServiceRequestFormViewModel
            {
                AmountOriginal = 0m,
                AmountZar = 0m,
                OriginalCurrencyCode = "USD"
            };

            await PopulateFormOptionsAsync(vm, includeOnlyActiveContracts: true);

            var rateResult = await _serviceRequestsApiClient.GetRateToZarAsync(vm.OriginalCurrencyCode);
            if (rateResult.IsSuccess)
            {
                vm.ExchangeRateToZar = rateResult.Data;
            }
            else
            {
                vm.ExchangeRateToZar = 0m;
                ViewData["CurrencyWarning"] = rateResult.ErrorMessage;
            }

            if (contractId.HasValue)
            {
                var hasContract = vm.ContractOptions.Any(c => c.Value == contractId.Value.ToString());
                if (hasContract)
                {
                    vm.ContractId = contractId.Value;
                    ViewData["PreselectedContract"] = true;
                }
            }

            return View(vm);
        }

        //............................................................................................//

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.AdminOrLogisticsManager)]
        public async Task<IActionResult> Create(ServiceRequestFormViewModel vm)
        {
            if (vm.ExchangeRateToZar > 0)
            {
                vm.AmountZar = decimal.Round(vm.AmountOriginal * vm.ExchangeRateToZar, 2, MidpointRounding.AwayFromZero);
            }

            if (!ModelState.IsValid)
            {
                await PopulateFormOptionsAsync(vm, includeOnlyActiveContracts: true);
                return View(vm);
            }

            var entity = new CreateServiceRequestDto
            {
                ContractId = vm.ContractId,
                Description = vm.Description,
                AmountOriginal = vm.AmountOriginal,
                OriginalCurrencyCode = vm.OriginalCurrencyCode.Trim().ToUpperInvariant(),
                ServiceRequestStatusId = vm.ServiceRequestStatusId
            };

            var result = await _serviceRequestsApiClient.CreateAsync(entity);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Service request created successfully.";
                return RedirectToAction(nameof(Index));
            }

            AddError(result);
            await PopulateFormOptionsAsync(vm, includeOnlyActiveContracts: true);
            return View(vm);
        }

        //............................................................................................//

        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _serviceRequestsApiClient.GetByIdAsync(id);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            var request = result.Data;
            var vm = new ServiceRequestFormViewModel
            {
                ServiceRequestId = request.ServiceRequestId,
                ContractId = request.ContractId,
                Description = request.Description,
                AmountOriginal = request.AmountOriginal,
                OriginalCurrencyCode = request.OriginalCurrencyCode,
                ExchangeRateToZar = request.ExchangeRateToZAR,
                AmountZar = request.AmountZAR,
                ServiceRequestStatusId = request.ServiceRequestStatusId
            };

            await PopulateFormOptionsAsync(vm, includeOnlyActiveContracts: false);
            return View(vm);
        }

        //............................................................................................//

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> Edit(int id, ServiceRequestFormViewModel vm)
        {
            if (id != vm.ServiceRequestId)
            {
                return NotFound();
            }

            if (vm.ExchangeRateToZar > 0)
            {
                vm.AmountZar = decimal.Round(vm.AmountOriginal * vm.ExchangeRateToZar, 2, MidpointRounding.AwayFromZero);
            }

            if (!ModelState.IsValid)
            {
                await PopulateFormOptionsAsync(vm, includeOnlyActiveContracts: false);
                return View(vm);
            }

            var existingResult = await _serviceRequestsApiClient.GetByIdAsync(id);
            if (!existingResult.IsSuccess || existingResult.Data == null)
            {
                return NotFound();
            }

            var existing = existingResult.Data;
            var dto = new UpdateServiceRequestDto
            {
                ServiceRequestId = id,
                ContractId = vm.ContractId,
                RequestedByUserId = existing.RequestedByUserId,
                Description = vm.Description,
                AmountOriginal = vm.AmountOriginal,
                OriginalCurrencyCode = vm.OriginalCurrencyCode.Trim().ToUpperInvariant(),
                ServiceRequestStatusId = vm.ServiceRequestStatusId,
                RequestedAt = existing.RequestedAt
            };

            var result = await _serviceRequestsApiClient.UpdateAsync(dto);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Service request updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            AddError(result);
            await PopulateFormOptionsAsync(vm, includeOnlyActiveContracts: false);
            return View(vm);
        }

        //............................................................................................//

        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _serviceRequestsApiClient.GetDetailsAsync(id);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            return View(result.Data.ToDeleteViewModel());
        }

        //............................................................................................//

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _serviceRequestsApiClient.DeleteAsync(id);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Service request deleted successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        //............................................................................................//

        [HttpGet]
        [Authorize(Roles = ApplicationRoles.AdminOrLogisticsManager)]
        public async Task<IActionResult> GetExchangeRate(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                return BadRequest(new { message = "Currency code is required." });
            }

            var result = await _serviceRequestsApiClient.GetRateToZarAsync(currencyCode);
            if (result.IsSuccess)
            {
                return Json(new { rate = result.Data });
            }

            return BadRequest(new { message = result.ErrorMessage });
        }

        //............................................................................................//

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.AdminOrLogisticsManager)]
        public async Task<IActionResult> UpdateStatus(int id, int serviceRequestStatusId)
        {
            var result = await _serviceRequestsApiClient.UpdateStatusAsync(id, serviceRequestStatusId);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Service request status updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        //............................................................................................//

        private async Task PopulateFilterOptionsAsync(ServiceRequestFilterViewModel vm)
        {
            var contracts = await _contractsApiClient.GetAllAsync();
            var statuses = await _lookupsApiClient.GetServiceRequestStatusesAsync();

            vm.ContractOptions = (contracts.Data ?? [])
                .OrderBy(c => c.Title)
                .Select(c => new SelectListItem(c.Title, c.ContractId.ToString()))
                .ToList();

            vm.StatusOptions = (statuses.Data ?? [])
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
                .ToList();
        }

        //............................................................................................//

        private async Task PopulateFormOptionsAsync(ServiceRequestFormViewModel vm, bool includeOnlyActiveContracts)
        {
            var contractsResult = await _contractsApiClient.GetAllAsync();
            var statusesResult = await _lookupsApiClient.GetServiceRequestStatusesAsync();
            var contracts = contractsResult.Data ?? [];
            var statuses = statusesResult.Data ?? [];

            if (includeOnlyActiveContracts)
            {
                contracts = contracts
                    .Where(c => string.Equals(c.ContractStatusName, "Active", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            vm.ContractOptions = contracts
                .OrderBy(c => c.Title)
                .Select(c => new SelectListItem(c.Title, c.ContractId.ToString()))
                .ToList();

            vm.StatusOptions = statuses
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
                .ToList();

            var currenciesResult = await _serviceRequestsApiClient.GetSupportedCurrenciesAsync();
            if (currenciesResult.IsSuccess)
            {
                vm.CurrencyOptions = currenciesResult.Data!
                    .OrderBy(c => c.Key)
                    .Select(c => new SelectListItem($"{c.Key} - {c.Value}", c.Key))
                    .ToList();
            }
            else
            {
                vm.CurrencyOptions = new List<SelectListItem>();
                ModelState.AddModelError(string.Empty, $"Unable to load currency list: {currenciesResult.ErrorMessage}");
            }

            if (vm.ServiceRequestStatusId == 0)
            {
                var pending = statuses.FirstOrDefault(s => string.Equals(s.Name, "Pending", StringComparison.OrdinalIgnoreCase));
                if (pending != null)
                {
                    vm.ServiceRequestStatusId = pending.Id;
                }
            }

            if (string.IsNullOrWhiteSpace(vm.OriginalCurrencyCode))
            {
                vm.OriginalCurrencyCode = "USD";
            }
        }

        //............................................................................................//

        private void AddError(ApiClientResult result)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "The request could not be completed.");
        }

    }
}

//.....................................o0oEND OF FILEo0o........................................//

//.....................................o0oEND OF FILEo0o..........................................//
