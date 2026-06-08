using GLMS.Web.Security;
using GLMS.Web.Services;
using GLMS.Web.ApiModels;
using GLMS.Web.ViewModels.Contracts;
using GLMS.Web.ViewModels.ServiceRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

//ST10445500 - PROG7311 - GLMS POE
//ContractsController

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.AllRoles)]
    public class ContractsController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IClientService _clientService;
        private readonly ILookupService _lookupService;
        private readonly IContractDocumentService _contractDocumentService;

        public ContractsController(
            IContractService contractService,
            IClientService clientService,
            ILookupService lookupService,
            IContractDocumentService contractDocumentService)
        {
            _contractService = contractService;
            _clientService = clientService;
            _lookupService = lookupService;
            _contractDocumentService = contractDocumentService;
        }

        //........................................................................................//

        public async Task<IActionResult> Index(ContractFilterViewModel filter)
        {
            var contracts = await _contractService.FilterAsync(filter.StatusId, filter.StartDate, filter.EndDate, filter.ClientId);

            filter.Contracts = contracts
                .OrderByDescending(c => c.CreatedAt)
                .Select(ToContractListItemViewModel)
                .ToList();
            await PopulateFilterOptionsAsync(filter);

            return View(filter);
        }

        //........................................................................................//

        public async Task<IActionResult> Details(int id)
        {
            var contract = await _contractService.GetDetailsAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            var documents = contract.Documents.Any()
                ? contract.Documents
                : await _contractDocumentService.GetByContractIdAsync(id);
            var current = documents.FirstOrDefault(d => d.IsCurrent);

            var vm = new ContractDetailsViewModel
            {
                Contract = ToContractDetailViewModel(contract),
                Documents = documents.Select(ToContractDocumentItemViewModel).ToList(),
                CurrentSignedAgreement = current == null ? null : ToContractDocumentItemViewModel(current),
                ServiceRequests = contract.ServiceRequests
                    .OrderByDescending(sr => sr.RequestedAt)
                    .Select(ToServiceRequestListItemViewModel)
                    .ToList()
            };

            var statuses = await _lookupService.GetContractStatusesAsync();
            vm.StatusOptions = statuses
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem(s.Name, s.Id.ToString(), s.Id == contract.ContractStatusId))
                .ToList();

            ViewData["UploadModel"] = new ContractDocumentUploadViewModel { ContractId = id };
            return View(vm);
        }

        //........................................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        public async Task<IActionResult> Create()
        {
            var vm = new ContractFormViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(12)
            };

            await PopulateFormOptionsAsync(vm);
            return View(vm);
        }

        //........................................................................................//

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        public async Task<IActionResult> Create(ContractFormViewModel vm)
        {
            if (vm.SignedAgreementFile != null && vm.SignedAgreementFile.Length > 0)
            {
                if (!IsValidSignedAgreementPdf(vm.SignedAgreementFile))
                {
                    ModelState.AddModelError(nameof(vm.SignedAgreementFile), "Only PDF files are allowed for signed agreements.");
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateFormOptionsAsync(vm);
                return View(vm);
            }

            try
            {
                var contract = new CreateContractDto
                {
                    ClientId = vm.ClientId,
                    Title = vm.Title,
                    StartDate = vm.StartDate,
                    EndDate = vm.EndDate,
                    ContractStatusId = vm.ContractStatusId,
                    ServiceLevel = vm.ServiceLevel,
                    Notes = vm.Notes
                };

                var created = await _contractService.CreateAsync(contract);

                if (vm.SignedAgreementFile != null && vm.SignedAgreementFile.Length > 0)
                {
                    await _contractDocumentService.UploadSignedAgreementAsync(created.ContractId, vm.SignedAgreementFile);
                    TempData["SuccessMessage"] = "Contract created successfully and signed agreement uploaded.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Contract created successfully.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateFormOptionsAsync(vm);
                return View(vm);
            }
        }

        //........................................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _contractService.GetByIdAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            var vm = new ContractFormViewModel
            {
                ContractId = contract.ContractId,
                ClientId = contract.ClientId,
                Title = contract.Title,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                ContractStatusId = contract.ContractStatusId,
                ServiceLevel = contract.ServiceLevel,
                Notes = contract.Notes
            };

            await PopulateFormOptionsAsync(vm);
            return View(vm);
        }

        //........................................................................................//

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        public async Task<IActionResult> Edit(int id, ContractFormViewModel vm)
        {
            if (id != vm.ContractId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateFormOptionsAsync(vm);
                return View(vm);
            }

            try
            {
                var existing = await _contractService.GetDetailsAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                var dto = new UpdateContractDto
                {
                    ContractId = id,
                    ClientId = vm.ClientId,
                    Title = vm.Title,
                    StartDate = vm.StartDate,
                    EndDate = vm.EndDate,
                    ContractStatusId = vm.ContractStatusId,
                    ServiceLevel = vm.ServiceLevel,
                    Notes = vm.Notes,
                    CreatedByUserId = existing.CreatedByUserId,
                    CreatedAt = existing.CreatedAt
                };

                await _contractService.UpdateAsync(dto);
                TempData["SuccessMessage"] = "Contract updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateFormOptionsAsync(vm);
                return View(vm);
            }
        }

        //........................................................................................//

        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _contractService.GetDetailsAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(ToContractDeleteViewModel(contract));
        }

        //........................................................................................//

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _contractService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Contract deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is KeyNotFoundException || ex is InvalidOperationException)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        //........................................................................................//

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        public async Task<IActionResult> UploadSignedAgreement(ContractDocumentUploadViewModel vm)
        {
            var contract = await _contractService.GetByIdAsync(vm.ContractId);
            if (contract == null)
            {
                return NotFound();
            }

            if (vm.File == null || vm.File.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a PDF file to upload.";
                return RedirectToAction(nameof(Details), new { id = vm.ContractId });
            }

            if (!IsValidSignedAgreementPdf(vm.File))
            {
                TempData["ErrorMessage"] = "Only PDF files are allowed for signed agreements.";
                return RedirectToAction(nameof(Details), new { id = vm.ContractId });
            }

            try
            {
                await _contractDocumentService.UploadSignedAgreementAsync(vm.ContractId, vm.File);
                TempData["SuccessMessage"] = "Signed agreement uploaded successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = vm.ContractId });
        }

        //........................................................................................//

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        public async Task<IActionResult> UpdateStatus(int id, int contractStatusId)
        {
            try
            {
                await _contractService.UpdateStatusAsync(id, contractStatusId);
                TempData["SuccessMessage"] = "Contract status updated successfully.";
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        //........................................................................................//

        [HttpGet]
        public async Task<IActionResult> DownloadAgreement(int id)
        {
            var document = await _contractDocumentService.GetByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            var file = await _contractDocumentService.DownloadAgreementAsync(id);
            if (file == null)
            {
                TempData["ErrorMessage"] = "The agreement file could not be found on the server.";
                return RedirectToAction(nameof(Details), new { id = document.ContractId });
            }

            return File(file.Bytes, file.ContentType, file.FileName);
        }

        //........................................................................................//

        private async Task PopulateFilterOptionsAsync(ContractFilterViewModel vm)
        {
            var statuses = await _lookupService.GetContractStatusesAsync();
            var clients = await _clientService.GetAllAsync();

            vm.StatusOptions = statuses
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
                .ToList();

            vm.ClientOptions = clients
                .OrderBy(c => c.CompanyName)
                .Select(c => new SelectListItem(c.CompanyName, c.ClientId.ToString()))
                .ToList();
        }

        //........................................................................................//

        private async Task PopulateFormOptionsAsync(ContractFormViewModel vm)
        {
            var statuses = await _lookupService.GetContractStatusesAsync();
            var clients = await _clientService.GetAllAsync();

            vm.StatusOptions = statuses
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
                .ToList();

            vm.ClientOptions = clients
                .Where(c => c.IsActive)
                .OrderBy(c => c.CompanyName)
                .Select(c => new SelectListItem(c.CompanyName, c.ClientId.ToString()))
                .ToList();
        }

        private bool IsValidSignedAgreementPdf(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            var contentType = file.ContentType ?? string.Empty;
            var isAllowedContentType = string.IsNullOrWhiteSpace(contentType)
                || string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
                || string.Equals(contentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase);

            return string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase) && isAllowedContentType;
        }

        private static ContractListItemViewModel ToContractListItemViewModel(ContractListDto contract)
        {
            return new ContractListItemViewModel
            {
                ContractId = contract.ContractId,
                Title = contract.Title,
                ClientName = contract.ClientName,
                ContractStatusName = contract.ContractStatusName,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                CreatedAt = contract.CreatedAt
            };
        }

        private static ContractDetailViewModel ToContractDetailViewModel(ContractDetailDto contract)
        {
            return new ContractDetailViewModel
            {
                ContractId = contract.ContractId,
                ClientId = contract.ClientId,
                Title = contract.Title,
                ClientName = contract.ClientName,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                ContractStatusId = contract.ContractStatusId,
                ContractStatusName = contract.ContractStatusName,
                ServiceLevel = contract.ServiceLevel,
                CreatedAt = contract.CreatedAt,
                Notes = contract.Notes
            };
        }

        private static ContractDocumentItemViewModel ToContractDocumentItemViewModel(ContractDocumentDto document)
        {
            return new ContractDocumentItemViewModel
            {
                ContractDocumentId = document.ContractDocumentId,
                OriginalFileName = document.OriginalFileName,
                UploadedAt = document.UploadedAt
            };
        }

        private static ServiceRequestListItemViewModel ToServiceRequestListItemViewModel(ServiceRequestListDto request)
        {
            return new ServiceRequestListItemViewModel
            {
                ServiceRequestId = request.ServiceRequestId,
                ContractId = request.ContractId,
                ContractTitle = request.ContractTitle,
                ServiceRequestStatusName = request.ServiceRequestStatusName,
                AmountOriginal = request.AmountOriginal,
                OriginalCurrencyCode = request.OriginalCurrencyCode,
                AmountZAR = request.AmountZAR,
                RequestedAt = request.RequestedAt
            };
        }

        private static ContractDeleteViewModel ToContractDeleteViewModel(ContractDetailDto contract)
        {
            return new ContractDeleteViewModel
            {
                ContractId = contract.ContractId,
                Title = contract.Title,
                ClientName = contract.ClientName,
                ContractStatusName = contract.ContractStatusName
            };
        }

    }
}

//.....................................o0oEND OF FILEo0o........................................//
