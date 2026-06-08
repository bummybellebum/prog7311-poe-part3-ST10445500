using GLMS.Api.DTOs.Auth;
using GLMS.Api.Models;

//ST10445500 - PROG7311 - GLMS POE
//AuthMappingExtensions

//.....................................o0oSTART OF FILEo0o........................................//

// Mapping keeps the API DTO shapes separate from the EF database models.

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

        public static UserListDto ToUserListDto(this ApplicationUser user, string role)
        {
            return new UserListDto
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

        public static UserDetailDto ToUserDetailDto(this ApplicationUser user, string role)
        {
            return new UserDetailDto
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

//.....................................o0oEND OF FILEo0o..........................................//
