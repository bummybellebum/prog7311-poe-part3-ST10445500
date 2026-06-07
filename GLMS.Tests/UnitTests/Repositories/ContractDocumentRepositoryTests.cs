using GLMS.Api.Data.Repositories;
using GLMS.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Tests.UnitTests.Repositories
{
    public class ContractDocumentRepositoryTests
    {
        [Fact]
        public async Task GetCurrentDocumentByContractAsync_ReturnsCurrentDocument()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContext();
            await TestDbContextFactory.SeedCoreDataAsync(context);
            var repository = new ContractDocumentRepository(context);

            // Act
            var result = await repository.GetCurrentDocumentByContractAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result!.IsCurrent);
            Assert.Equal(2, result.ContractDocumentId);
            Assert.NotNull(result.UploadedByUser);
        }

        [Fact]
        public async Task MarkCurrentDocumentsInactiveAsync_MarksCurrentDocumentsInactive()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContext();
            await TestDbContextFactory.SeedCoreDataAsync(context);
            var repository = new ContractDocumentRepository(context);

            // Act
            var updatedCount = await repository.MarkCurrentDocumentsInactiveAsync(1);

            // Assert
            Assert.Equal(1, updatedCount);
            Assert.False(await context.ContractDocuments.AnyAsync(document => document.ContractId == 1 && document.IsCurrent));
        }
    }
}
