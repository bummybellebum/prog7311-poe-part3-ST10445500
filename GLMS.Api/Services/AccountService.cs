using GLMS.Api.DTOs.Auth;
using GLMS.Api.DTOs.Mappings;
using GLMS.Api.Models;
using GLMS.Api.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;



namespace GLMS.Api.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        //..............................................................................//

        public async Task<IReadOnlyList<AdminUserListDto>> GetUsersAsync()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var result = new List<AdminUserListDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(user.ToAdminUserListDto(roles.FirstOrDefault() ?? string.Empty));
            }

            return result;
        }

        //..............................................................................//

        public async Task<AdminUserDetailDto?> GetUserForEditAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            return user.ToAdminUserDetailDto(roles.FirstOrDefault() ?? ApplicationRoles.LogisticsManager);
        }

        //..............................................................................//

        public async Task<AccountResult<AdminUserDetailDto>> CreateUserAsync(CreateAdminUserDto dto)
        {
            if (!IsSupportedRole(dto.Role))
            {
                return AccountResult<AdminUserDetailDto>.Failed("The selected role is not supported.");
            }

            var email = dto.Email.Trim();
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, dto.TemporaryPassword);
            if (!createResult.Succeeded)
            {
                return ToAccountResult<AdminUserDetailDto>(createResult);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!roleResult.Succeeded)
            {
                return ToAccountResult<AdminUserDetailDto>(roleResult);
            }

            return AccountResult<AdminUserDetailDto>.Success(user.ToAdminUserDetailDto(dto.Role));
        }

        //..............................................................................//

        public async Task<AccountResult> UpdateUserAsync(UpdateAdminUserDto dto)
        {
            if (!IsSupportedRole(dto.Role))
            {
                return AccountResult.Failed("The selected role is not supported.");
            }

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return AccountResult.NotFound("User not found.");
            }

            if (!dto.IsActive && await IsLastActiveAdminAsync(user))
            {
                return AccountResult.Failed("At least one active admin account is required.");
            }

            if (dto.Role != ApplicationRoles.Admin && await IsLastActiveAdminAsync(user))
            {
                return AccountResult.Failed("At least one active admin account is required.");
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email.Trim();
            user.UserName = user.Email;
            user.IsActive = dto.IsActive;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return ToAccountResult(updateResult);
            }

            var existingRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, existingRoles);
            if (!removeResult.Succeeded)
            {
                return ToAccountResult(removeResult);
            }

            var addResult = await _userManager.AddToRoleAsync(user, dto.Role);
            return addResult.Succeeded ? AccountResult.Success() : ToAccountResult(addResult);
        }

        //..............................................................................//

        public async Task<AccountResult> SetUserActiveAsync(string userId, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return AccountResult.NotFound("User not found.");
            }

            if (!isActive && await IsLastActiveAdminAsync(user))
            {
                return AccountResult.Failed("At least one active admin account is required.");
            }

            user.IsActive = isActive;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded ? AccountResult.Success() : ToAccountResult(result);
        }

        //..............................................................................//

        public async Task<AccountResult> ResetPasswordAsync(ResetAdminPasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return AccountResult.NotFound("User not found.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.TemporaryPassword);
            return result.Succeeded ? AccountResult.Success() : ToAccountResult(result);
        }

        //..............................................................................//

        private static bool IsSupportedRole(string role)
        {
            return ApplicationRoles.All.Contains(role);
        }

        //..............................................................................//

        private async Task<bool> IsLastActiveAdminAsync(ApplicationUser user)
        {
            if (!await _userManager.IsInRoleAsync(user, ApplicationRoles.Admin))
            {
                return false;
            }

            var activeAdmins = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Admin);
            return activeAdmins.Count(u => u.IsActive) <= 1;
        }

        //..............................................................................//

        private static AccountResult ToAccountResult(IdentityResult result)
        {
            return AccountResult.Failed(result.Errors.Select(e => e.Description));
        }

        //..............................................................................//

        private static AccountResult<T> ToAccountResult<T>(IdentityResult result)
        {
            return AccountResult<T>.Failed(result.Errors.Select(e => e.Description));
        }

        //..............................................................................//
    }
}


