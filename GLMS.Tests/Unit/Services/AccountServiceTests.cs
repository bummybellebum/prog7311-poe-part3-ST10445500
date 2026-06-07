using System.Security.Claims;
using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Services;
using GLMS.Web.ViewModels.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

//ST10445500 - PROG7311 - GLMS POE
//AccountServiceTests

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Tests.Unit.Services
{
    public class AccountServiceTests
    {
        [Fact]
        public async Task LoginAsync_WithValidCredentials_Succeeds()
        {
            var fixture = CreateFixture();
            await fixture.CreateUserAsync("user@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: true);

            var result = await fixture.AccountService.LoginAsync(new LoginViewModel
            {
                Email = "user@glms.local",
                Password = "Password123!"
            });

            Assert.True(result.Succeeded);
        }

        //........................................................................................//

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_Fails()
        {
            var fixture = CreateFixture();
            await fixture.CreateUserAsync("user@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: true);

            var result = await fixture.AccountService.LoginAsync(new LoginViewModel
            {
                Email = "user@glms.local",
                Password = "WrongPassword123!"
            });

            Assert.False(result.Succeeded);
            Assert.Contains("Invalid login attempt.", result.Errors);
        }

        //........................................................................................//

        [Fact]
        public async Task LoginAsync_WithInactiveUser_FailsBeforeSignIn()
        {
            var fixture = CreateFixture();
            await fixture.CreateUserAsync("user@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: false);

            var result = await fixture.AccountService.LoginAsync(new LoginViewModel
            {
                Email = "user@glms.local",
                Password = "Password123!"
            });

            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, error => error.Contains("inactive", StringComparison.OrdinalIgnoreCase));
        }

        //........................................................................................//

        [Fact]
        public async Task CreateUserAsync_WithSupportedRole_CreatesConfirmedUser()
        {
            var fixture = CreateFixture();

            var result = await fixture.AccountService.CreateUserAsync(new AdminUserCreateViewModel
            {
                FirstName = "Logistics",
                LastName = "Manager",
                Email = "logistics@glms.local",
                Role = ApplicationRoles.LogisticsManager,
                TemporaryPassword = "Password123!",
                ConfirmTemporaryPassword = "Password123!",
                IsActive = true
            });

            var user = await fixture.UserManager.FindByEmailAsync("logistics@glms.local");

            Assert.True(result.Succeeded);
            Assert.NotNull(user);
            Assert.True(user.EmailConfirmed);
            Assert.True(await fixture.UserManager.IsInRoleAsync(user, ApplicationRoles.LogisticsManager));
        }

        //........................................................................................//

        [Fact]
        public async Task UpdateUserAsync_UpdatesRoleAndActiveStatus()
        {
            var fixture = CreateFixture();
            await fixture.CreateUserAsync("admin@glms.local", "Password123!", ApplicationRoles.Admin, isActive: true);
            var user = await fixture.CreateUserAsync("user@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: true);

            var result = await fixture.AccountService.UpdateUserAsync(new AdminUserEditViewModel
            {
                UserId = user.Id,
                FirstName = "Updated",
                LastName = "User",
                Email = "updated@glms.local",
                Role = ApplicationRoles.Admin,
                IsActive = false
            });

            var updated = await fixture.UserManager.FindByIdAsync(user.Id);

            Assert.True(result.Succeeded);
            Assert.NotNull(updated);
            Assert.Equal("updated@glms.local", updated.Email);
            Assert.False(updated.IsActive);
            Assert.True(await fixture.UserManager.IsInRoleAsync(updated, ApplicationRoles.Admin));
            Assert.False(await fixture.UserManager.IsInRoleAsync(updated, ApplicationRoles.LogisticsManager));
        }

        //........................................................................................//

        [Fact]
        public async Task ChangePasswordAsync_WithValidCurrentPassword_Succeeds()
        {
            var fixture = CreateFixture();
            var user = await fixture.CreateUserAsync("user@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: true);

            var result = await fixture.AccountService.ChangePasswordAsync(CreatePrincipal(user), new ChangePasswordViewModel
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

            var result = await fixture.AccountService.ChangePasswordAsync(CreatePrincipal(user), new ChangePasswordViewModel
            {
                CurrentPassword = "WrongPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            });

            Assert.False(result.Succeeded);
        }

        //........................................................................................//

        [Fact]
        public async Task ResetPasswordAsync_WithTemporaryPassword_UpdatesPassword()
        {
            var fixture = CreateFixture();
            var user = await fixture.CreateUserAsync("user@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: true);

            var result = await fixture.AccountService.ResetPasswordAsync(new AdminResetPasswordViewModel
            {
                UserId = user.Id,
                TemporaryPassword = "Temporary123!",
                ConfirmTemporaryPassword = "Temporary123!"
            });

            Assert.True(result.Succeeded);
            Assert.True(await fixture.UserManager.CheckPasswordAsync(user, "Temporary123!"));
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

        private static AccountServiceFixture CreateFixture()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDataProtection();
            services.AddHttpContextAccessor();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IAccountService, AccountService>();

            var provider = services.BuildServiceProvider();
            var httpContext = new DefaultHttpContext { RequestServices = provider };
            provider.GetRequiredService<IHttpContextAccessor>().HttpContext = httpContext;

            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var role in ApplicationRoles.All)
            {
                roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
            }

            return new AccountServiceFixture(
                provider.GetRequiredService<IAccountService>(),
                provider.GetRequiredService<UserManager<ApplicationUser>>());
        }

        //........................................................................................//

        private sealed class AccountServiceFixture
        {
            public AccountServiceFixture(
                IAccountService accountService,
                UserManager<ApplicationUser> userManager)
            {
                AccountService = accountService;
                UserManager = userManager;
            }

            //........................................................................................//

            public IAccountService AccountService { get; }
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
