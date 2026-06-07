using GLMS.Api.Controllers;
using GLMS.Api.DTOs.ServiceRequests;
using GLMS.Api.Services;
using GLMS.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GLMS.Tests.UnitTests.Controllers
{
    public class ServiceRequestsControllerTests
    {
        [Fact]
        public async Task GetServiceRequest_WithMissingRequest_ReturnsNotFound()
        {
            // Arrange
            var serviceRequestService = new Mock<IServiceRequestService>();
            var currencyService = new Mock<ICurrencyExchangeService>();
            serviceRequestService
                .Setup(service => service.GetServiceRequestAsync(99))
                .ReturnsAsync((ServiceRequestDetailDto?)null);
            var controller = new ServiceRequestsController(serviceRequestService.Object, currencyService.Object);

            // Act
            var result = await controller.GetServiceRequest(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var serviceRequestService = new Mock<IServiceRequestService>();
            var currencyService = new Mock<ICurrencyExchangeService>();
            var dto = TestData.CreateServiceRequestDto();
            serviceRequestService
                .Setup(service => service.CreateServiceRequestAsync(dto))
                .ReturnsAsync(new ServiceRequestDetailDto
                {
                    ServiceRequestId = 7,
                    ContractId = dto.ContractId,
                    Description = dto.Description
                });
            var controller = new ServiceRequestsController(serviceRequestService.Object, currencyService.Object);

            // Act
            var result = await controller.Create(dto);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(ServiceRequestsController.GetServiceRequest), created.ActionName);
            Assert.IsType<ServiceRequestDetailDto>(created.Value);
        }

        [Fact]
        public async Task GetExchangeRate_WithMissingCurrency_ThrowsValidationError()
        {
            // Arrange
            var serviceRequestService = new Mock<IServiceRequestService>();
            var currencyService = new Mock<ICurrencyExchangeService>();
            var controller = new ServiceRequestsController(serviceRequestService.Object, currencyService.Object);

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => controller.GetExchangeRate("", CancellationToken.None));

            // Assert
            Assert.Equal("Currency code is required.", exception.Message);
        }
    }
}
