using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GLMS.Api.DTOs.Auth;
using GLMS.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

//ST10445500 - PROG7311 - GLMS POE
//AuthService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        //..............................................................................//

        public async Task<AuthServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            var email = dto.Email.Trim();
            var user = await _userManager.FindByEmailAsync(email);

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

            var roles = await _userManager.GetRolesAsync(user);
            var token = CreateToken(user, roles);
            return AuthServiceResult<AuthResponseDto>.Success(ToAuthResponse(user, roles, token.Token, token.ExpiresAt));
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
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                return AuthServiceResult<RegisterResponseDto>.Failed(createResult.Errors.Select(e => e.Description));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, ApplicationRoles.LogisticsManager);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return AuthServiceResult<RegisterResponseDto>.Failed(roleResult.Errors.Select(e => e.Description));
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
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
            {
                return AuthServiceResult<AuthResponseDto>.Unauthorized("Unable to find the signed-in user.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            return AuthServiceResult<AuthResponseDto>.Success(ToAuthResponse(user, roles));
        }

        //..............................................................................//

        public async Task<AuthServiceResult<AuthResponseDto>> UpdateProfileAsync(ClaimsPrincipal principal, UpdateProfileRequestDto dto)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
            {
                return AuthServiceResult<AuthResponseDto>.Unauthorized("Unable to find the signed-in user.");
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return AuthServiceResult<AuthResponseDto>.Failed(result.Errors.Select(e => e.Description));
            }

            var roles = await _userManager.GetRolesAsync(user);
            return AuthServiceResult<AuthResponseDto>.Success(ToAuthResponse(user, roles));
        }

        //..............................................................................//

        public async Task<AuthServiceResult<object>> ChangePasswordAsync(ClaimsPrincipal principal, ChangePasswordRequestDto dto)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
            {
                return AuthServiceResult<object>.Unauthorized("Unable to find the signed-in user.");
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
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

        private static AuthResponseDto ToAuthResponse(
            ApplicationUser user,
            IList<string> roles,
            string token = "",
            DateTime expiresAt = default)
        {
            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList()
            };
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o........................................//
