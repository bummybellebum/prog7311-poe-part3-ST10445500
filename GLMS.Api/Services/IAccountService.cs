using System.Security.Claims;
using GLMS.Api.DTOs.Auth;

//ST10445500 - PROG7311 - GLMS POE
//IAccountService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Services
{
    public interface IAccountService
    {
        Task<LoginResult> LoginAsync(LoginRequestDto dto);
        Task LogoutAsync();
        Task<UpdateProfileRequestDto?> GetProfileAsync(ClaimsPrincipal user);
        Task<AccountResult> UpdateProfileAsync(ClaimsPrincipal user, UpdateProfileRequestDto dto);
        Task<AccountResult> ChangePasswordAsync(ClaimsPrincipal user, ChangePasswordRequestDto dto);
        Task<IReadOnlyList<AdminUserListDto>> GetUsersAsync();
        Task<AdminUserDetailDto?> GetUserForEditAsync(string userId);
        Task<AccountResult> CreateUserAsync(CreateAdminUserDto dto);
        Task<AccountResult> UpdateUserAsync(UpdateAdminUserDto dto);
        Task<AccountResult> SetUserActiveAsync(string userId, bool isActive);
        Task<AccountResult> ResetPasswordAsync(ResetAdminPasswordDto dto);
    }
}

//.....................................o0oEND OF FILEo0o........................................//

