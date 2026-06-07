using GLMS.Api.Services;
using Microsoft.AspNetCore.Mvc;

//ST10445500 - PROG7311 - GLMS POE
//LookupsController

//.....................................o0oSTART OF FILEo0o........................................//

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
            return Ok(await _lookupService.GetContractStatusesAsync());
        }

        //..............................................................................//

        [HttpGet("service-request-statuses")]
        public async Task<IActionResult> GetServiceRequestStatuses()
        {
            return Ok(await _lookupService.GetServiceRequestStatusesAsync());
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
