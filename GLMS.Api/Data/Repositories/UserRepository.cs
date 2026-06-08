using System.Security.Claims;
using GLMS.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

//ST10445500 - PROG7311 - GLMS POE
//UserRepository

//.....................................o0oSTART OF FILEo0o........................................//

// The repository keeps database query code in one layer instead of inside controllers.

namespace GLMS.Api.Data.Repositories
{
    public interface IUserRepository
    {
        Task<List<ApplicationUser>> GetUsersAsync();
        Task<ApplicationUser?> FindByIdAsync(string userId);
        Task<ApplicationUser?> FindByEmailAsync(string email);
        Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal);
        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        Task<bool> IsInRoleAsync(ApplicationUser user, string role);
        Task<IReadOnlyList<ApplicationUser>> GetUsersInRoleAsync(string role);
        Task<bool> IsEmailUniqueAsync(string email, string? excludeUserId = null);
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password, string role);
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
        Task<IdentityResult> ChangeRoleAsync(ApplicationUser user, string role);
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string newPassword);
        Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
    }

    //..............................................................................//

    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        //..............................................................................//

        public async Task<List<ApplicationUser>> GetUsersAsync()
        {
            return await _userManager.Users
                .AsNoTracking()
                .OrderBy(u => u.Email)
                .ToListAsync();
        }

        //..............................................................................//

        public async Task<ApplicationUser?> FindByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        //..............................................................................//

        public async Task<ApplicationUser?> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email.Trim());
        }

        //..............................................................................//

        public async Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal)
        {
            return await _userManager.GetUserAsync(principal);
        }

        //..............................................................................//

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        //..............................................................................//

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string role)
        {
            return await _userManager.IsInRoleAsync(user, role);
        }

        //..............................................................................//

        public async Task<IReadOnlyList<ApplicationUser>> GetUsersInRoleAsync(string role)
        {
            return (await _userManager.GetUsersInRoleAsync(role)).ToList();
        }

        //..............................................................................//

        public async Task<bool> IsEmailUniqueAsync(string email, string? excludeUserId = null)
        {
            var existingUser = await FindByEmailAsync(email);

            return existingUser == null ||
                (!string.IsNullOrWhiteSpace(excludeUserId) && existingUser.Id == excludeUserId);
        }

        //..............................................................................//

        public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password, string role)
        {
            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                return createResult;
            }

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
            }

            return roleResult;
        }

        //..............................................................................//

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
        {
            return await _userManager.UpdateAsync(user);
        }

        //..............................................................................//

        public async Task<IdentityResult> ChangeRoleAsync(ApplicationUser user, string role)
        {
            var existingRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, existingRoles);
            if (!removeResult.Succeeded)
            {
                return removeResult;
            }

            return await _userManager.AddToRoleAsync(user, role);
        }

        //..............................................................................//

        public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string newPassword)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        //..............................................................................//

        public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o........................................//

//.....................................o0oEND OF FILEo0o..........................................//
