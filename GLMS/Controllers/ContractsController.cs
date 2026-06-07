using GLMS.Models;
using GLMS.Services;
using GLMS.ViewModels.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

//ST10445500 - PROG7311 - GLMS POE
//ContractsController

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ContractsController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IClientService _clientService;
        private readonly ILookupService _lookupService;
        private readonly IContractDocumentService _contractDocumentService;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContractsController(
            IContractService contractService,
            IClientService clientService,
            ILookupService lookupService,
            IContractDocumentService contractDocumentService,
            IWebHostEnvironment environment,
            UserManager<ApplicationUser> userManager)
        {
            _contractService = contractService;
            _clientService = clientService;
            _lookupService = lookupService;
            _contractDocumentService = contractDocumentService;
            _environment = environment;
            _userManager = userManager;
        }

        //........................................................................................//

        public async Task<IActionResult> Index(ContractFilterViewModel filter)
        {
            var contracts = await _contractService.FilterAsync(filter.StatusId, filter.StartDate, filter.EndDate, filter.ClientId);

            filter.Contracts = contracts.OrderByDescending(c => c.CreatedAt).ToList();
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

            var documents = await _contractDocumentService.GetByContractIdAsync(id);
            var current = documents.FirstOrDefault(d => d.IsCurrent);

            var vm = new ContractDetailsViewModel
            {
                Contract = contract,
                Documents = documents,
                CurrentSignedAgreement = current,
                ServiceRequests = contract.ServiceRequests.OrderByDescending(sr => sr.RequestedAt).ToList()
            };

            ViewData["UploadModel"] = new ContractDocumentUploadViewModel { ContractId = id };
            return View(vm);
        }

        //........................................................................................//

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
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Forbid();
                }

                var contract = new Contract
                {
                    ClientId = vm.ClientId,
                    Title = vm.Title,
                    StartDate = vm.StartDate,
                    EndDate = vm.EndDate,
                    ContractStatusId = vm.ContractStatusId,
                    ServiceLevel = vm.ServiceLevel,
                    Notes = vm.Notes,
                    CreatedByUserId = userId
                };

                await _contractService.CreateAsync(contract);

                if (vm.SignedAgreementFile != null && vm.SignedAgreementFile.Length > 0)
                {
                    await SaveSignedAgreementAsync(contract.ContractId, vm.SignedAgreementFile);
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
                var existing = await _contractService.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.ClientId = vm.ClientId;
                existing.Title = vm.Title;
                existing.StartDate = vm.StartDate;
                existing.EndDate = vm.EndDate;
                existing.ContractStatusId = vm.ContractStatusId;
                existing.ServiceLevel = vm.ServiceLevel;
                existing.Notes = vm.Notes;

                await _contractService.UpdateAsync(existing);
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

        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _contractService.GetDetailsAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        //........................................................................................//

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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
                await SaveSignedAgreementAsync(vm.ContractId, vm.File);
                TempData["SuccessMessage"] = "Signed agreement uploaded successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = vm.ContractId });
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

            var fullPath = Path.Combine(_environment.WebRootPath, document.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (!System.IO.File.Exists(fullPath))
            {
                TempData["ErrorMessage"] = "The agreement file could not be found on the server.";
                return RedirectToAction(nameof(Details), new { id = document.ContractId });
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(bytes, document.ContentType ?? "application/pdf", document.OriginalFileName);
        }

        //........................................................................................//

        private async Task PopulateFilterOptionsAsync(ContractFilterViewModel vm)
        {
            var statuses = await _lookupService.GetContractStatusesAsync();
            var clients = await _clientService.GetAllAsync();

            vm.StatusOptions = statuses
                .OrderBy(s => s.StatusName)
                .Select(s => new SelectListItem(s.StatusName, s.ContractStatusId.ToString()))
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
                .OrderBy(s => s.StatusName)
                .Select(s => new SelectListItem(s.StatusName, s.ContractStatusId.ToString()))
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

        private async Task SaveSignedAgreementAsync(int contractId, IFormFile file)
        {
            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "signed-agreements");
            Directory.CreateDirectory(uploadFolder);

            var storedFileName = $"contract-{contractId}-{Guid.NewGuid():N}.pdf";
            var fullPath = Path.Combine(uploadFolder, storedFileName);

            await using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new InvalidOperationException("Unable to identify the signed-in user for document upload.");
            }

            var existingDocuments = await _contractDocumentService.GetByContractIdAsync(contractId);
            foreach (var existing in existingDocuments.Where(d => d.IsCurrent))
            {
                existing.IsCurrent = false;
                await _contractDocumentService.UpdateAsync(existing);
            }

            var document = new ContractDocument
            {
                ContractId = contractId,
                DocumentType = "Signed Agreement",
                OriginalFileName = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                FilePath = Path.Combine("uploads", "signed-agreements", storedFileName).Replace("\\", "/"),
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                UploadedByUserId = userId,
                IsCurrent = true
            };

            await _contractDocumentService.CreateAsync(document);
        }

        //........................................................................................//
    }
}

//.....................................o0oEND OF FILEo0o........................................//
