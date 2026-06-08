using System.Net;
using System.Net.Http.Json;
using GLMS.Api.DTOs.ServiceRequests;
using GLMS.Tests.Helpers;

namespace GLMS.Tests.IntegrationTests
{
    public class ServiceRequestsApiTests
    {
        [Fact]
        public async Task PostServiceRequest_WithActiveContract_ReturnsCreated()
        {
            // Arrange
            await using var factory = new GlmsApiFactory();
            await factory.ResetDatabaseAsync();
            var client = factory.CreateClient();
            var dto = TestData.CreateServiceRequestDto();
            dto.ContractId = 1;
            dto.Description = "Integration service request";

            // Act
            var response = await client.PostAsJsonAsync("/api/service-requests", dto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<ServiceRequestDetailDto>();
            Assert.NotNull(created);
            Assert.Equal(dto.ContractId, created!.ContractId);
            Assert.Equal("Integration service request", created.Description);
            Assert.Equal("USD", created.OriginalCurrencyCode);
            Assert.Equal(18.5m, created.ExchangeRateToZAR);
            Assert.Equal(1850m, created.AmountZAR);
        }

        [Fact]
        public async Task PostServiceRequest_WithExpiredContract_ReturnsBadRequest()
        {
            // Arrange
            await using var factory = new GlmsApiFactory();
            await factory.ResetDatabaseAsync();
            var client = factory.CreateClient();
            var dto = TestData.CreateServiceRequestDto();
            dto.ContractId = 2;

            // Act
            var response = await client.PostAsJsonAsync("/api/service-requests", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("expired contract", body, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task PostServiceRequest_WithOnHoldContract_ReturnsBadRequest()
        {
            // Arrange
            await using var factory = new GlmsApiFactory();
            await factory.ResetDatabaseAsync();
            var client = factory.CreateClient();
            var dto = TestData.CreateServiceRequestDto();
            dto.ContractId = 3;

            // Act
            var response = await client.PostAsJsonAsync("/api/service-requests", dto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("on hold", body, StringComparison.OrdinalIgnoreCase);
        }
    }
}
