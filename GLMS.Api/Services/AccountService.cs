using System.Security.Claims;
using GLMS.Api.Data.Repositories;
using GLMS.Api.DTOs.Auth;
using GLMS.Api.DTOs.Mappings;
using GLMS.Api.Models;
using Microsoft.AspNetCore.Identity;

//ST10445500 - PROG7311 - GLMS POE
//AccountService

//.....................................o0oSTART OF FILEo0o........................................//

// The service keeps business rules and validation away from the controller.

namespace GLMS.Api.Services
{
    //manages user accounts and current signed-in account details
    public interface IAccountService
    {
        Task<IReadOnlyList<UserListDto>> GetUsersAsync();
        Task<UserDetailDto?> GetUserAsync(string userId);
        Task<AccountResult<UserDetailDto>> CreateUserAsync(CreateUserDto dto);
        Task<AccountResult> UpdateUserAsync(string userId, UpdateUserDto dto);
        Task<AccountResult> SetUserActiveAsync(string userId, bool isActive);
        Task<AccountResult> ResetUserPasswordAsync(ResetUserPasswordDto dto);
        Task<AccountResult<AuthResponseDto>> GetCurrentAccountAsync();
        Task<AccountResult<AuthResponseDto>> UpdateCurrentAccountAsync(UpdateAccountProfileDto dto);
        Task<AccountResult<object>> ChangeCurrentPasswordAsync(ChangePasswordRequestDto dto);
        string GetCurrentUserId();
        Task<ApplicationUser?> FindUserByEmailAsync(string email);
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
    }

    //..............................................................................//

    //simple result returned by account service methods
    public class AccountResult
    {
        protected AccountResult(bool succeeded, bool isNotFound, bool isUnauthorized, IReadOnlyList<string> errors)
        {
            Succeeded = succeeded;
            IsNotFound = isNotFound;
            IsUnauthorized = isUnauthorized;
            Errors = errors;
        }

        public bool Succeeded { get; }
        public bool IsNotFound { get; }
        public bool IsUnauthorized { get; }
        public IReadOnlyList<string> Errors { get; }

        public static AccountResult Success() => new(true, false, false, []);

        public static AccountResult Failed(params string[] errors) => new(false, false, false, errors);

        public static AccountResult Failed(IEnumerable<string> errors) => new(false, false, false, errors.ToList());

        public static AccountResult NotFound(params string[] errors) => new(false, true, false, errors);

        public static AccountResult Unauthorized(params string[] errors) => new(false, false, true, errors);
    }

    //..............................................................................//

    //simple result returned by account service methods that include a value
    public class AccountResult<T> : AccountResult
    {
        private AccountResult(bool succeeded, bool isNotFound, bool isUnauthorized, T? value, IReadOnlyList<string> errors)
            : base(succeeded, isNotFound, isUnauthorized, errors)
        {
            Value = value;
        }

        public T? Value { get; }

        public static AccountResult<T> Success(T value) => new(true, false, false, value, []);

        public static new AccountResult<T> Failed(params string[] errors) => new(false, false, false, default, errors);

        public static new AccountResult<T> Failed(IEnumerable<string> errors) => new(false, false, false, default, errors.ToList());

        public static new AccountResult<T> NotFound(params string[] errors) => new(false, true, false, default, errors);

        public static new AccountResult<T> Unauthorized(params string[] errors) => new(false, false, true, default, errors);
    }

    //..............................................................................//

    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(
            IUserRepository userRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        //..............................................................................//

        public async Task<IReadOnlyList<UserListDto>> GetUsersAsync()
        {
            var users = await _userRepository.GetUsersAsync();

            var result = new List<UserListDto>();
            foreach (var user in users)
            {
                var roles = await _userRepository.GetRolesAsync(user);
                result.Add(user.ToUserListDto(roles.FirstOrDefault() ?? string.Empty));
            }

            return result;
        }

        //..............................................................................//

        public async Task<UserDetailDto?> GetUserAsync(string userId)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var roles = await _userRepository.GetRolesAsync(user);
            return user.ToUserDetailDto(roles.FirstOrDefault() ?? ApplicationRoles.LogisticsManager);
        }

        //..............................................................................//

        public async Task<AccountResult<UserDetailDto>> CreateUserAsync(CreateUserDto dto)
        {
            if (!IsSupportedRole(dto.Role))
            {
                return AccountResult<UserDetailDto>.Failed("The selected role is not supported.");
            }

            var email = dto.Email.Trim();
            if (!await _userRepository.IsEmailUniqueAsync(email))
            {
                return AccountResult<UserDetailDto>.Failed("A user with this email address already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userRepository.CreateUserAsync(user, dto.TemporaryPassword, dto.Role);
            if (!result.Succeeded)
            {
                return ToAccountResult<UserDetailDto>(result);
            }

            return AccountResult<UserDetailDto>.Success(user.ToUserDetailDto(dto.Role));
        }

        //..............................................................................//

        public async Task<AccountResult> UpdateUserAsync(string userId, UpdateUserDto dto)
        {
            if (userId != dto.UserId)
            {
                throw new ArgumentException("User ID does not match.");
            }

            if (!IsSupportedRole(dto.Role))
            {
                return AccountResult.Failed("The selected role is not supported.");
            }

            var user = await _userRepository.FindByIdAsync(dto.UserId);
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

            var email = dto.Email.Trim();
            if (!await _userRepository.IsEmailUniqueAsync(email, user.Id))
            {
                return AccountResult.Failed("A user with this email address already exists.");
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = email;
            user.UserName = user.Email;
            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userRepository.UpdateUserAsync(user);
            if (!updateResult.Succeeded)
            {
                return ToAccountResult(updateResult);
            }

            var roleResult = await _userRepository.ChangeRoleAsync(user, dto.Role);
            return roleResult.Succeeded ? AccountResult.Success() : ToAccountResult(roleResult);
        }

        //..............................................................................//

        public async Task<AccountResult> SetUserActiveAsync(string userId, bool isActive)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                return AccountResult.NotFound("User not found.");
            }

            if (!isActive && await IsLastActiveAdminAsync(user))
            {
                return AccountResult.Failed("At least one active admin account is required.");
            }

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userRepository.UpdateUserAsync(user);
            return result.Succeeded ? AccountResult.Success() : ToAccountResult(result);
        }

        //..............................................................................//

        public async Task<AccountResult> ResetUserPasswordAsync(ResetUserPasswordDto dto)
        {
            var user = await _userRepository.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return AccountResult.NotFound("User not found.");
            }

            var result = await _userRepository.ResetPasswordAsync(user, dto.TemporaryPassword);
            return result.Succeeded ? AccountResult.Success() : ToAccountResult(result);
        }

        //..............................................................................//

        public async Task<AccountResult<AuthResponseDto>> GetCurrentAccountAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return AccountResult<AuthResponseDto>.Unauthorized("Unable to find the signed-in user.");
            }

            var roles = await _userRepository.GetRolesAsync(user);
            return AccountResult<AuthResponseDto>.Success(user.ToAuthResponseDto(roles));
        }

        //..............................................................................//

        public async Task<AccountResult<AuthResponseDto>> UpdateCurrentAccountAsync(UpdateAccountProfileDto dto)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return AccountResult<AuthResponseDto>.Unauthorized("Unable to find the signed-in user.");
            }

            var email = dto.Email.Trim();
            if (!await _userRepository.IsEmailUniqueAsync(email, user.Id))
            {
                return AccountResult<AuthResponseDto>.Failed("A user with this email address already exists.");
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = email;
            user.UserName = email;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userRepository.UpdateUserAsync(user);
            if (!result.Succeeded)
            {
                return AccountResult<AuthResponseDto>.Failed(result.Errors.Select(e => e.Description));
            }

            var roles = await _userRepository.GetRolesAsync(user);
            return AccountResult<AuthResponseDto>.Success(user.ToAuthResponseDto(roles));
        }

        //..............................................................................//

        public async Task<AccountResult<object>> ChangeCurrentPasswordAsync(ChangePasswordRequestDto dto)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return AccountResult<object>.Unauthorized("Unable to find the signed-in user.");
            }

            var result = await _userRepository.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            return result.Succeeded
                ? AccountResult<object>.Success(new { })
                : AccountResult<object>.Failed(result.Errors.Select(e => e.Description));
        }

        //..............................................................................//

        public string GetCurrentUserId()
        {
            return CurrentPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("Unable to identify the signed-in user.");
        }

        //..............................................................................//

        public async Task<ApplicationUser?> FindUserByEmailAsync(string email)
        {
            return await _userRepository.FindByEmailAsync(email);
        }

        //..............................................................................//

        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _userRepository.GetRolesAsync(user);
        }

        //..............................................................................//

        private ClaimsPrincipal? CurrentPrincipal => _httpContextAccessor.HttpContext?.User;

        //..............................................................................//

        private async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            return CurrentPrincipal == null ? null : await _userRepository.GetUserAsync(CurrentPrincipal);
        }

        //..............................................................................//

        private static bool IsSupportedRole(string role)
        {
            return ApplicationRoles.All.Contains(role);
        }

        //..............................................................................//

        private async Task<bool> IsLastActiveAdminAsync(ApplicationUser user)
        {
            if (!await _userRepository.IsInRoleAsync(user, ApplicationRoles.Admin))
            {
                return false;
            }

            var activeAdmins = await _userRepository.GetUsersInRoleAsync(ApplicationRoles.Admin);
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

//.....................................o0oEND OF FILEo0o..........................................//
