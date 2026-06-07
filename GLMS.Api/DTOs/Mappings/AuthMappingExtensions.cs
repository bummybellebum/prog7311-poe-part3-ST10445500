using GLMS.Api.DTOs.Auth;
using GLMS.Api.Models;

namespace GLMS.Api.DTOs.Mappings
{
    public static class AuthMappingExtensions
    {
        public static AuthResponseDto ToAuthResponseDto(
            this ApplicationUser user,
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

        public static AdminUserListDto ToAdminUserListDto(this ApplicationUser user, string role)
        {
            return new AdminUserListDto
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public static AdminUserDetailDto ToAdminUserDetailDto(this ApplicationUser user, string role)
        {
            return new AdminUserDetailDto
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Role = role,
                IsActive = user.IsActive
            };
        }
    }
}
