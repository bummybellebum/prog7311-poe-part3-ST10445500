using GLMS.Web.Security;
using GLMS.Web.Services;
using GLMS.Web.ApiModels;
using GLMS.Web.Mappings;
using GLMS.Web.ViewModels.ServiceRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestsController

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.AllRoles)]
    public class ServiceRequestsController : Controller
    {
        private readonly IServiceRequestService _serviceRequestService;
        private readonly IContractService _contractService;
        private readonly ILookupService _lookupService;
        private readonly ICurrencyExchangeService _currencyExchangeService;

        public ServiceRequestsController(
            IServiceRequestService serviceRequestService,
            IContractService contractService,
            ILookupService lookupService,
            ICurrencyExchangeService currencyExchangeService)
        {
            _serviceRequestService = serviceRequestService;
            _contractService = contractService;
            _lookupService = lookupService;
            _currencyExchangeService = currencyExchangeService;
        }

        //............................................................................................//

        public async Task<IActionResult> Index(ServiceRequestFilterViewModel filter)
        {
            var requests = await _serviceRequestService.GetAllAsync(filter.ContractId, filter.StatusId);

            filter.Requests = requests
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => r.ToListViewModel())
                .ToList();
            await PopulateFilterOptionsAsync(filter);

            return View(filter);
        }

        //............................................................................................//

        public async Task<IActionResult> Details(int id)
        {
            var request = await _serviceRequestService.GetDetailsAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            var statuses = await _lookupService.GetServiceRequestStatusesAsync();
            var vm = request.ToDetailsViewModel();
            vm.StatusOptions = statuses
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

            try
            {
                vm.ExchangeRateToZar = await _currencyExchangeService.GetRateToZarAsync(vm.OriginalCurrencyCode);
            }
            catch (Exception ex)
            {
                vm.ExchangeRateToZar = 0m;
                ViewData["CurrencyWarning"] = ex.Message;
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

            try
            {
                var entity = new CreateServiceRequestDto
                {
                    ContractId = vm.ContractId,
                    Description = vm.Description,
                    AmountOriginal = vm.AmountOriginal,
                    OriginalCurrencyCode = vm.OriginalCurrencyCode.Trim().ToUpperInvariant(),
                    ServiceRequestStatusId = vm.ServiceRequestStatusId
                };

                await _serviceRequestService.CreateAsync(entity);
                TempData["SuccessMessage"] = "Service request created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is KeyNotFoundException)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateFormOptionsAsync(vm, includeOnlyActiveContracts: true);
                return View(vm);
            }
        }

        //............................................................................................//

        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> Edit(int id)
        {
            var request = await _serviceRequestService.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }

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

            try
            {
                var existing = await _serviceRequestService.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

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

                await _serviceRequestService.UpdateAsync(dto);
                TempData["SuccessMessage"] = "Service request updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is KeyNotFoundException)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateFormOptionsAsync(vm, includeOnlyActiveContracts: false);
                return View(vm);
            }
        }

        //............................................................................................//

        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _serviceRequestService.GetDetailsAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request.ToDeleteViewModel());
        }

        //............................................................................................//

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _serviceRequestService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Service request deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is KeyNotFoundException || ex is InvalidOperationException)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
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

            try
            {
                var rate = await _currencyExchangeService.GetRateToZarAsync(currencyCode);
                return Json(new { rate });
            }
            catch (Exception ex)
            {
            return BadRequest(new { message = ex.Message });
            }
        }

        //............................................................................................//

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.AdminOrLogisticsManager)]
        public async Task<IActionResult> UpdateStatus(int id, int serviceRequestStatusId)
        {
            try
            {
                await _serviceRequestService.UpdateStatusAsync(id, serviceRequestStatusId);
                TempData["SuccessMessage"] = "Service request status updated successfully.";
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        //............................................................................................//

        private async Task PopulateFilterOptionsAsync(ServiceRequestFilterViewModel vm)
        {
            var contracts = await _contractService.GetAllAsync();
            var statuses = await _lookupService.GetServiceRequestStatusesAsync();

            vm.ContractOptions = contracts
                .OrderBy(c => c.Title)
                .Select(c => new SelectListItem(c.Title, c.ContractId.ToString()))
                .ToList();

            vm.StatusOptions = statuses
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
                .ToList();
        }

        //............................................................................................//

        private async Task PopulateFormOptionsAsync(ServiceRequestFormViewModel vm, bool includeOnlyActiveContracts)
        {
            var contracts = await _contractService.GetAllAsync();
            var statuses = await _lookupService.GetServiceRequestStatusesAsync();

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

            try
            {
                var currencies = await _currencyExchangeService.GetSupportedCurrenciesAsync();
                vm.CurrencyOptions = currencies
                    .OrderBy(c => c.Key)
                    .Select(c => new SelectListItem($"{c.Key} - {c.Value}", c.Key))
                    .ToList();
            }
            catch (Exception ex)
            {
                vm.CurrencyOptions = new List<SelectListItem>();
                ModelState.AddModelError(string.Empty, $"Unable to load currency list: {ex.Message}");
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

    }
}

//.....................................o0oEND OF FILEo0o........................................//
