using GLMS.Api.ApiHelpers;
using GLMS.Api.DTOs.Auth;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;



namespace GLMS.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		//..............................................................................//

		[AllowAnonymous]
		[HttpPost("login")]
		public async Task<IActionResult> Login(LoginRequestDto dto)
		{
			var result = await _authService.LoginAsync(dto);
			return result.Succeeded ? Ok(result.Value) : Unauthorized(new ApiErrorResponse(result.Errors));
		}

		//..............................................................................//
	}
}

