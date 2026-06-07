using GLMS.Api.Data.Repositories;
using GLMS.Api.DTOs.Auth;
using GLMS.Api.DTOs.Mappings;
using GLMS.Api.Models;
using Microsoft.AspNetCore.Identity;



namespace GLMS.Api.Services
{
    //manages admin user account actions
    public interface IAccountService
    {
        Task<IReadOnlyList<AdminUserListDto>> GetUsersAsync();
        Task<AdminUserDetailDto?> GetUserForEditAsync(string userId);
        Task<AccountResult<AdminUserDetailDto>> CreateUserAsync(CreateAdminUserDto dto);
        Task<AccountResult> UpdateUserAsync(UpdateAdminUserDto dto);
        Task<AccountResult> SetUserActiveAsync(string userId, bool isActive);
        Task<AccountResult> ResetPasswordAsync(ResetAdminPasswordDto dto);
    }

    //..............................................................................//

    //simple result returned by account service methods
    public class AccountResult
    {
        protected AccountResult(bool succeeded, bool isNotFound, IReadOnlyList<string> errors)
        {
            Succeeded = succeeded;
            IsNotFound = isNotFound;
            Errors = errors;
        }

        public bool Succeeded { get; }
        public bool IsNotFound { get; }
        public IReadOnlyList<string> Errors { get; }

        public static AccountResult Success() => new(true, false, []);

        public static AccountResult Failed(params string[] errors) => new(false, false, errors);

        public static AccountResult Failed(IEnumerable<string> errors) => new(false, false, errors.ToList());

        public static AccountResult NotFound(params string[] errors) => new(false, true, errors);
    }

    //..............................................................................//

    //simple result returned by account service methods that include a value
    public class AccountResult<T> : AccountResult
    {
        private AccountResult(bool succeeded, bool isNotFound, T? value, IReadOnlyList<string> errors)
            : base(succeeded, isNotFound, errors)
        {
            Value = value;
        }

        public T? Value { get; }

        public static AccountResult<T> Success(T value) => new(true, false, value, []);

        public static new AccountResult<T> Failed(params string[] errors) => new(false, false, default, errors);

        public static new AccountResult<T> Failed(IEnumerable<string> errors) => new(false, false, default, errors.ToList());

        public static new AccountResult<T> NotFound(params string[] errors) => new(false, true, default, errors);
    }

    //..............................................................................//

    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;

        public AccountService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        //..............................................................................//

        public async Task<IReadOnlyList<AdminUserListDto>> GetUsersAsync()
        {
            var users = await _userRepository.GetUsersAsync();

            var result = new List<AdminUserListDto>();
            foreach (var user in users)
            {
                var roles = await _userRepository.GetRolesAsync(user);
                result.Add(user.ToAdminUserListDto(roles.FirstOrDefault() ?? string.Empty));
            }

            return result;
        }

        //..............................................................................//

        public async Task<AdminUserDetailDto?> GetUserForEditAsync(string userId)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var roles = await _userRepository.GetRolesAsync(user);
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
            if (!await _userRepository.IsEmailUniqueAsync(email))
            {
                return AccountResult<AdminUserDetailDto>.Failed("A user with this email address already exists.");
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
                return ToAccountResult<AdminUserDetailDto>(result);
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

        public async Task<AccountResult> ResetPasswordAsync(ResetAdminPasswordDto dto)
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


