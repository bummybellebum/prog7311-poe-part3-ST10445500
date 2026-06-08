//ST10445500 - PROG7311 - GLMS POE
//AuthApiModels

//.....................................o0oSTART OF FILEo0o........................................//

// These API models match the JSON sent between the MVC frontend and the Web API.



//ST10445500 - PROG7311 - GLMS POE
//AuthApiModels

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web.ApiClients.Models
{
	public class LoginRequestDto
	{
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public bool RememberMe { get; set; }
		public string? ReturnUrl { get; set; }
	}

	public class AuthResponseDto
	{
		public string Token { get; set; } = string.Empty;
		public DateTime ExpiresAt { get; set; }
		public string UserId { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public List<string> Roles { get; set; } = new();
	}

	public class UpdateAccountProfileDto
	{
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string Email { get; set; } = string.Empty;
	}

	public class ChangePasswordRequestDto
	{
		public string CurrentPassword { get; set; } = string.Empty;
		public string NewPassword { get; set; } = string.Empty;
		public string ConfirmPassword { get; set; } = string.Empty;
	}

	public class UserListDto
	{
		public string UserId { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string Role { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }

		public string DisplayName
		{
			get
			{
				var name = $"{FirstName} {LastName}".Trim();
				return string.IsNullOrWhiteSpace(name) ? Email : name;
			}
		}
	}

	public class UserDetailDto
	{
		public string UserId { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string Role { get; set; } = string.Empty;
		public bool IsActive { get; set; }
	}

	public class CreateUserDto
	{
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string Email { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public string TemporaryPassword { get; set; } = string.Empty;
		public string ConfirmTemporaryPassword { get; set; } = string.Empty;
		public bool IsActive { get; set; } = true;
	}

	public class UpdateUserDto
	{
		public string UserId { get; set; } = string.Empty;
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string Email { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public bool IsActive { get; set; }
	}

	public class ResetUserPasswordDto
	{
		public string UserId { get; set; } = string.Empty;
		public string TemporaryPassword { get; set; } = string.Empty;
		public string ConfirmTemporaryPassword { get; set; } = string.Empty;
	}

	public class UpdateUserActiveDto
	{
		public bool IsActive { get; set; }
	}
}

//..........................................o0oEND OF FILEo0o..................................................//

//.....................................o0oEND OF FILEo0o..........................................//
