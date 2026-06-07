using GLMS.Api.DTOs;
using GLMS.Api.DTOs.ServiceRequests;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestsController

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = ApplicationRoles.AllRoles)]
    [Route("api/[controller]")]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly IServiceRequestService _serviceRequestService;
        private readonly ICurrencyExchangeService _currencyExchangeService;
        private readonly ICurrentUserService _currentUserService;

        public ServiceRequestsController(
            IServiceRequestService serviceRequestService,
            ICurrencyExchangeService currencyExchangeService,
            ICurrentUserService currentUserService)
        {
            _serviceRequestService = serviceRequestService;
            _currencyExchangeService = currencyExchangeService;
            _currentUserService = currentUserService;
        }

        //..............................................................................//

        [HttpGet]
        public async Task<IActionResult> GetServiceRequests(int? contractId = null, int? statusId = null)
        {
            var requests = await _serviceRequestService.GetAllAsync(contractId, statusId);
            return Ok(requests.Select(r => r.ToListDto()).ToList());
        }

        //..............................................................................//

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetServiceRequest(int id)
        {
            var request = await _serviceRequestService.GetDetailsAsync(id);
            return request == null ? NotFound() : Ok(request.ToDetailDto());
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrLogisticsManager)]
        [HttpPost]
        public async Task<IActionResult> Create(CreateServiceRequestDto dto)
        {
            try
            {
                var serviceRequest = dto.ToEntity(GetUserId());
                var created = await _serviceRequestService.CreateAsync(serviceRequest);
                var detail = await _serviceRequestService.GetDetailsAsync(created.ServiceRequestId);
                return CreatedAtAction(nameof(GetServiceRequest), new { id = created.ServiceRequestId }, (detail ?? created).ToDetailDto());
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
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateServiceRequestDto dto)
        {
            if (id != dto.ServiceRequestId)
            {
                return BadRequest(new { errors = new[] { "Service request ID does not match." } });
            }

            try
            {
                var serviceRequest = await _serviceRequestService.GetByIdAsync(id);
                if (serviceRequest == null)
                {
                    return NotFound();
                }

                dto.ApplyTo(serviceRequest);
                await _serviceRequestService.UpdateAsync(serviceRequest);
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

        [Authorize(Roles = ApplicationRoles.AdminOrLogisticsManager)]
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateServiceRequestStatusDto dto)
        {
            try
            {
                await _serviceRequestService.UpdateStatusAsync(id, dto.ServiceRequestStatusId);
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
                await _serviceRequestService.DeleteAsync(id);
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

        [Authorize(Roles = ApplicationRoles.AdminOrLogisticsManager)]
        [HttpGet("currencies")]
        public async Task<IActionResult> GetCurrencies(CancellationToken cancellationToken)
        {
            return Ok(await _currencyExchangeService.GetSupportedCurrenciesAsync(cancellationToken));
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrLogisticsManager)]
        [HttpGet("exchange-rate")]
        public async Task<IActionResult> GetExchangeRate(string currencyCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                return BadRequest(new { errors = new[] { "Currency code is required." } });
            }

            try
            {
                var rate = await _currencyExchangeService.GetRateToZarAsync(currencyCode, cancellationToken);
                return Ok(new { rate });
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is HttpRequestException || ex is TaskCanceledException)
            {
                return BadRequest(new { errors = new[] { ex.Message } });
            }
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
