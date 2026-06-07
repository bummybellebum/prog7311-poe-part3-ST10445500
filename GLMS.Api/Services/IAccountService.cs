using GLMS.Api.DTOs.Auth;
using GLMS.Api.Results;



namespace GLMS.Api.Services
{
    public interface IAccountService
    {
        Task<IReadOnlyList<AdminUserListDto>> GetUsersAsync();
        Task<AdminUserDetailDto?> GetUserForEditAsync(string userId);
        Task<AccountResult<AdminUserDetailDto>> CreateUserAsync(CreateAdminUserDto dto);
        Task<AccountResult> UpdateUserAsync(UpdateAdminUserDto dto);
        Task<AccountResult> SetUserActiveAsync(string userId, bool isActive);
        Task<AccountResult> ResetPasswordAsync(ResetAdminPasswordDto dto);
    }
}


