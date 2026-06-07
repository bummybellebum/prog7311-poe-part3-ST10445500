using GLMS.Web.Data.Repositories;
using GLMS.Web.Models;
using GLMS.Web.Services;
using Moq;

namespace GLMS.Tests.Unit.Services
{
    public class ServiceRequestServiceTests
    {
        [Fact]
        public async Task CreateAsync_ThrowsInvalidOperationException_WhenContractIsOnHold()
        {
            // Arrange
            var serviceRequestRepository = new Mock<IServiceRequestRepository>();
            var contractRepository = new Mock<IContractRepository>();
            var currencyExchangeService = new Mock<ICurrencyExchangeService>();

            var service = new ServiceRequestService(serviceRequestRepository.Object, contractRepository.Object, currencyExchangeService.Object);
            var request = CreateValidRequest();

            contractRepository
                .Setup(r => r.GetContractWithDetailsAsync(request.ContractId))
                .ReturnsAsync(CreateContractWithStatus("On Hold"));

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request));

            // Assert
            Assert.Equal("Cannot create a service request for a contract that is on hold.", exception.Message);
            serviceRequestRepository.Verify(r => r.AddAsync(It.IsAny<ServiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ThrowsInvalidOperationException_WhenContractIsExpired()
        {
            // Arrange
            var serviceRequestRepository = new Mock<IServiceRequestRepository>();
            var contractRepository = new Mock<IContractRepository>();
            var currencyExchangeService = new Mock<ICurrencyExchangeService>();

            var service = new ServiceRequestService(serviceRequestRepository.Object, contractRepository.Object, currencyExchangeService.Object);
            var request = CreateValidRequest();

            contractRepository
                .Setup(r => r.GetContractWithDetailsAsync(request.ContractId))
                .ReturnsAsync(CreateContractWithStatus("Expired"));

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request));

            // Assert
            Assert.Equal("Cannot create a service request for an expired contract.", exception.Message);
            serviceRequestRepository.Verify(r => r.AddAsync(It.IsAny<ServiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_CreatesServiceRequest_WhenContractIsActive()
        {
            // Arrange
            var serviceRequestRepository = new Mock<IServiceRequestRepository>();
            var contractRepository = new Mock<IContractRepository>();
            var currencyExchangeService = new Mock<ICurrencyExchangeService>();

            var service = new ServiceRequestService(serviceRequestRepository.Object, contractRepository.Object, currencyExchangeService.Object);
            var request = CreateValidRequest();

            contractRepository
                .Setup(r => r.GetContractWithDetailsAsync(request.ContractId))
                .ReturnsAsync(CreateContractWithStatus("Active"));

            currencyExchangeService
                .Setup(s => s.GetRateToZarAsync("USD", It.IsAny<CancellationToken>()))
                .ReturnsAsync(18.50m);

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.Equal("USD", result.OriginalCurrencyCode);
            Assert.Equal(18.50m, result.ExchangeRateToZAR);
            Assert.Equal(1850.00m, result.AmountZAR);
            serviceRequestRepository.Verify(r => r.AddAsync(It.Is<ServiceRequest>(sr => sr == request)), Times.Once);
            serviceRequestRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_RoundsConvertedAmountToTwoDecimals()
        {
            // Arrange
            var serviceRequestRepository = new Mock<IServiceRequestRepository>();
            var contractRepository = new Mock<IContractRepository>();
            var currencyExchangeService = new Mock<ICurrencyExchangeService>();

            var service = new ServiceRequestService(serviceRequestRepository.Object, contractRepository.Object, currencyExchangeService.Object);
            var request = CreateValidRequest();
            request.AmountOriginal = 123.45m;
            request.OriginalCurrencyCode = "eur";

            contractRepository
                .Setup(r => r.GetContractWithDetailsAsync(request.ContractId))
                .ReturnsAsync(CreateContractWithStatus("Active"));

            currencyExchangeService
                .Setup(s => s.GetRateToZarAsync("EUR", It.IsAny<CancellationToken>()))
                .ReturnsAsync(20.123456m);

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.Equal("EUR", result.OriginalCurrencyCode);
            Assert.Equal(2484.24m, result.AmountZAR);
        }

        private static ServiceRequest CreateValidRequest()
        {
            return new ServiceRequest
            {
                ContractId = 1,
                RequestedByUserId = "user-1",
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
                ContractStatusId = 2,
                CreatedByUserId = "creator-1",
                ContractStatus = new ContractStatus
                {
                    ContractStatusId = 2,
                    StatusName = statusName
                }
            };
        }
    }
}
