using GLMS.Api.Data.Repositories;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Moq;

namespace GLMS.Tests.UnitTests.Services
{
    public class LookupServiceTests
    {
        [Fact]
        public async Task GetContractStatusLookupsAsync_ReturnsLookupDtos()
        {
            // Arrange
            var contractStatusRepository = new Mock<IRepository<ContractStatus>>();
            var serviceRequestStatusRepository = new Mock<IRepository<ServiceRequestStatus>>();
            contractStatusRepository
                .Setup(repository => repository.GetAllAsync())
                .ReturnsAsync(new List<ContractStatus>
                {
                    new() { ContractStatusId = 1, StatusName = "Draft" }
                });
            var service = new LookupService(contractStatusRepository.Object, serviceRequestStatusRepository.Object);

            // Act
            var result = await service.GetContractStatusLookupsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Draft", result[0].Name);
        }

        [Fact]
        public async Task GetClientLookupsAsync_WithoutClientRepository_ThrowsValidationError()
        {
            // Arrange
            var contractStatusRepository = new Mock<IRepository<ContractStatus>>();
            var serviceRequestStatusRepository = new Mock<IRepository<ServiceRequestStatus>>();
            var service = new LookupService(contractStatusRepository.Object, serviceRequestStatusRepository.Object);

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetClientLookupsAsync());

            // Assert
            Assert.Equal("Client lookup repository is not configured.", exception.Message);
        }
    }
}
