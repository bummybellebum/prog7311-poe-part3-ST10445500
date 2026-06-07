using System.Security.Claims;
using System.Text.Encodings.Web;
using GLMS.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

//ST10445500 - PROG7311 - GLMS POE
//TestingAuthHandler

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services.Testing
{
    public class TestingAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "Testing";

        public TestingAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        //..............................................................................//

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Path.StartsWithSegments("/Account/Login") ||
                Request.Path.StartsWithSegments("/Account/AccessDenied"))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testing-admin"),
                new Claim(ClaimTypes.Name, "admin@glms.local"),
                new Claim(ClaimTypes.Email, "admin@glms.local"),
                new Claim(ClaimTypes.Role, ApplicationRoles.Admin),
                new Claim("ApiToken", "testing-token")
            };

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
