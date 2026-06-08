using GLMS.Api.Data;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

//ST10445500 - PROG7311 - GLMS POE
//GlmsApiFactory

//.....................................o0oSTART OF FILEo0o........................................//

// Test helpers keep repeated setup code out of the test classes.

namespace GLMS.Tests.Helpers
{
    public sealed class GlmsApiFactory : WebApplicationFactory<GLMS.Api.Program>
    {
        private readonly string _databaseName = Guid.NewGuid().ToString();

        public async Task ResetDatabaseAsync()
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureDeletedAsync();
            // Each integration test starts from known data so test results are repeatable.
            await TestDbContextFactory.SeedIntegrationDataAsync(context);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureAppConfiguration(configuration =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=GLMS.Tests;Trusted_Connection=True;",
                    ["Jwt:Key"] = "01234567890123456789012345678901",
                    ["Jwt:Issuer"] = "GLMS.Tests",
                    ["Jwt:Audience"] = "GLMS.Tests",
                    ["Jwt:ExpiresMinutes"] = "60"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                // Tests use an in-memory database so they do not depend on a developer SQL Server.
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));

                services.RemoveAll<ICurrencyExchangeService>();
                // The fake currency service keeps tests stable and avoids external API calls.
                services.AddSingleton<ICurrencyExchangeService, FakeCurrencyExchangeService>();

                services.PostConfigure<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                });

                services.AddAuthentication(TestAuthHandler.SchemeName)
                    // The test auth handler lets integration tests reach protected API routes.
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
            });
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
