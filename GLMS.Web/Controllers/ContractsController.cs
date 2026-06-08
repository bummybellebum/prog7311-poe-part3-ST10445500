using GLMS.Web.ApiClients;
using GLMS.Web.ApiClients.Models;
using GLMS.Web.Authorization;
using GLMS.Web.ViewModels.Mappings;
using GLMS.Web.ViewModels.Contracts;
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
        private readonly IContractsApiClient _contractsApiClient;
        private readonly IClientsApiClient _clientsApiClient;
        private readonly ILookupsApiClient _lookupsApiClient;

        public ContractsController(
            IContractsApiClient contractsApiClient,
            IClientsApiClient clientsApiClient,
            ILookupsApiClient lookupsApiClient)
        {
            _contractsApiClient = contractsApiClient;
            _clientsApiClient = clientsApiClient;
            _lookupsApiClient = lookupsApiClient;
        }

        //........................................................................................//

        public async Task<IActionResult> Index(ContractFilterViewModel filter)
        {
            var result = await _contractsApiClient.FilterAsync(filter.StatusId, filter.StartDate, filter.EndDate, filter.ClientId);
            if (!result.IsSuccess)
            {
                ViewData["ErrorMessage"] = result.ErrorMessage;
                filter.Contracts = [];
                await PopulateFilterOptionsAsync(filter);
                return View(filter);
            }

            filter.Contracts = result.Data!
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => c.ToListViewModel())
                .ToList();
            await PopulateFilterOptionsAsync(filter);

            return View(filter);
        }

        //........................................................................................//

        public async Task<IActionResult> Details(int id)
        {
            var contractResult = await _contractsApiClient.GetDetailsAsync(id);
            if (!contractResult.IsSuccess || contractResult.Data == null)
            {
                return NotFound();
            }

            var contract = contractResult.Data;
            var documents = contract.Documents.Any()
                ? contract.Documents
                : (await _contractsApiClient.GetDocumentsByContractIdAsync(id)).Data ?? [];
            var current = documents.FirstOrDefault(d => d.IsCurrent);

            var vm = new ContractDetailsViewModel
            {
                Contract = contract.ToDetailViewModel(),
                Documents = documents.Select(d => d.ToItemViewModel()).ToList(),
                CurrentSignedAgreement = current?.ToItemViewModel(),
                ServiceRequests = contract.ServiceRequests
                    .OrderByDescending(sr => sr.RequestedAt)
                    .Select(sr => sr.ToListViewModel())
                    .ToList()
            };

            var statuses = await _lookupsApiClient.GetContractStatusesAsync();
            vm.StatusOptions = (statuses.Data ?? [])
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

            var result = await _contractsApiClient.CreateAsync(contract);
            if (result.IsSuccess && result.Data != null)
            {
                if (vm.SignedAgreementFile != null && vm.SignedAgreementFile.Length > 0)
                {
                    var uploadResult = await _contractsApiClient.UploadSignedAgreementAsync(result.Data.ContractId, vm.SignedAgreementFile);
                    if (!uploadResult.IsSuccess)
                    {
                        ModelState.AddModelError(string.Empty, uploadResult.ErrorMessage ?? "The signed agreement could not be uploaded.");
                        await PopulateFormOptionsAsync(vm);
                        return View(vm);
                    }

                    TempData["SuccessMessage"] = "Contract created successfully and signed agreement uploaded.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Contract created successfully.";
                }

                return RedirectToAction(nameof(Index));
            }

            AddError(result);
            await PopulateFormOptionsAsync(vm);
            return View(vm);
        }

        //........................................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _contractsApiClient.GetByIdAsync(id);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            var contract = result.Data;
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

            var existingResult = await _contractsApiClient.GetDetailsAsync(id);
            if (!existingResult.IsSuccess || existingResult.Data == null)
            {
                return NotFound();
            }

            var existing = existingResult.Data;
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

            var result = await _contractsApiClient.UpdateAsync(dto);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Contract updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            AddError(result);
            await PopulateFormOptionsAsync(vm);
            return View(vm);
        }

        //........................................................................................//

        [Authorize(Roles = ApplicationRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _contractsApiClient.GetDetailsAsync(id);
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
            var result = await _contractsApiClient.DeleteAsync(id);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Contract deleted successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        //........................................................................................//

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        public async Task<IActionResult> UploadSignedAgreement(ContractDocumentUploadViewModel vm)
        {
            var contract = await _contractsApiClient.GetByIdAsync(vm.ContractId);
            if (!contract.IsSuccess || contract.Data == null)
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

            var result = await _contractsApiClient.UploadSignedAgreementAsync(vm.ContractId, vm.File);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Signed agreement uploaded successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Details), new { id = vm.ContractId });
        }

        //........................................................................................//

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        public async Task<IActionResult> UpdateStatus(int id, int contractStatusId)
        {
            var result = await _contractsApiClient.UpdateStatusAsync(id, contractStatusId);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Contract status updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        //........................................................................................//

        [HttpGet]
        public async Task<IActionResult> DownloadAgreement(int id)
        {
            var document = await _contractsApiClient.GetDocumentByIdAsync(id);
            if (!document.IsSuccess || document.Data == null)
            {
                return NotFound();
            }

            var file = await _contractsApiClient.DownloadAgreementAsync(id);
            if (!file.IsSuccess || file.Data == null)
            {
                TempData["ErrorMessage"] = file.ErrorMessage ?? "The agreement file could not be found on the server.";
                return RedirectToAction(nameof(Details), new { id = document.Data.ContractId });
            }

            return File(file.Data.Bytes, file.Data.ContentType, file.Data.FileName);
        }

        //........................................................................................//

        private async Task PopulateFilterOptionsAsync(ContractFilterViewModel vm)
        {
            var statuses = await _lookupsApiClient.GetContractStatusesAsync();
            var clients = await _clientsApiClient.GetAllAsync();

            vm.StatusOptions = (statuses.Data ?? [])
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
                .ToList();

            vm.ClientOptions = (clients.Data ?? [])
                .OrderBy(c => c.CompanyName)
                .Select(c => new SelectListItem(c.CompanyName, c.ClientId.ToString()))
                .ToList();
        }

        //........................................................................................//

        private async Task PopulateFormOptionsAsync(ContractFormViewModel vm)
        {
            var statuses = await _lookupsApiClient.GetContractStatusesAsync();
            var clients = await _clientsApiClient.GetAllAsync();

            vm.StatusOptions = (statuses.Data ?? [])
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
                .ToList();

            vm.ClientOptions = (clients.Data ?? [])
                .Where(c => c.IsActive)
                .OrderBy(c => c.CompanyName)
                .Select(c => new SelectListItem(c.CompanyName, c.ClientId.ToString()))
                .ToList();
        }

        //........................................................................................//

        private void AddError(ApiClientResult result)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "The request could not be completed.");
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

    }
}

//.....................................o0oEND OF FILEo0o........................................//
