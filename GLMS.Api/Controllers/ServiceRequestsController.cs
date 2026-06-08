using GLMS.Api.DTOs.ServiceRequests;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestsController

//.....................................o0oSTART OF FILEo0o........................................//

// The API controller receives HTTP requests and sends the work to services.

namespace GLMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = ApplicationRoles.AllRoles)]
    [Route("api/service-requests")]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly IServiceRequestService _serviceRequestService;
        private readonly ICurrencyExchangeService _currencyExchangeService;

        public ServiceRequestsController(
            IServiceRequestService serviceRequestService,
            ICurrencyExchangeService currencyExchangeService)
        {
            _serviceRequestService = serviceRequestService;
            _currencyExchangeService = currencyExchangeService;
        }

        //..............................................................................//

        [HttpGet]
        public async Task<IActionResult> GetServiceRequests(int? contractId = null, int? statusId = null)
        {
            return Ok(await _serviceRequestService.GetServiceRequestsAsync(contractId, statusId));
        }

        //..............................................................................//

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetServiceRequest(int id)
        {
            var request = await _serviceRequestService.GetServiceRequestAsync(id);
            return request == null ? NotFound() : Ok(request);
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrLogisticsManager)]
        [HttpPost]
        public async Task<IActionResult> Create(CreateServiceRequestDto dto)
        {
            var created = await _serviceRequestService.CreateServiceRequestAsync(dto);
            return CreatedAtAction(nameof(GetServiceRequest), new { id = created.ServiceRequestId }, created);
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.Admin)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateServiceRequestDto dto)
        {
            return Ok(await _serviceRequestService.UpdateServiceRequestAsync(id, dto));
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrLogisticsManager)]
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateServiceRequestStatusDto dto)
        {
            return Ok(await _serviceRequestService.UpdateServiceRequestStatusAsync(id, dto));
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.Admin)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _serviceRequestService.DeleteServiceRequestAsync(id);
            return NoContent();
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
                throw new ArgumentException("Currency code is required.");
            }

            var rate = await _currencyExchangeService.GetRateToZarAsync(currencyCode, cancellationToken);
            return Ok(new { rate });
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
