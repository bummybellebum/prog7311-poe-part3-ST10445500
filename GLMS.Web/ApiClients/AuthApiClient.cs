using GLMS.Web.ApiClients.Models;

//ST10445500 - PROG7311 - GLMS POE
//AuthApiClient

//.....................................o0oSTART OF FILEo0o........................................//

// The MVC frontend uses this API client to call the backend with HttpClient.

namespace GLMS.Web.ApiClients
{
	public interface IAuthApiClient
	{
		Task<ApiClientResult<AuthResponseDto>> LoginAsync(LoginRequestDto request);
		Task<ApiClientResult<AuthResponseDto>> GetCurrentAccountAsync();
		Task<ApiClientResult<AuthResponseDto>> UpdateCurrentAccountAsync(UpdateAccountProfileDto request);
		Task<ApiClientResult> ChangePasswordAsync(ChangePasswordRequestDto request);
		Task<ApiClientResult<List<UserListDto>>> GetUsersAsync();
		Task<ApiClientResult<UserDetailDto>> GetUserAsync(string userId);
		Task<ApiClientResult<UserDetailDto>> CreateUserAsync(CreateUserDto request);
		Task<ApiClientResult<UserDetailDto>> UpdateUserAsync(UpdateUserDto request);
		Task<ApiClientResult<UserDetailDto>> SetUserActiveAsync(string userId, bool isActive);
		Task<ApiClientResult> ResetPasswordAsync(ResetUserPasswordDto request);
	}

	//..............................................................................//

	public class AuthApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : ApiClientBase(httpClient, httpContextAccessor), IAuthApiClient
	{

		//..............................................................................//

		public Task<ApiClientResult<AuthResponseDto>> LoginAsync(LoginRequestDto request)
		{
			return PostAsync<LoginRequestDto, AuthResponseDto>("api/auth/login", request);
		}

		//..............................................................................//

		public Task<ApiClientResult<AuthResponseDto>> GetCurrentAccountAsync()
		{
			return GetAsync<AuthResponseDto>("api/accounts/me");
		}

		//..............................................................................//

		public Task<ApiClientResult<AuthResponseDto>> UpdateCurrentAccountAsync(UpdateAccountProfileDto request)
		{
			return PutAsync<UpdateAccountProfileDto, AuthResponseDto>("api/accounts/me", request);
		}

		//..............................................................................//

		public Task<ApiClientResult> ChangePasswordAsync(ChangePasswordRequestDto request)
		{
			return PostAsync("api/accounts/me/change-password", request);
		}

		//..............................................................................//

		public async Task<ApiClientResult<List<UserListDto>>> GetUsersAsync()
		{
			var result = await GetAsync<List<UserListDto>>("api/accounts");
			if (result.IsSuccess)
			{
				result.Data ??= new List<UserListDto>();
			}

			return result;
		}

		//..............................................................................//

		public Task<ApiClientResult<UserDetailDto>> GetUserAsync(string userId)
		{
			return GetAsync<UserDetailDto>($"api/accounts/{Uri.EscapeDataString(userId)}");
		}

		//..............................................................................//

		public Task<ApiClientResult<UserDetailDto>> CreateUserAsync(CreateUserDto request)
		{
			return PostAsync<CreateUserDto, UserDetailDto>("api/accounts", request);
		}

		//..............................................................................//

		public Task<ApiClientResult<UserDetailDto>> UpdateUserAsync(UpdateUserDto request)
		{
			return PutAsync<UpdateUserDto, UserDetailDto>($"api/accounts/{Uri.EscapeDataString(request.UserId)}", request);
		}

		//..............................................................................//

		public Task<ApiClientResult<UserDetailDto>> SetUserActiveAsync(string userId, bool isActive)
		{
			return PatchAsync<UpdateUserActiveDto, UserDetailDto>(
				$"api/accounts/{Uri.EscapeDataString(userId)}/active",
				new UpdateUserActiveDto { IsActive = isActive });
		}

		//..............................................................................//

		public Task<ApiClientResult> ResetPasswordAsync(ResetUserPasswordDto request)
		{
			return PostAsync($"api/accounts/{Uri.EscapeDataString(request.UserId)}/reset-password", request);
		}

		//..............................................................................//
	}
}

//.....................................o0oEND OF FILEo0o..........................................//
