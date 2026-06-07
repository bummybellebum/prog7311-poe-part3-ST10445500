using GLMS.Api.Data.Repositories;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Moq;

namespace GLMS.Tests.Unit.Services
{
    public class LookupServiceTests
    {
        [Fact]
        public async Task GetContractStatusLookupsAsync_ReturnsStatusLookupsFromRepository()
        {
            // Arrange
            var contractStatusRepository = new Mock<IRepository<ContractStatus>>();
            var serviceRequestStatusRepository = new Mock<IRepository<ServiceRequestStatus>>();

            contractStatusRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<ContractStatus> { new ContractStatus { ContractStatusId = 2, StatusName = "Active" } });

            var service = new LookupService(contractStatusRepository.Object, serviceRequestStatusRepository.Object);

            // Act
            var result = await service.GetContractStatusLookupsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(2, result[0].Id);
            Assert.Equal("Active", result[0].Name);
        }

        [Fact]
        public async Task GetServiceRequestStatusLookupsAsync_ReturnsStatusLookupsFromRepository()
        {
            // Arrange
            var contractStatusRepository = new Mock<IRepository<ContractStatus>>();
            var serviceRequestStatusRepository = new Mock<IRepository<ServiceRequestStatus>>();

            serviceRequestStatusRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<ServiceRequestStatus> { new ServiceRequestStatus { ServiceRequestStatusId = 1, StatusName = "Pending" } });

            var service = new LookupService(contractStatusRepository.Object, serviceRequestStatusRepository.Object);

            // Act
            var result = await service.GetServiceRequestStatusLookupsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Pending", result[0].Name);
        }
    }
}

