using System.Security.Claims;
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
    [Authorize(Roles = "Admin,LogisticsManager")]
    [Route("api/[controller]")]
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
            var requests = await _serviceRequestService.GetAllAsync();

            if (contractId.HasValue)
            {
                requests = requests.Where(r => r.ContractId == contractId.Value).ToList();
            }

            if (statusId.HasValue)
            {
                requests = requests.Where(r => r.ServiceRequestStatusId == statusId.Value).ToList();
            }

            return Ok(requests.OrderByDescending(r => r.RequestedAt).ToList());
        }

        //..............................................................................//

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetServiceRequest(int id)
        {
            var request = await _serviceRequestService.GetDetailsAsync(id);
            return request == null ? NotFound() : Ok(request);
        }

        //..............................................................................//

        [HttpPost]
        public async Task<IActionResult> Create(ServiceRequest serviceRequest)
        {
            try
            {
                serviceRequest.RequestedByUserId = GetUserId();
                var created = await _serviceRequestService.CreateAsync(serviceRequest);
                return CreatedAtAction(nameof(GetServiceRequest), new { id = created.ServiceRequestId }, created);
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

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.ServiceRequestId)
            {
                return BadRequest(new { errors = new[] { "Service request ID does not match." } });
            }

            try
            {
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

        [HttpGet("currencies")]
        public async Task<IActionResult> GetCurrencies(CancellationToken cancellationToken)
        {
            return Ok(await _currencyExchangeService.GetSupportedCurrenciesAsync(cancellationToken));
        }

        //..............................................................................//

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
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("Unable to identify the signed-in user.");
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
