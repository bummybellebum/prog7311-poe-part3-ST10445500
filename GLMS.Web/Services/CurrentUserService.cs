using System.Security.Claims;

//ST10445500 - PROG7311 - GLMS POE
//CurrentUserService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        //..............................................................................//

        public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public string? UserName => User?.Identity?.Name;

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o........................................//
