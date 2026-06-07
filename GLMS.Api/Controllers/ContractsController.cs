using System.Security.Claims;
using GLMS.Api.DTOs;
using GLMS.Api.DTOs.Contracts;
using GLMS.Api.DTOs.Documents;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//ST10445500 - PROG7311 - GLMS POE
//ContractsController

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IContractDocumentService _contractDocumentService;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public ContractsController(
            IContractService contractService,
            IContractDocumentService contractDocumentService,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _contractService = contractService;
            _contractDocumentService = contractDocumentService;
            _environment = environment;
            _configuration = configuration;
        }

        //..............................................................................//

        [HttpGet]
        public async Task<IActionResult> GetContracts(int? statusId = null, DateTime? startDate = null, DateTime? endDate = null, int? clientId = null)
        {
            var contracts = await _contractService.FilterAsync(statusId, startDate, endDate, clientId);
            return Ok(contracts.OrderByDescending(c => c.CreatedAt).Select(c => c.ToListDto()).ToList());
        }

        //..............................................................................//

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetContract(int id)
        {
            var contract = await _contractService.GetDetailsAsync(id);
            return contract == null ? NotFound() : Ok(contract.ToDetailDto());
        }

        //..............................................................................//

        [HttpPost]
        public async Task<IActionResult> Create(CreateContractDto dto)
        {
            try
            {
                var contract = dto.ToEntity(GetUserId());
                var created = await _contractService.CreateAsync(contract);
                var detail = await _contractService.GetDetailsAsync(created.ContractId);
                return CreatedAtAction(nameof(GetContract), new { id = created.ContractId }, (detail ?? created).ToDetailDto());
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
            {
                return BadRequest(new { errors = new[] { ex.Message } });
            }
        }

        //..............................................................................//

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateContractDto dto)
        {
            if (id != dto.ContractId)
            {
                return BadRequest(new { errors = new[] { "Contract ID does not match." } });
            }

            try
            {
                var contract = await _contractService.GetByIdAsync(id);
                if (contract == null)
                {
                    return NotFound();
                }

                dto.ApplyTo(contract);
                await _contractService.UpdateAsync(contract);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(new { errors = new[] { ex.Message } });
            }
        }

        //..............................................................................//

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateContractStatusDto dto)
        {
            var contract = await _contractService.GetByIdAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            contract.ContractStatusId = dto.ContractStatusId;
            await _contractService.UpdateAsync(contract);
            return Ok();
        }

        //..............................................................................//

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _contractService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(new { errors = new[] { ex.Message } });
            }
        }

        //..............................................................................//

        [HttpGet("{contractId:int}/documents")]
        public async Task<IActionResult> GetDocuments(int contractId)
        {
            var documents = await _contractDocumentService.GetByContractIdAsync(contractId);
            return Ok(documents.Select(d => d.ToDto()).ToList());
        }

        //..............................................................................//

        [HttpGet("documents/{documentId:int}")]
        public async Task<IActionResult> GetDocument(int documentId)
        {
            var document = await _contractDocumentService.GetByIdAsync(documentId);
            return document == null ? NotFound() : Ok(document.ToDto());
        }

        //..............................................................................//

        [HttpPost("{contractId:int}/documents")]
        public async Task<IActionResult> CreateDocument(int contractId, CreateContractDocumentDto dto)
        {
            if (contractId != dto.ContractId)
            {
                return BadRequest(new { errors = new[] { "Contract ID does not match." } });
            }

            try
            {
                var document = dto.ToEntity();
                var created = await _contractDocumentService.CreateAsync(document);
                return CreatedAtAction(nameof(GetDocument), new { documentId = created.ContractDocumentId }, created.ToDto());
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(new { errors = new[] { ex.Message } });
            }
        }

        //..............................................................................//

        [HttpPut("documents/{documentId:int}")]
        public async Task<IActionResult> UpdateDocument(int documentId, UpdateContractDocumentDto dto)
        {
            if (documentId != dto.ContractDocumentId)
            {
                return BadRequest(new { errors = new[] { "Document ID does not match." } });
            }

            try
            {
                var document = await _contractDocumentService.GetByIdAsync(documentId);
                if (document == null)
                {
                    return NotFound();
                }

                dto.ApplyTo(document);
                await _contractDocumentService.UpdateAsync(document);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(new { errors = new[] { ex.Message } });
            }
        }

        //..............................................................................//

        [HttpDelete("documents/{documentId:int}")]
        public async Task<IActionResult> DeleteDocument(int documentId)
        {
            try
            {
                await _contractDocumentService.DeleteAsync(documentId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(new { errors = new[] { ex.Message } });
            }
        }

        //..............................................................................//

        [HttpPost("{contractId:int}/signed-agreement")]
        public async Task<IActionResult> UploadSignedAgreement(int contractId, IFormFile file)
        {
            var contract = await _contractService.GetByIdAsync(contractId);
            if (contract == null)
            {
                return NotFound();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { errors = new[] { "Please select a PDF file to upload." } });
            }

            if (!IsValidSignedAgreementPdf(file))
            {
                return BadRequest(new { errors = new[] { "Only PDF files are allowed for signed agreements." } });
            }

            var uploadFolder = GetUploadFolder();
            Directory.CreateDirectory(uploadFolder);

            var storedFileName = $"contract-{contractId}-{Guid.NewGuid():N}.pdf";
            var fullPath = Path.Combine(uploadFolder, storedFileName);

            await using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            var existingDocuments = await _contractDocumentService.GetByContractIdAsync(contractId);
            foreach (var existing in existingDocuments.Where(d => d.IsCurrent))
            {
                existing.IsCurrent = false;
                await _contractDocumentService.UpdateAsync(existing);
            }

            var relativeFolder = _configuration["Uploads:SignedAgreementFolder"] ?? "uploads/signed-agreements";
            var document = new ContractDocument
            {
                ContractId = contractId,
                DocumentType = "Signed Agreement",
                OriginalFileName = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                FilePath = Path.Combine(relativeFolder, storedFileName).Replace("\\", "/"),
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                UploadedByUserId = GetUserId(),
                IsCurrent = true
            };

            var created = await _contractDocumentService.CreateAsync(document);
            return CreatedAtAction(nameof(GetDocument), new { documentId = created.ContractDocumentId }, created.ToDto());
        }

        //..............................................................................//

        [HttpGet("documents/{documentId:int}/download")]
        public async Task<IActionResult> DownloadAgreement(int documentId)
        {
            var document = await _contractDocumentService.GetByIdAsync(documentId);
            if (document == null)
            {
                return NotFound();
            }

            var fullPath = Path.Combine(_environment.ContentRootPath, document.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound(new { errors = new[] { "The agreement file could not be found on the server." } });
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(bytes, document.ContentType ?? "application/pdf", document.OriginalFileName);
        }

        //..............................................................................//

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("Unable to identify the signed-in user.");
        }

        private string GetUploadFolder()
        {
            var relativeFolder = _configuration["Uploads:SignedAgreementFolder"] ?? "uploads/signed-agreements";
            return Path.Combine(_environment.ContentRootPath, relativeFolder.Replace("/", Path.DirectorySeparatorChar.ToString()));
        }

        private static bool IsValidSignedAgreementPdf(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            var contentType = file.ContentType ?? string.Empty;
            var isAllowedContentType = string.IsNullOrWhiteSpace(contentType)
                || string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
                || string.Equals(contentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase);

            return string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase) && isAllowedContentType;
        }

        //..............................................................................//
    }

}

//.....................................o0oEND OF FILEo0o..........................................//
