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
    [Authorize(Roles = ApplicationRoles.Admin)]
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
            var clients = await _clientService.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                clients = clients
                    .Where(c => c.CompanyName.Contains(search, StringComparison.OrdinalIgnoreCase)
                             || c.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return Ok(clients.OrderBy(c => c.CompanyName).ToList());
        }

        //..............................................................................//

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetClient(int id)
        {
            var client = await _clientService.GetWithContractsAsync(id);
            return client == null ? NotFound() : Ok(client);
        }

        //..............................................................................//

        [HttpPost]
        public async Task<IActionResult> Create(Client client)
        {
            try
            {
                var created = await _clientService.CreateAsync(client);
                return CreatedAtAction(nameof(GetClient), new { id = created.ClientId }, created);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(new { errors = new[] { ex.Message } });
            }
        }

        //..............................................................................//

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Client client)
        {
            if (id != client.ClientId)
            {
                return BadRequest(new { errors = new[] { "Client ID does not match." } });
            }

            try
            {
                await _clientService.UpdateAsync(client);
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
                await _clientService.DeleteAsync(id);
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
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
