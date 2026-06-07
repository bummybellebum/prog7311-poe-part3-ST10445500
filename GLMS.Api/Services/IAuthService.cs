using System.Security.Claims;
using GLMS.Api.DTOs.Auth;
using GLMS.Api.Results;



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

