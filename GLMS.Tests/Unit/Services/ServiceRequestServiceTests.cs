using GLMS.Api.Data.Repositories;
using GLMS.Api.DTOs.ServiceRequests;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Moq;

namespace GLMS.Tests.Unit.Services
{
    public class ServiceRequestServiceTests
    {
        [Fact]
        public async Task CreateServiceRequestAsync_ThrowsInvalidOperationException_WhenContractIsOnHold()
        {
            // Arrange
            var service = CreateService(
                out var serviceRequestRepository,
                out var contractRepository,
                out _);
            var request = CreateValidRequest();

            contractRepository
                .Setup(r => r.GetContractWithDetailsAsync(request.ContractId))
                .ReturnsAsync(CreateContractWithStatus(ContractStatusConstants.OnHoldName));

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateServiceRequestAsync(request));

            // Assert
            Assert.Equal("Cannot create a service request for a contract that is on hold.", exception.Message);
            serviceRequestRepository.Verify(r => r.AddAsync(It.IsAny<ServiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreateServiceRequestAsync_ThrowsInvalidOperationException_WhenContractIsExpired()
        {
            // Arrange
            var service = CreateService(
                out var serviceRequestRepository,
                out var contractRepository,
                out _);
            var request = CreateValidRequest();

            contractRepository
                .Setup(r => r.GetContractWithDetailsAsync(request.ContractId))
                .ReturnsAsync(CreateContractWithStatus(ContractStatusConstants.ExpiredName));

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateServiceRequestAsync(request));

            // Assert
            Assert.Equal("Cannot create a service request for an expired contract.", exception.Message);
            serviceRequestRepository.Verify(r => r.AddAsync(It.IsAny<ServiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreateServiceRequestAsync_CreatesServiceRequest_WhenContractIsActive()
        {
            // Arrange
            var service = CreateService(
                out var serviceRequestRepository,
                out var contractRepository,
                out var currencyExchangeService);
            var request = CreateValidRequest();

            contractRepository
                .Setup(r => r.GetContractWithDetailsAsync(request.ContractId))
                .ReturnsAsync(CreateContractWithStatus(ContractStatusConstants.ActiveName));

            currencyExchangeService
                .Setup(s => s.GetRateToZarAsync("USD", It.IsAny<CancellationToken>()))
                .ReturnsAsync(18.50m);

            // Act
            var result = await service.CreateServiceRequestAsync(request);

            // Assert
            Assert.Equal("USD", result.OriginalCurrencyCode);
            Assert.Equal(18.50m, result.ExchangeRateToZAR);
            Assert.Equal(1850.00m, result.AmountZAR);
            serviceRequestRepository.Verify(r => r.AddAsync(It.Is<ServiceRequest>(sr =>
                sr.ContractId == request.ContractId &&
                sr.RequestedByUserId == "user-1")), Times.Once);
            serviceRequestRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateServiceRequestAsync_RoundsConvertedAmountToTwoDecimals()
        {
            // Arrange
            var service = CreateService(
                out _,
                out var contractRepository,
                out var currencyExchangeService);
            var request = CreateValidRequest();
            request.AmountOriginal = 123.45m;
            request.OriginalCurrencyCode = "eur";

            contractRepository
                .Setup(r => r.GetContractWithDetailsAsync(request.ContractId))
                .ReturnsAsync(CreateContractWithStatus(ContractStatusConstants.ActiveName));

            currencyExchangeService
                .Setup(s => s.GetRateToZarAsync("EUR", It.IsAny<CancellationToken>()))
                .ReturnsAsync(20.123456m);

            // Act
            var result = await service.CreateServiceRequestAsync(request);

            // Assert
            Assert.Equal("EUR", result.OriginalCurrencyCode);
            Assert.Equal(2484.24m, result.AmountZAR);
        }

        [Fact]
        public async Task CreateServiceRequestAsync_ThrowsInvalidOperationException_WhenContractIsDraft()
        {
            // Arrange
            var service = CreateService(
                out var serviceRequestRepository,
                out var contractRepository,
                out _);
            var request = CreateValidRequest();

            contractRepository
                .Setup(r => r.GetContractWithDetailsAsync(request.ContractId))
                .ReturnsAsync(CreateContractWithStatus(ContractStatusConstants.DraftName));

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateServiceRequestAsync(request));

            // Assert
            Assert.Equal("Service requests can only be created for active contracts. This contract status is: Draft", exception.Message);
            serviceRequestRepository.Verify(r => r.AddAsync(It.IsAny<ServiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreateServiceRequestAsync_ThrowsInvalidOperationException_WhenContractStatusIsMissing()
        {
            // Arrange
            var service = CreateService(
                out var serviceRequestRepository,
                out var contractRepository,
                out _);
            var request = CreateValidRequest();

            contractRepository
                .Setup(r => r.GetContractWithDetailsAsync(request.ContractId))
                .ReturnsAsync(CreateContractWithoutStatus());

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateServiceRequestAsync(request));

            // Assert
            Assert.Equal("Service requests can only be created for active contracts. This contract status is: Unknown", exception.Message);
            serviceRequestRepository.Verify(r => r.AddAsync(It.IsAny<ServiceRequest>()), Times.Never);
        }

        private static ServiceRequestService CreateService(
            out Mock<IServiceRequestRepository> serviceRequestRepository,
            out Mock<IContractRepository> contractRepository,
            out Mock<ICurrencyExchangeService> currencyExchangeService)
        {
            serviceRequestRepository = new Mock<IServiceRequestRepository>();
            contractRepository = new Mock<IContractRepository>();
            currencyExchangeService = new Mock<ICurrencyExchangeService>();

            var currentUserService = new Mock<ICurrentUserService>();
            currentUserService.Setup(s => s.UserId).Returns("user-1");

            return new ServiceRequestService(
                serviceRequestRepository.Object,
                contractRepository.Object,
                currencyExchangeService.Object,
                currentUserService.Object);
        }

        private static CreateServiceRequestDto CreateValidRequest()
        {
            return new CreateServiceRequestDto
            {
                ContractId = 1,
                Description = "Need additional support services",
                AmountOriginal = 100m,
                OriginalCurrencyCode = "usd",
                ServiceRequestStatusId = 1
            };
        }

        private static Contract CreateContractWithStatus(string statusName)
        {
            return new Contract
            {
                ContractId = 1,
                ClientId = 1,
                Title = "Contract A",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddMonths(6),
                ContractStatusId = ContractStatusConstants.ActiveId,
                CreatedByUserId = "creator-1",
                ContractStatus = new ContractStatus
                {
                    ContractStatusId = ContractStatusConstants.ActiveId,
                    StatusName = statusName
                }
            };
        }

        private static Contract CreateContractWithoutStatus()
        {
            return new Contract
            {
                ContractId = 1,
                ClientId = 1,
                Title = "Contract A",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddMonths(6),
                ContractStatusId = ContractStatusConstants.DraftId,
                CreatedByUserId = "creator-1"
            };
        }
    }
}
