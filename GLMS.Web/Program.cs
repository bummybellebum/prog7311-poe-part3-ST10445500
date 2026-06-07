using GLMS.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

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

            //register MVC authentication with a local cookie
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            builder.Services.AddHttpContextAccessor();

            RegisterApiClient<IAccountService, AccountService>(builder);
            RegisterApiClient<IClientService, ClientService>(builder);
            RegisterApiClient<IContractService, ContractService>(builder);
            RegisterApiClient<IContractDocumentService, ContractDocumentService>(builder);
            RegisterApiClient<IServiceRequestService, ServiceRequestService>(builder);
            RegisterApiClient<ILookupService, LookupService>(builder);
            RegisterApiClient<ICurrencyExchangeService, CurrencyExchangeService>(builder);

            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

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

            app.UseHttpsRedirection();
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
                var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7174/";
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });
        }

        //..............................................................................//
    }
}

//..........................................o0oEND OF FILEo0o..................................................//
