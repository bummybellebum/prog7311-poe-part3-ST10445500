using GLMS.Api.Data.Repositories;
using GLMS.Tests.Helpers;

namespace GLMS.Tests.UnitTests.Repositories
{
    public class ServiceRequestRepositoryTests
    {
        [Fact]
        public async Task GetFilteredServiceRequestsAsync_WithContractAndStatus_ReturnsMatchingRequests()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContext();
            await TestDbContextFactory.SeedCoreDataAsync(context);
            var repository = new ServiceRequestRepository(context);

            // Act
            var result = await repository.GetFilteredServiceRequestsAsync(contractId: 1, statusId: 1);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].ServiceRequestId);
            Assert.NotNull(result[0].Contract);
            Assert.NotNull(result[0].Contract.Client);
            Assert.NotNull(result[0].ServiceRequestStatus);
        }

        [Fact]
        public async Task GetServiceRequestsByStatusAsync_ReturnsNewestMatchingRequestsFirst()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContext();
            await TestDbContextFactory.SeedCoreDataAsync(context);
            var repository = new ServiceRequestRepository(context);

            // Act
            var result = await repository.GetServiceRequestsByStatusAsync(4);

            // Assert
            Assert.Single(result);
            Assert.Equal(2, result[0].ServiceRequestId);
        }
    }
}
