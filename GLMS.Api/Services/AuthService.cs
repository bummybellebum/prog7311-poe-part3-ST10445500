using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GLMS.Api.DTOs.Auth;
using GLMS.Api.DTOs.Mappings;
using GLMS.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

//ST10445500 - PROG7311 - GLMS POE
//AuthService

//.....................................o0oSTART OF FILEo0o........................................//

// The service keeps business rules and validation away from the controller.

namespace GLMS.Api.Services
{
    //manages login and token creation
    public interface IAuthService
    {
        Task<AuthServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto dto);
    }

    //..............................................................................//

    //simple result returned by auth service methods
    public class AuthServiceResult<T>
    {
        private AuthServiceResult(bool succeeded, bool isUnauthorized, T? value, IReadOnlyList<string> errors)
        {
            Succeeded = succeeded;
            IsUnauthorized = isUnauthorized;
            Value = value;
            Errors = errors;
        }

        public bool Succeeded { get; }
        public bool IsUnauthorized { get; }
        public T? Value { get; }
        public IReadOnlyList<string> Errors { get; }

        public static AuthServiceResult<T> Success(T value) => new(true, false, value, []);

        public static AuthServiceResult<T> Unauthorized(params string[] errors) => new(false, true, default, errors);
    }

    //..............................................................................//

    public class AuthService : IAuthService
    {
        private readonly IAccountService _accountService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            IAccountService accountService,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _accountService = accountService;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        //..............................................................................//

        public async Task<AuthServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            var email = dto.Email.Trim();
            var user = await _accountService.FindUserByEmailAsync(email);

            if (user == null)
            {
                return AuthServiceResult<AuthResponseDto>.Unauthorized("Invalid login attempt.");
            }

            if (!user.IsActive)
            {
                return AuthServiceResult<AuthResponseDto>.Unauthorized("This account is inactive. Please contact an administrator.");
            }

            // Identity checks the password and handles lockout rules for safer login attempts.
            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    return AuthServiceResult<AuthResponseDto>.Unauthorized("This account is locked. Try again later.");
                }

                if (result.RequiresTwoFactor)
                {
                    return AuthServiceResult<AuthResponseDto>.Unauthorized("Two-factor sign-in is not enabled for GLMS.");
                }

                return AuthServiceResult<AuthResponseDto>.Unauthorized("Invalid login attempt.");
            }

            var roles = await _accountService.GetUserRolesAsync(user);
            var token = CreateToken(user, roles);
            return AuthServiceResult<AuthResponseDto>.Success(user.ToAuthResponseDto(roles, token.Token, token.ExpiresAt));
        }

        //..............................................................................//

        private (string Token, DateTime ExpiresAt) CreateToken(ApplicationUser user, IList<string> roles)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT key is missing.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiresMinutes"] ?? "120"));

            // The API puts user and role details into the token so protected endpoints can authorize requests.
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.Email ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        //..............................................................................//

    }
}

//.....................................o0oEND OF FILEo0o..........................................//
