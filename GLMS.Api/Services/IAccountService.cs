using System.Security.Claims;
using GLMS.Api.ViewModels.Account;

//ST10445500 - PROG7311 - GLMS POE
//IAccountService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Services
{
    public interface IAccountService
    {
        Task<LoginResult> LoginAsync(LoginViewModel vm);
        Task LogoutAsync();
        Task<ProfileViewModel?> GetProfileAsync(ClaimsPrincipal user);
        Task<AccountResult> UpdateProfileAsync(ClaimsPrincipal user, ProfileViewModel vm);
        Task<AccountResult> ChangePasswordAsync(ClaimsPrincipal user, ChangePasswordViewModel vm);
        Task<IReadOnlyList<AdminUserListItemViewModel>> GetUsersAsync();
        Task<AdminUserEditViewModel?> GetUserForEditAsync(string userId);
        Task<AccountResult> CreateUserAsync(AdminUserCreateViewModel vm);
        Task<AccountResult> UpdateUserAsync(AdminUserEditViewModel vm);
        Task<AccountResult> SetUserActiveAsync(string userId, bool isActive);
        Task<AccountResult> ResetPasswordAsync(AdminResetPasswordViewModel vm);
    }
}

//.....................................o0oEND OF FILEo0o........................................//

