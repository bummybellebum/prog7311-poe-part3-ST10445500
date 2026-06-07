using System.Net;
using System.Net.Http.Json;
using GLMS.Api.Data;
using GLMS.Api.DTOs.Contracts;
using GLMS.Api.Models;
using GLMS.Tests.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace GLMS.Tests.IntegrationTests
{
    public class ContractsApiTests
    {
        [Fact]
        public async Task GetContracts_ReturnsOkAndJson()
        {
            // Arrange
            await using var factory = new GlmsApiFactory();
            await factory.ResetDatabaseAsync();
            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/contracts");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task PostContracts_WithValidContract_ReturnsCreated()
        {
            // Arrange
            await using var factory = new GlmsApiFactory();
            await factory.ResetDatabaseAsync();
            var client = factory.CreateClient();
            var dto = TestData.CreateContractDto();
            dto.Title = "Integration Contract";

            // Act
            var response = await client.PostAsJsonAsync("/api/contracts", dto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<ContractDetailDto>();
            Assert.NotNull(created);
            Assert.Equal("Integration Contract", created!.Title);
        }

        [Fact]
        public async Task PatchContractStatus_WithValidStatus_ReturnsOk()
        {
            // Arrange
            await using var factory = new GlmsApiFactory();
            await factory.ResetDatabaseAsync();
            var client = factory.CreateClient();

            // Act
            var response = await client.PatchAsJsonAsync("/api/contracts/1/status", new UpdateContractStatusDto
            {
                ContractStatusId = ContractStatusConstants.OnHoldId
            });

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<ContractDetailDto>();
            Assert.NotNull(updated);
            Assert.Equal(ContractStatusConstants.OnHoldId, updated!.ContractStatusId);
        }

        [Fact]
        public async Task CreateThenReadContract_ReturnsCreatedContract()
        {
            // Arrange
            await using var factory = new GlmsApiFactory();
            await factory.ResetDatabaseAsync();
            var client = factory.CreateClient();
            var dto = TestData.CreateContractDto();
            dto.Title = "Read After Create";

            // Act
            var createResponse = await client.PostAsJsonAsync("/api/contracts", dto);
            var created = await createResponse.Content.ReadFromJsonAsync<ContractDetailDto>();
            var readResponse = await client.GetAsync($"/api/contracts/{created!.ContractId}");

            // Assert
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
            var read = await readResponse.Content.ReadFromJsonAsync<ContractDetailDto>();
            Assert.NotNull(read);
            Assert.Equal("Read After Create", read!.Title);
        }

        private sealed class GlmsApiFactory : WebApplicationFactory<GLMS.Api.Program>
        {
            private readonly string _databaseName = Guid.NewGuid().ToString();

            public async Task ResetDatabaseAsync()
            {
                using var scope = Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.EnsureDeletedAsync();
                await TestDbContextFactory.SeedIntegrationDataAsync(context);
            }

            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.UseEnvironment("Testing");
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
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase(_databaseName));

                    services.PostConfigure<AuthenticationOptions>(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    });

                    services.AddAuthentication(TestAuthHandler.SchemeName)
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
                });
            }
        }
    }
}
