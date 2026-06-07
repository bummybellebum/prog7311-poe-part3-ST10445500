using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GLMS.Api.Data.Repositories;
using GLMS.Api.DTOs.Auth;
using GLMS.Api.DTOs.Mappings;
using GLMS.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;



namespace GLMS.Api.Services
{
    //manages login, registration, and signed-in user account actions
    public interface IAuthService
    {
        Task<AuthServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto dto);
        Task<AuthServiceResult<RegisterResponseDto>> RegisterAsync(RegisterRequestDto dto);
        Task<AuthServiceResult<AuthResponseDto>> GetCurrentUserAsync(ClaimsPrincipal principal);
        Task<AuthServiceResult<AuthResponseDto>> UpdateProfileAsync(ClaimsPrincipal principal, UpdateProfileRequestDto dto);
        Task<AuthServiceResult<object>> ChangePasswordAsync(ClaimsPrincipal principal, ChangePasswordRequestDto dto);
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

        public static AuthServiceResult<T> Failed(params string[] errors) => new(false, false, default, errors);

        public static AuthServiceResult<T> Failed(IEnumerable<string> errors) => new(false, false, default, errors.ToList());

        public static AuthServiceResult<T> Unauthorized(params string[] errors) => new(false, true, default, errors);
    }

    //..............................................................................//

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        //..............................................................................//

        public async Task<AuthServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            var email = dto.Email.Trim();
            var user = await _userRepository.FindByEmailAsync(email);

            if (user == null)
            {
                return AuthServiceResult<AuthResponseDto>.Unauthorized("Invalid login attempt.");
            }

            if (!user.IsActive)
            {
                return AuthServiceResult<AuthResponseDto>.Unauthorized("This account is inactive. Please contact an administrator.");
            }

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

            var roles = await _userRepository.GetRolesAsync(user);
            var token = CreateToken(user, roles);
            return AuthServiceResult<AuthResponseDto>.Success(user.ToAuthResponseDto(roles, token.Token, token.ExpiresAt));
        }

        //..............................................................................//

        public async Task<AuthServiceResult<RegisterResponseDto>> RegisterAsync(RegisterRequestDto dto)
        {
            var email = dto.Email.Trim();
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await _userRepository.CreateUserAsync(user, dto.Password, ApplicationRoles.LogisticsManager);
            if (!createResult.Succeeded)
            {
                return AuthServiceResult<RegisterResponseDto>.Failed(createResult.Errors.Select(e => e.Description));
            }

            return AuthServiceResult<RegisterResponseDto>.Success(new RegisterResponseDto
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Role = ApplicationRoles.LogisticsManager
            });
        }

        //..............................................................................//

        public async Task<AuthServiceResult<AuthResponseDto>> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            var user = await _userRepository.GetUserAsync(principal);
            if (user == null)
            {
                return AuthServiceResult<AuthResponseDto>.Unauthorized("Unable to find the signed-in user.");
            }

            var roles = await _userRepository.GetRolesAsync(user);
            return AuthServiceResult<AuthResponseDto>.Success(user.ToAuthResponseDto(roles));
        }

        //..............................................................................//

        public async Task<AuthServiceResult<AuthResponseDto>> UpdateProfileAsync(ClaimsPrincipal principal, UpdateProfileRequestDto dto)
        {
            var user = await _userRepository.GetUserAsync(principal);
            if (user == null)
            {
                return AuthServiceResult<AuthResponseDto>.Unauthorized("Unable to find the signed-in user.");
            }

            var email = dto.Email.Trim();
            if (!await _userRepository.IsEmailUniqueAsync(email, user.Id))
            {
                return AuthServiceResult<AuthResponseDto>.Failed("A user with this email address already exists.");
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = email;
            user.UserName = email;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userRepository.UpdateUserAsync(user);
            if (!result.Succeeded)
            {
                return AuthServiceResult<AuthResponseDto>.Failed(result.Errors.Select(e => e.Description));
            }

            var roles = await _userRepository.GetRolesAsync(user);
            return AuthServiceResult<AuthResponseDto>.Success(user.ToAuthResponseDto(roles));
        }

        //..............................................................................//

        public async Task<AuthServiceResult<object>> ChangePasswordAsync(ClaimsPrincipal principal, ChangePasswordRequestDto dto)
        {
            var user = await _userRepository.GetUserAsync(principal);
            if (user == null)
            {
                return AuthServiceResult<object>.Unauthorized("Unable to find the signed-in user.");
            }

            var result = await _userRepository.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            return result.Succeeded
                ? AuthServiceResult<object>.Success(new { })
                : AuthServiceResult<object>.Failed(result.Errors.Select(e => e.Description));
        }

        //..............................................................................//

        private (string Token, DateTime ExpiresAt) CreateToken(ApplicationUser user, IList<string> roles)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT key is missing.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiresMinutes"] ?? "120"));

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

