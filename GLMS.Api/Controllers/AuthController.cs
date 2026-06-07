using GLMS.Api.DTOs.Auth;
using GLMS.Api.Responses;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//ST10445500 - PROG7311 - GLMS POE
//AuthController

//.....................................o0oSTART OF FILEo0o........................................//

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

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return result.Succeeded
                ? CreatedAtAction(nameof(Me), new { }, result.Value)
                : BadRequest(new ApiErrorResponse(result.Errors));
        }

        //..............................................................................//

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var result = await _authService.GetCurrentUserAsync(User);
            return result.Succeeded ? Ok(result.Value) : Unauthorized(new ApiErrorResponse(result.Errors));
        }

        //..............................................................................//

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequestDto dto)
        {
            var result = await _authService.UpdateProfileAsync(User, dto);
            return ToActionResult(result);
        }

        //..............................................................................//

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto dto)
        {
            var result = await _authService.ChangePasswordAsync(User, dto);
            return ToActionResult(result);
        }

        //..............................................................................//

        private IActionResult ToActionResult<T>(AuthServiceResult<T> result)
        {
            if (result.Succeeded)
            {
                return Ok(result.Value);
            }

            return result.IsUnauthorized
                ? Unauthorized(new ApiErrorResponse(result.Errors))
                : BadRequest(new ApiErrorResponse(result.Errors));
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
