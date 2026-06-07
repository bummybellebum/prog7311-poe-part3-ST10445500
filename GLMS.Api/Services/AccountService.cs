using System.Security.Claims;
using GLMS.Api.DTOs.Auth;
using GLMS.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

//ST10445500 - PROG7311 - GLMS POE
//AccountService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        //..............................................................................//

        public async Task<LoginResult> LoginAsync(LoginRequestDto dto)
        {
            var email = dto.Email.Trim();
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return LoginResult.FailedLogin("Invalid login attempt.");
            }

            if (!user.IsActive)
            {
                return LoginResult.FailedLogin("This account is inactive. Please contact an administrator.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, dto.RememberMe);
                return LoginResult.SuccessLogin();
            }

            if (result.IsLockedOut)
            {
                return LoginResult.LockedOut();
            }

            if (result.RequiresTwoFactor)
            {
                return LoginResult.TwoFactorRequired();
            }

            return LoginResult.FailedLogin("Invalid login attempt.");
        }

        //..............................................................................//

        public Task LogoutAsync()
        {
            return _signInManager.SignOutAsync();
        }

        //..............................................................................//

        public async Task<UpdateProfileRequestDto?> GetProfileAsync(ClaimsPrincipal user)
        {
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser == null)
            {
                return null;
            }

            return new UpdateProfileRequestDto
            {
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email ?? string.Empty
            };
        }

        //..............................................................................//

        public async Task<AccountResult> UpdateProfileAsync(ClaimsPrincipal user, UpdateProfileRequestDto dto)
        {
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser == null)
            {
                return AccountResult.Failed("Unable to find the signed-in user.");
            }

            appUser.FirstName = dto.FirstName;
            appUser.LastName = dto.LastName;

            var result = await _userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
            {
                return ToAccountResult(result);
            }

            await _signInManager.RefreshSignInAsync(appUser);
            return AccountResult.Success();
        }

        //..............................................................................//

        public async Task<AccountResult> ChangePasswordAsync(ClaimsPrincipal user, ChangePasswordRequestDto dto)
        {
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser == null)
            {
                return AccountResult.Failed("Unable to find the signed-in user.");
            }

            var result = await _userManager.ChangePasswordAsync(appUser, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                return ToAccountResult(result);
            }

            await _signInManager.RefreshSignInAsync(appUser);
            return AccountResult.Success();
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
                result.Add(new AdminUserListDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = roles.FirstOrDefault() ?? string.Empty,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                });
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
            return new AdminUserDetailDto
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? ApplicationRoles.LogisticsManager,
                IsActive = user.IsActive
            };
        }

        //..............................................................................//

        public async Task<AccountResult> CreateUserAsync(CreateAdminUserDto dto)
        {
            if (!IsSupportedRole(dto.Role))
            {
                return AccountResult.Failed("The selected role is not supported.");
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
                return ToAccountResult(createResult);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!roleResult.Succeeded)
            {
                return ToAccountResult(roleResult);
            }

            return AccountResult.Success();
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
                return AccountResult.Failed("User not found.");
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
                return AccountResult.Failed("User not found.");
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
                return AccountResult.Failed("User not found.");
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
    }
}

//.....................................o0oEND OF FILEo0o........................................//

