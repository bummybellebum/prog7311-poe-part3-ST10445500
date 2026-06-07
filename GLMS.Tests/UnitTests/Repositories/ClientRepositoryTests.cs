using GLMS.Api.Data.Repositories;
using GLMS.Tests.Helpers;

namespace GLMS.Tests.UnitTests.Repositories
{
    public class ClientRepositoryTests
    {
        [Fact]
        public async Task GetClientsAsync_WithSearchText_SearchesCompanyAndEmail()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContext();
            await TestDbContextFactory.SeedCoreDataAsync(context);
            var repository = new ClientRepository(context);

            // Act
            var companyResult = await repository.GetClientsAsync("global");
            var emailResult = await repository.GetClientsAsync("client1@glms");

            // Assert
            Assert.Single(companyResult);
            Assert.Equal(2, companyResult[0].ClientId);
            Assert.Single(emailResult);
            Assert.Equal(1, emailResult[0].ClientId);
        }

        [Fact]
        public async Task IsCompanyNameUniqueAsync_WithExistingName_ReturnsFalse()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContext();
            await TestDbContextFactory.SeedCoreDataAsync(context);
            var repository = new ClientRepository(context);

            // Act
            var result = await repository.IsCompanyNameUniqueAsync("Acme Logistics");

            // Assert
            Assert.False(result);
        }
    }
}
