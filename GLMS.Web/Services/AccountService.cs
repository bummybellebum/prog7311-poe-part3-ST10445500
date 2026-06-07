using System.Security.Claims;
using System.Text;
using System.Text.Json;
using GLMS.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

//ST10445500 - PROG7311 - GLMS POE
//AccountService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services
{
    public class AccountService : ApiClientService, IAccountService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        //..............................................................................//

        public async Task<LoginResult> LoginAsync(LoginViewModel vm)
        {
            using var content = new StringContent(JsonSerializer.Serialize(vm, JsonOptions), Encoding.UTF8, "application/json");
            using var response = await HttpClient.PostAsync("api/auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                return LoginResult.FailedLogin(await ReadLoginErrorAsync(response));
            }

            var auth = await ReadAsync<AuthResponseDto>(response);
            if (auth == null || string.IsNullOrWhiteSpace(auth.Token))
            {
                return LoginResult.FailedLogin("The API did not return a valid login token.");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, auth.UserId),
                new(ClaimTypes.Name, auth.Email),
                new(ClaimTypes.Email, auth.Email),
                new("ApiToken", auth.Token)
            };

            foreach (var role in auth.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var properties = new AuthenticationProperties
            {
                IsPersistent = vm.RememberMe,
                ExpiresUtc = auth.ExpiresAt
            };

            await _httpContextAccessor.HttpContext!.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
            return LoginResult.SuccessLogin();
        }

        //..............................................................................//

        public Task LogoutAsync()
        {
            return _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        //..............................................................................//

        public async Task<ProfileViewModel?> GetProfileAsync(ClaimsPrincipal user)
        {
            var auth = await GetAsync<AuthResponseDto>("api/auth/me");
            if (auth == null)
            {
                return null;
            }

            return new ProfileViewModel
            {
                FirstName = auth.FirstName,
                LastName = auth.LastName,
                Email = auth.Email
            };
        }

        //..............................................................................//

        public async Task<AccountResult> UpdateProfileAsync(ClaimsPrincipal user, ProfileViewModel vm)
        {
            try
            {
                await PutAsync("api/auth/profile", vm);
                return AccountResult.Success();
            }
            catch (InvalidOperationException ex)
            {
                return AccountResult.Failed(ex.Message);
            }
        }

        //..............................................................................//

        public async Task<AccountResult> ChangePasswordAsync(ClaimsPrincipal user, ChangePasswordViewModel vm)
        {
            try
            {
                await PostNoResultAsync("api/auth/change-password", vm);
                return AccountResult.Success();
            }
            catch (InvalidOperationException ex)
            {
                return AccountResult.Failed(ex.Message);
            }
        }

        //..............................................................................//

        public async Task<IReadOnlyList<AdminUserListItemViewModel>> GetUsersAsync()
        {
            return await GetAsync<List<AdminUserListItemViewModel>>("api/admin/users") ?? new List<AdminUserListItemViewModel>();
        }

        //..............................................................................//

        public Task<AdminUserEditViewModel?> GetUserForEditAsync(string userId)
        {
            return GetAsync<AdminUserEditViewModel>($"api/admin/users/{Uri.EscapeDataString(userId)}");
        }

        //..............................................................................//

        public async Task<AccountResult> CreateUserAsync(AdminUserCreateViewModel vm)
        {
            try
            {
                await PostNoResultAsync("api/admin/users", vm);
                return AccountResult.Success();
            }
            catch (InvalidOperationException ex)
            {
                return AccountResult.Failed(ex.Message);
            }
        }

        //..............................................................................//

        public async Task<AccountResult> UpdateUserAsync(AdminUserEditViewModel vm)
        {
            try
            {
                await PutAsync($"api/admin/users/{Uri.EscapeDataString(vm.UserId)}", vm);
                return AccountResult.Success();
            }
            catch (InvalidOperationException ex)
            {
                return AccountResult.Failed(ex.Message);
            }
        }

        //..............................................................................//

        public async Task<AccountResult> SetUserActiveAsync(string userId, bool isActive)
        {
            try
            {
                await PatchAsync($"api/admin/users/{Uri.EscapeDataString(userId)}/active", new { isActive });
                return AccountResult.Success();
            }
            catch (InvalidOperationException ex)
            {
                return AccountResult.Failed(ex.Message);
            }
        }

        //..............................................................................//

        public async Task<AccountResult> ResetPasswordAsync(AdminResetPasswordViewModel vm)
        {
            try
            {
                await PostNoResultAsync($"api/admin/users/{Uri.EscapeDataString(vm.UserId)}/reset-password", vm);
                return AccountResult.Success();
            }
            catch (InvalidOperationException ex)
            {
                return AccountResult.Failed(ex.Message);
            }
        }

        //..............................................................................//

        private static async Task<string> ReadLoginErrorAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return "Invalid login attempt.";
            }

            try
            {
                var error = JsonSerializer.Deserialize<ApiErrorResponse>(content, JsonOptions);
                if (error?.Errors?.Count > 0)
                {
                    return string.Join(" ", error.Errors);
                }
            }
            catch
            {
                return "Invalid login attempt.";
            }

            return "Invalid login attempt.";
        }

        //..............................................................................//

        private class AuthResponseDto
        {
            public string Token { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public string UserId { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public List<string> Roles { get; set; } = new();
        }

        private class ApiErrorResponse
        {
            public List<string> Errors { get; set; } = new();
        }
    }
}

//.....................................o0oEND OF FILEo0o........................................//
