using GLMS.Web.ApiClients;
using GLMS.Web.ApiClients.Testing;
using GLMS.Web.Security.Testing;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;

//ST10445500 - PROG7311 - GLMS POE
//Program.cs

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var isTesting = builder.Environment.IsEnvironment("Testing");
            if (isTesting)
            {
                builder.Logging.ClearProviders();
                builder.Logging.AddConsole();
            }

            //register MVC authentication with a local cookie
            if (isTesting)
            {
                builder.Services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "..", "obj", "glms-web-testing-keys")));

                builder.Services.AddAuthentication(TestingAuthHandler.SchemeName)
                    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestingAuthHandler>(
                        TestingAuthHandler.SchemeName,
                        options => { });
            }
            else
            {
                builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.LoginPath = "/Account/Login";
                        options.LogoutPath = "/Account/Logout";
                        options.AccessDeniedPath = "/Account/AccessDenied";
                    });
            }

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            builder.Services.AddHttpContextAccessor();

            if (isTesting)
            {
                builder.Services.AddScoped<IAuthApiClient, TestingAuthApiClient>();
                builder.Services.AddScoped<IClientsApiClient, TestingClientsApiClient>();
                builder.Services.AddScoped<IContractsApiClient, TestingContractsApiClient>();
                builder.Services.AddScoped<IServiceRequestsApiClient, TestingServiceRequestsApiClient>();
                builder.Services.AddScoped<ILookupsApiClient, TestingLookupsApiClient>();
            }
            else
            {
                RegisterApiClient<IAuthApiClient, AuthApiClient>(builder);
                RegisterApiClient<IClientsApiClient, ClientsApiClient>(builder);
                RegisterApiClient<IContractsApiClient, ContractsApiClient>(builder);
                RegisterApiClient<IServiceRequestsApiClient, ServiceRequestsApiClient>(builder);
                RegisterApiClient<ILookupsApiClient, LookupsApiClient>(builder);
            }

            //add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            //set up how the app handles web requests
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // Enable HSTS for security
                app.UseHsts();
            }

            if (!isTesting)
            {
                app.UseHttpsRedirection();
            }
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }

        //..............................................................................//

        private static void RegisterApiClient<TInterface, TImplementation>(WebApplicationBuilder builder)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            builder.Services.AddHttpClient<TInterface, TImplementation>((sp, client) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var baseUrl = configuration["ApiSettings:BaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    throw new InvalidOperationException("ApiSettings:BaseUrl must be configured for GLMS.Web API clients.");
                }

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });
        }

        //..............................................................................//
    }
}

//..........................................o0oEND OF FILEo0o..................................................//
