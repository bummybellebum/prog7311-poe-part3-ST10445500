using GLMS.Api.DTOs.Auth;
using GLMS.Api.Models;
using GLMS.Api.Responses;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;



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
            return result.Succeeded
                ? CreatedAtAction(nameof(GetUser), new { id = result.Value!.UserId }, result.Value)
                : ToActionResult(result);
        }

        //..............................................................................//

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, UpdateAdminUserDto dto)
        {
            if (id != dto.UserId)
            {
                throw new ArgumentException("User ID does not match.");
            }

            var result = await _accountService.UpdateUserAsync(dto);
            if (!result.Succeeded)
            {
                return ToActionResult(result);
            }

            var user = await _accountService.GetUserForEditAsync(id);
            return user == null ? NotFound(new ApiErrorResponse("User not found.")) : Ok(user);
        }

        //..............................................................................//

        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(string id, ResetAdminPasswordDto dto)
        {
            if (id != dto.UserId)
            {
                throw new ArgumentException("User ID does not match.");
            }

            var result = await _accountService.ResetPasswordAsync(dto);
            return result.Succeeded ? Ok(new { }) : ToActionResult(result);
        }

        //..............................................................................//

        [HttpPatch("{id}/active")]
        public async Task<IActionResult> SetActive(string id, UpdateUserActiveDto dto)
        {
            var result = await _accountService.SetUserActiveAsync(id, dto.IsActive);
            if (!result.Succeeded)
            {
                return ToActionResult(result);
            }

            var user = await _accountService.GetUserForEditAsync(id);
            return user == null ? NotFound(new ApiErrorResponse("User not found.")) : Ok(user);
        }

        //..............................................................................//

        private IActionResult ToActionResult(AccountResult result)
        {
            return result.IsNotFound
                ? NotFound(new ApiErrorResponse(result.Errors))
                : BadRequest(new ApiErrorResponse(result.Errors));
        }

        //..............................................................................//
    }

}

