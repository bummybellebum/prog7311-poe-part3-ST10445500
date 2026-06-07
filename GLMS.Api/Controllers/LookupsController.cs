using GLMS.Api.DTOs;
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
            var statuses = await _lookupService.GetContractStatusesAsync();
            return Ok(statuses.Select(s => s.ToLookupDto()).ToList());
        }

        //..............................................................................//

        [HttpGet("service-request-statuses")]
        public async Task<IActionResult> GetServiceRequestStatuses()
        {
            var statuses = await _lookupService.GetServiceRequestStatusesAsync();
            return Ok(statuses.Select(s => s.ToLookupDto()).ToList());
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
