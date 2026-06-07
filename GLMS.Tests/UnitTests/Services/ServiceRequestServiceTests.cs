using GLMS.Api.Data.Repositories;
using GLMS.Api.DTOs.ServiceRequests;
using GLMS.Api.Models;
using GLMS.Api.Services;
using GLMS.Tests.Helpers;
using Moq;

namespace GLMS.Tests.UnitTests.Services
{
    public class ServiceRequestServiceTests
    {
        [Fact]
        public async Task CreateServiceRequestAsync_WithExpiredContract_BlocksServiceRequest()
        {
            // Arrange
            var service = CreateService(out var serviceRequestRepository, out var contractRepository, out _);
            var dto = TestData.CreateServiceRequestDto();
            contractRepository
                .Setup(repository => repository.GetContractWithDetailsAsync(dto.ContractId))
                .ReturnsAsync(TestData.Contract(statusId: ContractStatusConstants.ExpiredId, statusName: ContractStatusConstants.ExpiredName));

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateServiceRequestAsync(dto));

            // Assert
            Assert.Equal("Cannot create a service request for an expired contract.", exception.Message);
            serviceRequestRepository.Verify(repository => repository.AddAsync(It.IsAny<ServiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreateServiceRequestAsync_WithOnHoldContract_BlocksServiceRequest()
        {
            // Arrange
            var service = CreateService(out var serviceRequestRepository, out var contractRepository, out _);
            var dto = TestData.CreateServiceRequestDto();
            contractRepository
                .Setup(repository => repository.GetContractWithDetailsAsync(dto.ContractId))
                .ReturnsAsync(TestData.Contract(statusId: ContractStatusConstants.OnHoldId, statusName: ContractStatusConstants.OnHoldName));

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateServiceRequestAsync(dto));

            // Assert
            Assert.Equal("Cannot create a service request for a contract that is on hold.", exception.Message);
            serviceRequestRepository.Verify(repository => repository.AddAsync(It.IsAny<ServiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreateServiceRequestAsync_WithActiveContract_ReturnsCreatedServiceRequest()
        {
            // Arrange
            var service = CreateService(out var serviceRequestRepository, out var contractRepository, out var currencyService);
            var dto = TestData.CreateServiceRequestDto();
            contractRepository
                .Setup(repository => repository.GetContractWithDetailsAsync(dto.ContractId))
                .ReturnsAsync(TestData.Contract());
            currencyService
                .Setup(service => service.GetRateToZarAsync("USD", It.IsAny<CancellationToken>()))
                .ReturnsAsync(18.50m);

            // Act
            var result = await service.CreateServiceRequestAsync(dto);

            // Assert
            Assert.Equal("USD", result.OriginalCurrencyCode);
            Assert.Equal(18.50m, result.ExchangeRateToZAR);
            Assert.Equal(1850m, result.AmountZAR);
            serviceRequestRepository.Verify(repository => repository.AddAsync(It.Is<ServiceRequest>(request =>
                request.ContractId == dto.ContractId &&
                request.RequestedByUserId == TestData.UserId)), Times.Once);
            serviceRequestRepository.Verify(repository => repository.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateServiceRequestAsync_WithMissingDescription_ThrowsValidationError()
        {
            // Arrange
            var service = CreateService(out var serviceRequestRepository, out _, out _);
            var dto = TestData.CreateServiceRequestDto();
            dto.Description = "";

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateServiceRequestAsync(dto));

            // Assert
            Assert.StartsWith("Description is required.", exception.Message);
            serviceRequestRepository.Verify(repository => repository.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateServiceRequestAsync_WithUnsupportedCurrency_ThrowsValidationError()
        {
            // Arrange
            var service = CreateService(out var serviceRequestRepository, out var contractRepository, out var currencyService);
            var dto = TestData.CreateServiceRequestDto();
            contractRepository
                .Setup(repository => repository.GetContractWithDetailsAsync(dto.ContractId))
                .ReturnsAsync(TestData.Contract());
            currencyService
                .Setup(service => service.GetRateToZarAsync("USD", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Currency code is invalid."));

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateServiceRequestAsync(dto));

            // Assert
            Assert.Equal("Currency code is invalid.", exception.Message);
            serviceRequestRepository.Verify(repository => repository.SaveChangesAsync(), Times.Never);
        }

        private static ServiceRequestService CreateService(
            out Mock<IServiceRequestRepository> serviceRequestRepository,
            out Mock<IContractRepository> contractRepository,
            out Mock<ICurrencyExchangeService> currencyService)
        {
            serviceRequestRepository = new Mock<IServiceRequestRepository>();
            contractRepository = new Mock<IContractRepository>();
            currencyService = new Mock<ICurrencyExchangeService>();

            var accountService = new Mock<IAccountService>();
            accountService
                .Setup(service => service.GetCurrentUserId())
                .Returns(TestData.UserId);

            return new ServiceRequestService(
                serviceRequestRepository.Object,
                contractRepository.Object,
                currencyService.Object,
                accountService.Object);
        }
    }
}
