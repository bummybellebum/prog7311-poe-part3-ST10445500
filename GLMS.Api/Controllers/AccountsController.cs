using GLMS.Api.ApiHelpers;
using GLMS.Api.DTOs.Auth;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//ST10445500 - PROG7311 - GLMS POE
//AccountsController

//.....................................o0oSTART OF FILEo0o........................................//

// The API controller receives HTTP requests and sends the work to services.

namespace GLMS.Api.Controllers
{
	[ApiController]
	[Authorize(Roles = ApplicationRoles.AllRoles)]
	[Route("api/accounts")]
	public class AccountsController : ControllerBase
	{
		private readonly IAccountService _accountService;

		public AccountsController(IAccountService accountService)
		{
			_accountService = accountService;
		}

		//..............................................................................//

		[Authorize(Roles = ApplicationRoles.Admin)]
		[HttpGet]
		public async Task<IActionResult> GetUsers()
		{
			return Ok(await _accountService.GetUsersAsync());
		}

		//..............................................................................//

		[HttpGet("me")]
		public async Task<IActionResult> Me()
		{
			var result = await _accountService.GetCurrentAccountAsync();
			return ToActionResult(result);
		}

		//..............................................................................//

		[HttpPut("me")]
		public async Task<IActionResult> UpdateCurrentAccount(UpdateAccountProfileDto dto)
		{
			var result = await _accountService.UpdateCurrentAccountAsync(dto);
			return ToActionResult(result);
		}

		//..............................................................................//

		[HttpPost("me/change-password")]
		public async Task<IActionResult> ChangeCurrentPassword(ChangePasswordRequestDto dto)
		{
			var result = await _accountService.ChangeCurrentPasswordAsync(dto);
			return ToActionResult(result);
		}

		//..............................................................................//

		[Authorize(Roles = ApplicationRoles.Admin)]
		[HttpGet("{id}")]
		public async Task<IActionResult> GetUser(string id)
		{
			var user = await _accountService.GetUserAsync(id);
			return user == null ? NotFound() : Ok(user);
		}

		//..............................................................................//

		[Authorize(Roles = ApplicationRoles.Admin)]
		[HttpPost]
		public async Task<IActionResult> Create(CreateUserDto dto)
		{
			var result = await _accountService.CreateUserAsync(dto);
			return result.Succeeded
				? CreatedAtAction(nameof(GetUser), new { id = result.Value!.UserId }, result.Value)
				: ToActionResult(result);
		}

		//..............................................................................//

		[Authorize(Roles = ApplicationRoles.Admin)]
		[HttpPut("{id}")]
		public async Task<IActionResult> Update(string id, UpdateUserDto dto)
		{
			var result = await _accountService.UpdateUserAsync(id, dto);
			if (!result.Succeeded)
			{
				return ToActionResult(result);
			}

			var user = await _accountService.GetUserAsync(id);
			return user == null ? NotFound(new ApiErrorResponse("User not found.")) : Ok(user);
		}

		//..............................................................................//

		[Authorize(Roles = ApplicationRoles.Admin)]
		[HttpPost("{id}/reset-password")]
		public async Task<IActionResult> ResetPassword(string id, ResetUserPasswordDto dto)
		{
			if (id != dto.UserId)
			{
				throw new ArgumentException("User ID does not match.");
			}

			var result = await _accountService.ResetUserPasswordAsync(dto);
			return result.Succeeded ? Ok(new { }) : ToActionResult(result);
		}

		//..............................................................................//

		[Authorize(Roles = ApplicationRoles.Admin)]
		[HttpPatch("{id}/active")]
		public async Task<IActionResult> SetActive(string id, UpdateUserActiveDto dto)
		{
			var result = await _accountService.SetUserActiveAsync(id, dto.IsActive);
			if (!result.Succeeded)
			{
				return ToActionResult(result);
			}

			var user = await _accountService.GetUserAsync(id);
			return user == null ? NotFound(new ApiErrorResponse("User not found.")) : Ok(user);
		}

		//..............................................................................//

		private IActionResult ToActionResult(AccountResult result)
		{
			if (result.Succeeded)
			{
				return Ok();
			}

			if (result.IsUnauthorized)
			{
				return Unauthorized(new ApiErrorResponse(result.Errors));
			}

			return result.IsNotFound
				? NotFound(new ApiErrorResponse(result.Errors))
				: BadRequest(new ApiErrorResponse(result.Errors));
		}

		//..............................................................................//

		private IActionResult ToActionResult<T>(AccountResult<T> result)
		{
			if (result.Succeeded)
			{
				return Ok(result.Value);
			}

			if (result.IsUnauthorized)
			{
				return Unauthorized(new ApiErrorResponse(result.Errors));
			}

			return result.IsNotFound
				? NotFound(new ApiErrorResponse(result.Errors))
				: BadRequest(new ApiErrorResponse(result.Errors));
		}

		//..............................................................................//
	}

}

//.....................................o0oEND OF FILEo0o..........................................//
