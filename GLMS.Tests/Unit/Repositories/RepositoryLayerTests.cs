using GLMS.Api.Data;
using GLMS.Api.Data.Repositories;
using GLMS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Tests.Unit.Repositories
{
    public class RepositoryLayerTests
    {
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
            Assert.NotNull(result[0].RequestedByUser);
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

            var statusActive = new ContractStatus { ContractStatusId = 2, StatusName = "Active" };
            var requestStatusPending = new ServiceRequestStatus { ServiceRequestStatusId = 1, StatusName = "Pending" };
            var user = new ApplicationUser { Id = "user-1", UserName = "user@glms.local", Email = "user@glms.local" };

            var contract = new Contract
            {
                ContractId = 1,
                ClientId = 1,
                ContractStatusId = 2,
                CreatedByUserId = "user-1",
                Title = "Support Contract",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddMonths(6),
                ServiceLevel = "Gold",
                Notes = "Core repository test contract"
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
                ServiceRequestStatusId = 1
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
                IsCurrent = true
            };

            context.Clients.Add(client);
            context.ContractStatuses.Add(statusActive);
            context.ServiceRequestStatuses.Add(requestStatusPending);
            context.Users.Add(user);
            context.Contracts.Add(contract);
            context.ServiceRequests.Add(request);
            context.ContractDocuments.Add(document);

            await context.SaveChangesAsync();
        }
    }
}

