using GLMS.Api.Data.Repositories;
using GLMS.Api.Models;
using GLMS.Tests.Helpers;

namespace GLMS.Tests.UnitTests.Repositories
{
    public class ContractRepositoryTests
    {
        [Fact]
        public async Task GetFilteredContractsAsync_WithStatusDateAndClient_ReturnsMatchingContracts()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContext();
            await TestDbContextFactory.SeedCoreDataAsync(context);
            var repository = new ContractRepository(context);

            // Act
            var result = await repository.GetFilteredContractsAsync(
                statusId: ContractStatusConstants.ActiveId,
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
        public async Task GetContractsByDateRangeAsync_ReturnsContractsOrderedByStartDate()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContext();
            await TestDbContextFactory.SeedCoreDataAsync(context);
            var repository = new ContractRepository(context);

            // Act
            var result = await repository.GetContractsByDateRangeAsync(
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 31));

            // Assert
            Assert.Equal([1, 3], result.Select(contract => contract.ContractId).ToList());
        }
    }
}
