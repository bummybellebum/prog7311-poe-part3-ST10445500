using GLMS.Api.DTOs.Contracts;
using GLMS.Api.DTOs.Documents;
using GLMS.Api.Models;
using GLMS.Api.Responses;
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

        public ContractsController(
            IContractService contractService,
            IContractDocumentService contractDocumentService)
        {
            _contractService = contractService;
            _contractDocumentService = contractDocumentService;
        }

        //..............................................................................//

        [HttpGet]
        public async Task<IActionResult> GetContracts(int? statusId = null, DateTime? startDate = null, DateTime? endDate = null, int? clientId = null)
        {
            return Ok(await _contractService.FilterDtosAsync(statusId, startDate, endDate, clientId));
        }

        //..............................................................................//

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetContract(int id)
        {
            var contract = await _contractService.GetDetailDtoAsync(id);
            return contract == null ? NotFound() : Ok(contract);
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        [HttpPost]
        public async Task<IActionResult> Create(CreateContractDto dto)
        {
            var created = await _contractService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetContract), new { id = created.ContractId }, created);
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateContractDto dto)
        {
            return Ok(await _contractService.UpdateAsync(id, dto));
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateContractStatusDto dto)
        {
            return Ok(await _contractService.UpdateStatusAsync(id, dto));
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.Admin)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _contractService.DeleteAsync(id);
            return NoContent();
        }

        //..............................................................................//

        [HttpGet("{contractId:int}/documents")]
        public async Task<IActionResult> GetDocuments(int contractId)
        {
            return Ok(await _contractDocumentService.GetDtosByContractIdAsync(contractId));
        }

        //..............................................................................//

        [HttpGet("documents/{documentId:int}")]
        public async Task<IActionResult> GetDocument(int documentId)
        {
            var document = await _contractDocumentService.GetDtoByIdAsync(documentId);
            return document == null ? NotFound() : Ok(document);
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        [HttpPost("{contractId:int}/documents")]
        public async Task<IActionResult> CreateDocument(int contractId, CreateContractDocumentDto dto)
        {
            var created = await _contractDocumentService.CreateAsync(contractId, dto);
            return CreatedAtAction(nameof(GetDocument), new { documentId = created.ContractDocumentId }, created);
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        [HttpPut("documents/{documentId:int}")]
        public async Task<IActionResult> UpdateDocument(int documentId, UpdateContractDocumentDto dto)
        {
            return Ok(await _contractDocumentService.UpdateAsync(documentId, dto));
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        [HttpDelete("documents/{documentId:int}")]
        public async Task<IActionResult> DeleteDocument(int documentId)
        {
            await _contractDocumentService.DeleteAsync(documentId);
            return NoContent();
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        [HttpPost("{contractId:int}/signed-agreement")]
        public async Task<IActionResult> UploadSignedAgreement(int contractId, IFormFile file)
        {
            var created = await _contractDocumentService.UploadSignedAgreementDtoAsync(contractId, file);
            return CreatedAtAction(nameof(GetDocument), new { documentId = created.ContractDocumentId }, created);
        }

        //..............................................................................//

        [HttpGet("documents/{documentId:int}/download")]
        public async Task<IActionResult> DownloadAgreement(int documentId)
        {
            var file = await _contractDocumentService.GetSignedAgreementDownloadAsync(documentId);
            if (file == null)
            {
                return NotFound(new ApiErrorResponse("The agreement file could not be found on the server."));
            }

            return PhysicalFile(file.PhysicalPath, file.ContentType, file.FileName);
        }

        //..............................................................................//
    }

}

//.....................................o0oEND OF FILEo0o..........................................//
