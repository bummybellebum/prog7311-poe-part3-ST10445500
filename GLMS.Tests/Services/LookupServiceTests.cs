using GLMS.Data.Repositories;
using GLMS.Models;
using GLMS.Services;
using Moq;

namespace GLMS.Tests.Services
{
    public class LookupServiceTests
    {
        [Fact]
        public async Task GetContractStatusesAsync_ReturnsStatusesFromRepository()
        {
            // Arrange
            var contractStatusRepository = new Mock<IRepository<ContractStatus>>();
            var serviceRequestStatusRepository = new Mock<IRepository<ServiceRequestStatus>>();

            contractStatusRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<ContractStatus> { new ContractStatus { ContractStatusId = 2, StatusName = "Active" } });

            var service = new LookupService(contractStatusRepository.Object, serviceRequestStatusRepository.Object);

            // Act
            var result = await service.GetContractStatusesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Active", result[0].StatusName);
        }

        [Fact]
        public async Task GetServiceRequestStatusesAsync_ReturnsStatusesFromRepository()
        {
            // Arrange
            var contractStatusRepository = new Mock<IRepository<ContractStatus>>();
            var serviceRequestStatusRepository = new Mock<IRepository<ServiceRequestStatus>>();

            serviceRequestStatusRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<ServiceRequestStatus> { new ServiceRequestStatus { ServiceRequestStatusId = 1, StatusName = "Pending" } });

            var service = new LookupService(contractStatusRepository.Object, serviceRequestStatusRepository.Object);

            // Act
            var result = await service.GetServiceRequestStatusesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Pending", result[0].StatusName);
        }
    }
}
