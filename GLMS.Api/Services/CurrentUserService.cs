using System.Security.Claims;
using GLMS.Api.Models;
using Microsoft.AspNetCore.Identity;

//ST10445500 - PROG7311 - GLMS POE
//CurrentUserService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Services
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserName { get; }
        ClaimsPrincipal? User { get; }
    }

    //..............................................................................//

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        //..............................................................................//

        public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public string? UserId => User == null ? null : _userManager.GetUserId(User);

        public string? UserName => User?.Identity?.Name;

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o........................................//

