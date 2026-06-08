using GLMS.Api.Services;
using Microsoft.AspNetCore.Mvc;

//ST10445500 - PROG7311 - GLMS POE
//LookupsController

//.....................................o0oSTART OF FILEo0o........................................//

// The API controller receives HTTP requests and sends the work to services.

namespace GLMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LookupsController : ControllerBase
    {
        private readonly ILookupService _lookupService;

        public LookupsController(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        //..............................................................................//

        [HttpGet("contract-statuses")]
        public async Task<IActionResult> GetContractStatuses()
        {
            return Ok(await _lookupService.GetContractStatusLookupsAsync());
        }

        //..............................................................................//

        [HttpGet("service-request-statuses")]
        public async Task<IActionResult> GetServiceRequestStatuses()
        {
            return Ok(await _lookupService.GetServiceRequestStatusLookupsAsync());
        }

        //..............................................................................//

        [HttpGet("clients")]
        public async Task<IActionResult> GetClients()
        {
            return Ok(await _lookupService.GetClientLookupsAsync());
        }

        //..............................................................................//

        [HttpGet("contracts")]
        public async Task<IActionResult> GetContracts()
        {
            return Ok(await _lookupService.GetContractLookupsAsync());
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
