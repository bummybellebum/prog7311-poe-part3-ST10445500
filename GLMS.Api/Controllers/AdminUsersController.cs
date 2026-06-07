using GLMS.Api.DTOs.Auth;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//ST10445500 - PROG7311 - GLMS POE
//AdminUsersController

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [Route("api/admin/users")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AdminUsersController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        //..............................................................................//

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(await _accountService.GetUsersAsync());
        }

        //..............................................................................//

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _accountService.GetUserForEditAsync(id);
            return user == null ? NotFound() : Ok(user);
        }

        //..............................................................................//

        [HttpPost]
        public async Task<IActionResult> Create(CreateAdminUserDto dto)
        {
            var result = await _accountService.CreateUserAsync(dto);
            return result.Succeeded ? Created(string.Empty, null) : BadRequest(new { errors = result.Errors });
        }

        //..............................................................................//

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, UpdateAdminUserDto dto)
        {
            if (id != dto.UserId)
            {
                return BadRequest(new { errors = new[] { "User ID does not match." } });
            }

            var result = await _accountService.UpdateUserAsync(dto);
            return result.Succeeded ? Ok() : BadRequest(new { errors = result.Errors });
        }

        //..............................................................................//

        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(string id, ResetAdminPasswordDto dto)
        {
            if (id != dto.UserId)
            {
                return BadRequest(new { errors = new[] { "User ID does not match." } });
            }

            var result = await _accountService.ResetPasswordAsync(dto);
            return result.Succeeded ? Ok() : BadRequest(new { errors = result.Errors });
        }

        //..............................................................................//

        [HttpPatch("{id}/active")]
        public async Task<IActionResult> SetActive(string id, UpdateUserActiveDto dto)
        {
            var result = await _accountService.SetUserActiveAsync(id, dto.IsActive);
            return result.Succeeded ? Ok() : BadRequest(new { errors = result.Errors });
        }

        //..............................................................................//
    }

}

//.....................................o0oEND OF FILEo0o..........................................//
