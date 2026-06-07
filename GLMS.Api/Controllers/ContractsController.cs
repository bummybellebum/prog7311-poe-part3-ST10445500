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
    [Authorize(Roles = ApplicationRoles.AllRoles)]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IContractDocumentService _contractDocumentService;
        private readonly ICurrentUserService _currentUserService;

        public ContractsController(
            IContractService contractService,
            IContractDocumentService contractDocumentService,
            ICurrentUserService currentUserService)
        {
            _contractService = contractService;
            _contractDocumentService = contractDocumentService;
            _currentUserService = currentUserService;
        }

        //..............................................................................//

        [HttpGet]
        public async Task<IActionResult> GetContracts(int? statusId = null, DateTime? startDate = null, DateTime? endDate = null, int? clientId = null)
        {
            var contracts = await _contractService.FilterAsync(statusId, startDate, endDate, clientId);
            return Ok(contracts.Select(c => c.ToListDto()).ToList());
        }

        //..............................................................................//

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetContract(int id)
        {
            var contract = await _contractService.GetDetailsAsync(id);
            return contract == null ? NotFound() : Ok(contract.ToDetailDto());
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
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

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
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

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateContractStatusDto dto)
        {
            try
            {
                await _contractService.UpdateStatusAsync(id, dto.ContractStatusId);
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

        [Authorize(Roles = ApplicationRoles.Admin)]
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

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
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

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
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

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
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

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        [HttpPost("{contractId:int}/signed-agreement")]
        public async Task<IActionResult> UploadSignedAgreement(int contractId, IFormFile file)
        {
            try
            {
                var created = await _contractDocumentService.UploadSignedAgreementAsync(contractId, file, GetUserId());
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

        [HttpGet("documents/{documentId:int}/download")]
        public async Task<IActionResult> DownloadAgreement(int documentId)
        {
            var file = await _contractDocumentService.GetSignedAgreementDownloadAsync(documentId);
            if (file == null)
            {
                return NotFound(new { errors = new[] { "The agreement file could not be found on the server." } });
            }

            return PhysicalFile(file.PhysicalPath, file.ContentType, file.FileName);
        }

        //..............................................................................//

        private string GetUserId()
        {
            return _currentUserService.UserId
                ?? throw new InvalidOperationException("Unable to identify the signed-in user.");
        }

        //..............................................................................//
    }

}

//.....................................o0oEND OF FILEo0o..........................................//
