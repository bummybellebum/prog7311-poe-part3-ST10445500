using System.Security.Claims;

//ST10445500 - PROG7311 - GLMS POE
//ICurrentUserService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Services
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserName { get; }
        ClaimsPrincipal? User { get; }
    }
}

//.....................................o0oEND OF FILEo0o........................................//

