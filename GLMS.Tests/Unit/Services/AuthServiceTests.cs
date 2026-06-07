using System.Security.Claims;
using GLMS.Api.Data;
using GLMS.Api.DTOs.Auth;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

//ST10445500 - PROG7311 - GLMS POE
//AuthServiceTests

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Tests.Unit.Services
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsTokenProfileAndRoles()
        {
            var fixture = CreateFixture();
            await fixture.CreateUserAsync("user@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: true);

            var result = await fixture.AuthService.LoginAsync(new LoginRequestDto
            {
                Email = "user@glms.local",
                Password = "Password123!"
            });

            Assert.True(result.Succeeded);
            Assert.NotNull(result.Value);
            Assert.False(string.IsNullOrWhiteSpace(result.Value.Token));
            Assert.True(result.Value.ExpiresAt > DateTime.UtcNow);
            Assert.Equal("user@glms.local", result.Value.Email);
            Assert.Contains(ApplicationRoles.LogisticsManager, result.Value.Roles);
        }

        //........................................................................................//

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_Fails()
        {
            var fixture = CreateFixture();
            await fixture.CreateUserAsync("user@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: true);

            var result = await fixture.AuthService.LoginAsync(new LoginRequestDto
            {
                Email = "user@glms.local",
                Password = "WrongPassword123!"
            });

            Assert.False(result.Succeeded);
            Assert.True(result.IsUnauthorized);
            Assert.Contains("Invalid login attempt.", result.Errors);
        }

        //........................................................................................//

        [Fact]
        public async Task LoginAsync_WithInactiveUser_FailsBeforePasswordSignIn()
        {
            var fixture = CreateFixture();
            await fixture.CreateUserAsync("user@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: false);

            var result = await fixture.AuthService.LoginAsync(new LoginRequestDto
            {
                Email = "user@glms.local",
                Password = "Password123!"
            });

            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, error => error.Contains("inactive", StringComparison.OrdinalIgnoreCase));
        }

        //........................................................................................//

        [Fact]
        public async Task RegisterAsync_CreatesOnlyLogisticsManager()
        {
            var fixture = CreateFixture();

            var result = await fixture.AuthService.RegisterAsync(new RegisterRequestDto
            {
                FirstName = "New",
                LastName = "User",
                Email = "newuser@glms.local",
                Password = "Password123!"
            });

            var user = await fixture.UserManager.FindByEmailAsync("newuser@glms.local");
            IList<string> roles = user == null ? new List<string>() : await fixture.UserManager.GetRolesAsync(user);

            Assert.True(result.Succeeded);
            Assert.NotNull(result.Value);
            Assert.Equal(ApplicationRoles.LogisticsManager, result.Value.Role);
            Assert.NotNull(user);
            Assert.Equal([ApplicationRoles.LogisticsManager], roles);
        }

        //........................................................................................//

        [Fact]
        public async Task ChangePasswordAsync_WithValidCurrentPassword_Succeeds()
        {
            var fixture = CreateFixture();
            var user = await fixture.CreateUserAsync("user@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: true);

            var result = await fixture.AuthService.ChangePasswordAsync(CreatePrincipal(user), new ChangePasswordRequestDto
            {
                CurrentPassword = "Password123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            });

            Assert.True(result.Succeeded);
            Assert.True(await fixture.UserManager.CheckPasswordAsync(user, "NewPassword123!"));
        }

        //........................................................................................//

        [Fact]
        public async Task ChangePasswordAsync_WithInvalidCurrentPassword_Fails()
        {
            var fixture = CreateFixture();
            var user = await fixture.CreateUserAsync("user@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: true);

            var result = await fixture.AuthService.ChangePasswordAsync(CreatePrincipal(user), new ChangePasswordRequestDto
            {
                CurrentPassword = "WrongPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            });

            Assert.False(result.Succeeded);
        }

        //........................................................................................//

        private static ClaimsPrincipal CreatePrincipal(ApplicationUser user)
        {
            var identity = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Email ?? string.Empty)
                ],
                IdentityConstants.ApplicationScheme);

            return new ClaimsPrincipal(identity);
        }

        //........................................................................................//

        private static AuthServiceFixture CreateFixture()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            var keyDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "glms-test-data-protection-keys"));
            keyDirectory.Create();
            services.AddDataProtection()
                .PersistKeysToFileSystem(keyDirectory);
            services.AddHttpContextAccessor();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "test-jwt-key-with-enough-length-1234567890",
                    ["Jwt:Issuer"] = "glms-test",
                    ["Jwt:Audience"] = "glms-test-client",
                    ["Jwt:ExpiresMinutes"] = "120"
                })
                .Build());

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IAuthService, AuthService>();

            var provider = services.BuildServiceProvider();
            var httpContext = new DefaultHttpContext { RequestServices = provider };
            provider.GetRequiredService<IHttpContextAccessor>().HttpContext = httpContext;

            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var role in ApplicationRoles.All)
            {
                roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
            }

            return new AuthServiceFixture(
                provider.GetRequiredService<IAuthService>(),
                provider.GetRequiredService<UserManager<ApplicationUser>>());
        }

        //........................................................................................//

        private sealed class AuthServiceFixture
        {
            public AuthServiceFixture(
                IAuthService authService,
                UserManager<ApplicationUser> userManager)
            {
                AuthService = authService;
                UserManager = userManager;
            }

            //........................................................................................//

            public IAuthService AuthService { get; }
            public UserManager<ApplicationUser> UserManager { get; }

            //........................................................................................//

            public async Task<ApplicationUser> CreateUserAsync(
                string email,
                string password,
                string role,
                bool isActive)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = "Test",
                    LastName = "User",
                    IsActive = isActive,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await UserManager.CreateAsync(user, password);
                Assert.True(createResult.Succeeded, string.Join(" ", createResult.Errors.Select(e => e.Description)));

                var roleResult = await UserManager.AddToRoleAsync(user, role);
                Assert.True(roleResult.Succeeded, string.Join(" ", roleResult.Errors.Select(e => e.Description)));

                return user;
            }

            //........................................................................................//
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
