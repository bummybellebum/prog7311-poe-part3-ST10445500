using GLMS.Api.DTOs.Clients;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//ST10445500 - PROG7311 - GLMS POE
//ClientsController

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = ApplicationRoles.AllRoles)]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        //..............................................................................//

        [HttpGet]
        public async Task<IActionResult> GetClients(string? search = null)
        {
            return Ok(await _clientService.GetListAsync(search));
        }

        //..............................................................................//

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetClient(int id)
        {
            var client = await _clientService.GetDetailDtoAsync(id);
            return client == null ? NotFound() : Ok(client);
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        [HttpPost]
        public async Task<IActionResult> Create(CreateClientDto dto)
        {
            var created = await _clientService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetClient), new { id = created.ClientId }, created);
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.AdminOrContractManager)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateClientDto dto)
        {
            return Ok(await _clientService.UpdateAsync(id, dto));
        }

        //..............................................................................//

        [Authorize(Roles = ApplicationRoles.Admin)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _clientService.DeleteAsync(id);
            return NoContent();
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
