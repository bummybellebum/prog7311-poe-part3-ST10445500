using System.Security.Claims;
using GLMS.Api.DTOs.Auth;

//ST10445500 - PROG7311 - GLMS POE
//IAuthService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Services
{
    public interface IAuthService
    {
        Task<AuthServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto dto);
        Task<AuthServiceResult<RegisterResponseDto>> RegisterAsync(RegisterRequestDto dto);
        Task<AuthServiceResult<AuthResponseDto>> GetCurrentUserAsync(ClaimsPrincipal principal);
        Task<AuthServiceResult<AuthResponseDto>> UpdateProfileAsync(ClaimsPrincipal principal, UpdateProfileRequestDto dto);
        Task<AuthServiceResult<object>> ChangePasswordAsync(ClaimsPrincipal principal, ChangePasswordRequestDto dto);
    }
}

//.....................................o0oEND OF FILEo0o........................................//
