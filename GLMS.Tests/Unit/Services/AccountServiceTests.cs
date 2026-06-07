using GLMS.Api.Data;
using GLMS.Api.DTOs.Auth;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Microsoft.AspNetCore.DataProtection;
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
        public void ApplicationRoles_All_ContainsExactlySupportedRoles()
        {
            Assert.Equal(
                [ApplicationRoles.Admin, ApplicationRoles.ContractManager, ApplicationRoles.LogisticsManager],
                ApplicationRoles.All);
        }

        //........................................................................................//

        [Fact]
        public async Task CreateUserAsync_WithSupportedRole_CreatesConfirmedUser()
        {
            var fixture = CreateFixture();

            var result = await fixture.AccountService.CreateUserAsync(new CreateAdminUserDto
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
        public async Task CreateUserAsync_WithContractManagerRole_CreatesConfirmedUser()
        {
            var fixture = CreateFixture();

            var result = await fixture.AccountService.CreateUserAsync(new CreateAdminUserDto
            {
                FirstName = "Contract",
                LastName = "Manager",
                Email = "contracts@glms.local",
                Role = ApplicationRoles.ContractManager,
                TemporaryPassword = "Password123!",
                ConfirmTemporaryPassword = "Password123!",
                IsActive = true
            });

            var user = await fixture.UserManager.FindByEmailAsync("contracts@glms.local");

            Assert.True(result.Succeeded);
            Assert.NotNull(user);
            Assert.True(user.EmailConfirmed);
            Assert.True(await fixture.UserManager.IsInRoleAsync(user, ApplicationRoles.ContractManager));
        }

        //........................................................................................//

        [Fact]
        public async Task CreateUserAsync_WithUnsupportedRole_Fails()
        {
            var fixture = CreateFixture();

            var result = await fixture.AccountService.CreateUserAsync(new CreateAdminUserDto
            {
                Email = "unsupported@glms.local",
                Role = "UnsupportedRole",
                TemporaryPassword = "Password123!",
                ConfirmTemporaryPassword = "Password123!",
                IsActive = true
            });

            Assert.False(result.Succeeded);
            Assert.Contains("The selected role is not supported.", result.Errors);
        }

        //........................................................................................//

        [Fact]
        public async Task UpdateUserAsync_UpdatesRoleAndActiveStatus()
        {
            var fixture = CreateFixture();
            await fixture.CreateUserAsync("admin@glms.local", "Password123!", ApplicationRoles.Admin, isActive: true);
            var user = await fixture.CreateUserAsync("user@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: true);

            var result = await fixture.AccountService.UpdateUserAsync(new UpdateAdminUserDto
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

        [Fact]
        public async Task ResetPasswordAsync_WithTemporaryPassword_UpdatesPassword()
        {
            var fixture = CreateFixture();
            var user = await fixture.CreateUserAsync("user@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: true);

            var result = await fixture.AccountService.ResetPasswordAsync(new ResetAdminPasswordDto
            {
                UserId = user.Id,
                TemporaryPassword = "Temporary123!",
                ConfirmTemporaryPassword = "Temporary123!"
            });

            Assert.True(result.Succeeded);
            Assert.True(await fixture.UserManager.CheckPasswordAsync(user, "Temporary123!"));
        }

        //........................................................................................//

        private static AccountServiceFixture CreateFixture()
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

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IAccountService, AccountService>();

            var provider = services.BuildServiceProvider();

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

