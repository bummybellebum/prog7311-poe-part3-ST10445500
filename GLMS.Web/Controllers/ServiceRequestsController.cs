using GLMS.Web.Models;
using GLMS.Web.Services;
using GLMS.Web.ViewModels.ServiceRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestsController

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Controllers
{
    [Authorize(Roles = "Admin,LogisticsManager")]
    public class ServiceRequestsController : Controller
    {
        private readonly IServiceRequestService _serviceRequestService;
        private readonly IContractService _contractService;
        private readonly ILookupService _lookupService;
        private readonly ICurrencyExchangeService _currencyExchangeService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServiceRequestsController(
            IServiceRequestService serviceRequestService,
            IContractService contractService,
            ILookupService lookupService,
            ICurrencyExchangeService currencyExchangeService,
            UserManager<ApplicationUser> userManager)
        {
            _serviceRequestService = serviceRequestService;
            _contractService = contractService;
            _lookupService = lookupService;
            _currencyExchangeService = currencyExchangeService;
            _userManager = userManager;
        }

        //............................................................................................//

        public async Task<IActionResult> Index(ServiceRequestFilterViewModel filter)
        {
            var requests = await _serviceRequestService.GetAllAsync();

            if (filter.ContractId.HasValue)
            {
                requests = requests.Where(r => r.ContractId == filter.ContractId.Value).ToList();
            }

            if (filter.StatusId.HasValue)
            {
                requests = requests.Where(r => r.ServiceRequestStatusId == filter.StatusId.Value).ToList();
            }

            filter.Requests = requests.OrderByDescending(r => r.RequestedAt).ToList();
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

            return View(request);
        }

        //............................................................................................//

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
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Forbid();
                }

                var entity = new ServiceRequest
                {
                    ContractId = vm.ContractId,
                    RequestedByUserId = userId,
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

                existing.ContractId = vm.ContractId;
                existing.Description = vm.Description;
                existing.AmountOriginal = vm.AmountOriginal;
                existing.OriginalCurrencyCode = vm.OriginalCurrencyCode.Trim().ToUpperInvariant();
                existing.ServiceRequestStatusId = vm.ServiceRequestStatusId;

                await _serviceRequestService.UpdateAsync(existing);
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

        public async Task<IActionResult> Delete(int id)
        {
            var request = await _serviceRequestService.GetDetailsAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        //............................................................................................//

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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

        private async Task PopulateFilterOptionsAsync(ServiceRequestFilterViewModel vm)
        {
            var contracts = await _contractService.GetAllAsync();
            var statuses = await _lookupService.GetServiceRequestStatusesAsync();

            vm.ContractOptions = contracts
                .OrderBy(c => c.Title)
                .Select(c => new SelectListItem(c.Title, c.ContractId.ToString()))
                .ToList();

            vm.StatusOptions = statuses
                .OrderBy(s => s.StatusName)
                .Select(s => new SelectListItem(s.StatusName, s.ServiceRequestStatusId.ToString()))
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
                    .Where(c => string.Equals(c.ContractStatus?.StatusName, "Active", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            vm.ContractOptions = contracts
                .OrderBy(c => c.Title)
                .Select(c => new SelectListItem(c.Title, c.ContractId.ToString()))
                .ToList();

            vm.StatusOptions = statuses
                .OrderBy(s => s.StatusName)
                .Select(s => new SelectListItem(s.StatusName, s.ServiceRequestStatusId.ToString()))
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
                var pending = statuses.FirstOrDefault(s => string.Equals(s.StatusName, "Pending", StringComparison.OrdinalIgnoreCase));
                if (pending != null)
                {
                    vm.ServiceRequestStatusId = pending.ServiceRequestStatusId;
                }
            }

            if (string.IsNullOrWhiteSpace(vm.OriginalCurrencyCode))
            {
                vm.OriginalCurrencyCode = "USD";
            }
        }

        //............................................................................................//
    }
}

//.....................................o0oEND OF FILEo0o........................................//
