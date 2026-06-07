using GLMS.Api.Data;
using GLMS.Api.Data.Repositories;
using GLMS.Api.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GLMS.Tests.Unit.Repositories
{
    public class RepositoryLayerTests
    {
        [Fact]
        public async Task Repository_BaseHelpers_FindFirstAndExists()
        {
            // Arrange
            using var context = CreateContext();
            await SeedCoreDataAsync(context);
            var repository = new Repository<Client>(context);

            // Act
            var found = await repository.FindAsync(1);
            var first = await repository.FirstOrDefaultAsync(c => c.CompanyName == "Global Freight");
            var exists = await repository.ExistsAsync(c => c.Email == "contact@acme.com");

            // Assert
            Assert.NotNull(found);
            Assert.Equal("Acme Corp", found!.CompanyName);
            Assert.NotNull(first);
            Assert.Equal(2, first!.ClientId);
            Assert.True(exists);
        }

        //........................................................................................//

        [Fact]
        public async Task ClientRepository_GetClientWithContractsAsync_LoadsContractsAndStatus()
        {
            // Arrange
            using var context = CreateContext();
            await SeedCoreDataAsync(context);
            var repository = new ClientRepository(context);

            // Act
            var result = await repository.GetClientWithContractsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result!.Contracts);
            Assert.All(result.Contracts, contract => Assert.NotNull(contract.ContractStatus));
        }

        [Fact]
        public async Task ContractRepository_GetContractWithDetailsAsync_LoadsRelatedData()
        {
            // Arrange
            using var context = CreateContext();
            await SeedCoreDataAsync(context);
            var repository = new ContractRepository(context);

            // Act
            var result = await repository.GetContractWithDetailsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result!.Client);
            Assert.NotNull(result.ContractStatus);
            Assert.NotNull(result.CreatedByUser);
        }

        [Fact]
        public async Task ContractRepository_GetFilteredContractsAsync_AppliesCombinedFilters()
        {
            // Arrange
            using var context = CreateContext();
            await SeedCoreDataAsync(context);
            var repository = new ContractRepository(context);

            // Act
            var result = await repository.GetFilteredContractsAsync(
                statusId: 2,
                startDate: new DateTime(2026, 1, 1),
                endDate: new DateTime(2026, 1, 31),
                clientId: 1);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].ContractId);
            Assert.NotNull(result[0].Client);
            Assert.NotNull(result[0].ContractStatus);
        }

        [Fact]
        public async Task ClientRepository_GetClientsAsync_SearchesCompanyAndEmail()
        {
            // Arrange
            using var context = CreateContext();
            await SeedCoreDataAsync(context);
            var repository = new ClientRepository(context);

            // Act
            var companyResult = await repository.GetClientsAsync("global");
            var emailResult = await repository.GetClientsAsync("contact@acme");

            // Assert
            Assert.Single(companyResult);
            Assert.Equal(2, companyResult[0].ClientId);
            Assert.Single(emailResult);
            Assert.Equal(1, emailResult[0].ClientId);
        }

        [Fact]
        public async Task ServiceRequestRepository_GetServiceRequestsByStatusAsync_ReturnsExpectedRequests()
        {
            // Arrange
            using var context = CreateContext();
            await SeedCoreDataAsync(context);
            var repository = new ServiceRequestRepository(context);

            // Act
            var result = await repository.GetServiceRequestsByStatusAsync(1);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].ServiceRequestId);
            Assert.NotNull(result[0].Contract);
            Assert.NotNull(result[0].Contract.Client);
            Assert.NotNull(result[0].ServiceRequestStatus);
        }

        [Fact]
        public async Task ServiceRequestRepository_GetFilteredServiceRequestsAsync_AppliesContractAndStatusFilters()
        {
            // Arrange
            using var context = CreateContext();
            await SeedCoreDataAsync(context);
            var repository = new ServiceRequestRepository(context);

            // Act
            var result = await repository.GetFilteredServiceRequestsAsync(contractId: 1, statusId: 1);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].ServiceRequestId);
            Assert.NotNull(result[0].Contract.Client);
        }

        [Fact]
        public async Task ContractDocumentRepository_GetCurrentDocumentByContractAsync_ReturnsCurrentDocumentWithUploader()
        {
            // Arrange
            using var context = CreateContext();
            await SeedCoreDataAsync(context);
            var repository = new ContractDocumentRepository(context);

            // Act
            var result = await repository.GetCurrentDocumentByContractAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result!.IsCurrent);
            Assert.NotNull(result.UploadedByUser);
        }

        [Fact]
        public async Task ContractDocumentRepository_GetDocumentsByContractAsync_ReturnsHistoryNewestFirst()
        {
            // Arrange
            using var context = CreateContext();
            await SeedCoreDataAsync(context);
            var repository = new ContractDocumentRepository(context);

            // Act
            var result = await repository.GetDocumentsByContractAsync(1);

            // Assert
            Assert.Equal([2, 1], result.Select(d => d.ContractDocumentId).ToList());
            Assert.All(result, document => Assert.NotNull(document.UploadedByUser));
        }

        [Fact]
        public async Task ContractDocumentRepository_MarkCurrentDocumentsInactiveAsync_MarksCurrentDocumentsInactive()
        {
            // Arrange
            using var context = CreateContext();
            await SeedCoreDataAsync(context);
            var repository = new ContractDocumentRepository(context);

            // Act
            var updatedCount = await repository.MarkCurrentDocumentsInactiveAsync(1);

            // Assert
            Assert.Equal(1, updatedCount);
            Assert.False(await context.ContractDocuments.AnyAsync(d => d.ContractId == 1 && d.IsCurrent));
        }

        //........................................................................................//

        [Fact]
        public async Task UserRepository_GetUsersAsync_ReturnsUsersOrderedByEmail()
        {
            // Arrange
            using var fixture = CreateIdentityFixture();
            await fixture.CreateUserAsync("z-user@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: true);
            await fixture.CreateUserAsync("a-user@glms.local", "Password123!", ApplicationRoles.Admin, isActive: true);

            // Act
            var users = await fixture.UserRepository.GetUsersAsync();

            // Assert
            Assert.Equal(["a-user@glms.local", "z-user@glms.local"], users.Select(u => u.Email).ToList());
        }

        //........................................................................................//

        [Fact]
        public async Task UserRepository_ChangeRoleAsync_ReplacesExistingRole()
        {
            // Arrange
            using var fixture = CreateIdentityFixture();
            var user = await fixture.CreateUserAsync("user@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: true);

            // Act
            var result = await fixture.UserRepository.ChangeRoleAsync(user, ApplicationRoles.Admin);
            var roles = await fixture.UserManager.GetRolesAsync(user);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal([ApplicationRoles.Admin], roles);
        }

        //........................................................................................//

        [Fact]
        public async Task UserRepository_IsEmailUniqueAsync_ExcludesCurrentUser()
        {
            // Arrange
            using var fixture = CreateIdentityFixture();
            var user = await fixture.CreateUserAsync("user@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: true);
            await fixture.CreateUserAsync("other@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: true);

            // Act
            var sameUserEmailIsUnique = await fixture.UserRepository.IsEmailUniqueAsync("user@glms.local", user.Id);
            var otherUserEmailIsUnique = await fixture.UserRepository.IsEmailUniqueAsync("other@glms.local", user.Id);

            // Assert
            Assert.True(sameUserEmailIsUnique);
            Assert.False(otherUserEmailIsUnique);
        }

        //........................................................................................//

        [Fact]
        public async Task UserRepository_ResetAndChangePasswordAsync_UpdatesPassword()
        {
            // Arrange
            using var fixture = CreateIdentityFixture();
            var user = await fixture.CreateUserAsync("user@glms.local", "Password123!", ApplicationRoles.LogisticsManager, isActive: true);

            // Act
            var resetResult = await fixture.UserRepository.ResetPasswordAsync(user, "Temporary123!");
            var changeResult = await fixture.UserRepository.ChangePasswordAsync(user, "Temporary123!", "Changed123!");

            // Assert
            Assert.True(resetResult.Succeeded);
            Assert.True(changeResult.Succeeded);
            Assert.True(await fixture.UserManager.CheckPasswordAsync(user, "Changed123!"));
        }

        //........................................................................................//

        private static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private static async Task SeedCoreDataAsync(ApplicationDbContext context)
        {
            var client = new Client
            {
                ClientId = 1,
                CompanyName = "Acme Corp",
                Email = "contact@acme.com",
                Phone = "0123456789",
                Region = "Gauteng",
                Country = "South Africa"
            };

            var secondClient = new Client
            {
                ClientId = 2,
                CompanyName = "Global Freight",
                Email = "ops@global.example",
                Phone = "0987654321",
                Region = "Western Cape",
                Country = "South Africa"
            };

            var statusDraft = new ContractStatus { ContractStatusId = 1, StatusName = "Draft" };
            var statusActive = new ContractStatus { ContractStatusId = 2, StatusName = "Active" };
            var requestStatusPending = new ServiceRequestStatus { ServiceRequestStatusId = 1, StatusName = "Pending" };
            var requestStatusCompleted = new ServiceRequestStatus { ServiceRequestStatusId = 4, StatusName = "Completed" };
            var user = new ApplicationUser { Id = "user-1", UserName = "user@glms.local", Email = "user@glms.local" };

            var contract = new Contract
            {
                ContractId = 1,
                ClientId = 1,
                ContractStatusId = 2,
                CreatedByUserId = "user-1",
                Title = "Support Contract",
                StartDate = new DateTime(2026, 1, 10),
                EndDate = new DateTime(2026, 7, 10),
                ServiceLevel = "Gold",
                Notes = "Core repository test contract",
                CreatedAt = new DateTime(2026, 1, 10)
            };

            var secondContract = new Contract
            {
                ContractId = 2,
                ClientId = 2,
                ContractStatusId = 2,
                CreatedByUserId = "user-1",
                Title = "Warehouse Contract",
                StartDate = new DateTime(2026, 2, 15),
                EndDate = new DateTime(2026, 8, 15),
                ServiceLevel = "Silver",
                Notes = "Outside January filter",
                CreatedAt = new DateTime(2026, 2, 15)
            };

            var thirdContract = new Contract
            {
                ContractId = 3,
                ClientId = 1,
                ContractStatusId = 1,
                CreatedByUserId = "user-1",
                Title = "Draft Contract",
                StartDate = new DateTime(2026, 1, 20),
                EndDate = new DateTime(2026, 4, 20),
                ServiceLevel = "Bronze",
                Notes = "Wrong status",
                CreatedAt = new DateTime(2026, 1, 20)
            };

            var request = new ServiceRequest
            {
                ServiceRequestId = 1,
                ContractId = 1,
                RequestedByUserId = "user-1",
                Description = "Test request",
                AmountOriginal = 100m,
                OriginalCurrencyCode = "USD",
                ExchangeRateToZAR = 18m,
                AmountZAR = 1800m,
                ServiceRequestStatusId = 1,
                RequestedAt = new DateTime(2026, 1, 11)
            };

            var secondRequest = new ServiceRequest
            {
                ServiceRequestId = 2,
                ContractId = 2,
                RequestedByUserId = "user-1",
                Description = "Completed request",
                AmountOriginal = 50m,
                OriginalCurrencyCode = "EUR",
                ExchangeRateToZAR = 20m,
                AmountZAR = 1000m,
                ServiceRequestStatusId = 4,
                RequestedAt = new DateTime(2026, 2, 16)
            };

            var document = new ContractDocument
            {
                ContractDocumentId = 1,
                ContractId = 1,
                DocumentType = "Signed Agreement",
                OriginalFileName = "agreement.pdf",
                StoredFileName = "agreement-1.pdf",
                FilePath = "uploads/agreement-1.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = 100,
                UploadedByUserId = "user-1",
                IsCurrent = false,
                UploadedAt = new DateTime(2026, 1, 11)
            };

            var currentDocument = new ContractDocument
            {
                ContractDocumentId = 2,
                ContractId = 1,
                DocumentType = "Signed Agreement",
                OriginalFileName = "agreement-current.pdf",
                StoredFileName = "agreement-2.pdf",
                FilePath = "uploads/agreement-2.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = 150,
                UploadedByUserId = "user-1",
                IsCurrent = true,
                UploadedAt = new DateTime(2026, 1, 12)
            };

            context.Clients.Add(client);
            context.Clients.Add(secondClient);
            context.ContractStatuses.AddRange(statusDraft, statusActive);
            context.ServiceRequestStatuses.AddRange(requestStatusPending, requestStatusCompleted);
            context.Users.Add(user);
            context.Contracts.AddRange(contract, secondContract, thirdContract);
            context.ServiceRequests.AddRange(request, secondRequest);
            context.ContractDocuments.AddRange(document, currentDocument);

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        //........................................................................................//

        private static IdentityFixture CreateIdentityFixture()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var keyDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "glms-test-data-protection-keys"));
            keyDirectory.Create();
            services.AddDataProtection()
                .PersistKeysToFileSystem(keyDirectory);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IUserRepository, UserRepository>();

            var provider = services.BuildServiceProvider();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in ApplicationRoles.All)
            {
                roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
            }

            return new IdentityFixture(
                provider,
                provider.GetRequiredService<IUserRepository>(),
                provider.GetRequiredService<UserManager<ApplicationUser>>());
        }

        //........................................................................................//

        private sealed class IdentityFixture : IDisposable
        {
            private readonly ServiceProvider _provider;

            public IdentityFixture(
                ServiceProvider provider,
                IUserRepository userRepository,
                UserManager<ApplicationUser> userManager)
            {
                _provider = provider;
                UserRepository = userRepository;
                UserManager = userManager;
            }

            public IUserRepository UserRepository { get; }
            public UserManager<ApplicationUser> UserManager { get; }

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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createResult = await UserManager.CreateAsync(user, password);
                Assert.True(createResult.Succeeded, string.Join(" ", createResult.Errors.Select(e => e.Description)));

                var roleResult = await UserManager.AddToRoleAsync(user, role);
                Assert.True(roleResult.Succeeded, string.Join(" ", roleResult.Errors.Select(e => e.Description)));

                return user;
            }

            public void Dispose()
            {
                _provider.Dispose();
            }
        }
    }
}

