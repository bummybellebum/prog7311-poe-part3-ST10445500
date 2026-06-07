using GLMS.Api.Data;
using GLMS.Api.Data.Repositories;
using GLMS.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GLMS.Tests.UnitTests.Repositories
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task GetUsersAsync_ReturnsUsersOrderedByEmail()
        {
            // Arrange
            using var fixture = new IdentityFixture();
            await fixture.CreateUserAsync("z-user@glms.local");
            await fixture.CreateUserAsync("a-user@glms.local");

            // Act
            var result = await fixture.UserRepository.GetUsersAsync();

            // Assert
            Assert.Equal(["a-user@glms.local", "z-user@glms.local"], result.Select(user => user.Email).ToList());
        }

        [Fact]
        public async Task IsEmailUniqueAsync_WithExistingEmail_ReturnsFalse()
        {
            // Arrange
            using var fixture = new IdentityFixture();
            var user = await fixture.CreateUserAsync("user@glms.local");

            // Act
            var sameUserResult = await fixture.UserRepository.IsEmailUniqueAsync("user@glms.local", user.Id);
            var otherUserResult = await fixture.UserRepository.IsEmailUniqueAsync("user@glms.local");

            // Assert
            Assert.True(sameUserResult);
            Assert.False(otherUserResult);
        }

        private sealed class IdentityFixture : IDisposable
        {
            private readonly ServiceProvider _provider;

            public IdentityFixture()
            {
                var services = new ServiceCollection();
                services.AddLogging();
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
                services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();
                services.AddScoped<IUserRepository, UserRepository>();

                _provider = services.BuildServiceProvider();
                UserManager = _provider.GetRequiredService<UserManager<ApplicationUser>>();
                UserRepository = _provider.GetRequiredService<IUserRepository>();
            }

            public IUserRepository UserRepository { get; }
            private UserManager<ApplicationUser> UserManager { get; }

            public async Task<ApplicationUser> CreateUserAsync(string email)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = "Test",
                    LastName = "User",
                    IsActive = true
                };

                var result = await UserManager.CreateAsync(user, "Password123!");
                Assert.True(result.Succeeded, string.Join(" ", result.Errors.Select(error => error.Description)));

                return user;
            }

            public void Dispose()
            {
                _provider.Dispose();
            }
        }
    }
}
