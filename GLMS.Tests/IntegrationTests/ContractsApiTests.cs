using System.Net;
using System.Net.Http.Json;
using GLMS.Api.DTOs.Contracts;
using GLMS.Api.Models;
using GLMS.Tests.Helpers;

namespace GLMS.Tests.IntegrationTests
{
    public class ContractsApiTests
    {
        [Fact]
        public async Task GetContracts_ReturnsOkAndJson()
        {
            // Arrange
            await using var factory = new GlmsApiFactory();
            await factory.ResetDatabaseAsync();
            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/contracts");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task GetContracts_WithFilters_ReturnsOkAndJson()
        {
            // Arrange
            await using var factory = new GlmsApiFactory();
            await factory.ResetDatabaseAsync();
            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/contracts?statusId=2&startDate=2026-01-01&endDate=2026-01-31");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
            var contracts = await response.Content.ReadFromJsonAsync<List<ContractListDto>>();
            Assert.NotNull(contracts);
            var contract = Assert.Single(contracts!);
            Assert.Equal(ContractStatusConstants.ActiveId, contract.ContractStatusId);
            Assert.Equal("Active Integration Contract", contract.Title);
        }

        [Fact]
        public async Task PostContracts_WithValidContract_ReturnsCreated()
        {
            // Arrange
            await using var factory = new GlmsApiFactory();
            await factory.ResetDatabaseAsync();
            var client = factory.CreateClient();
            var dto = TestData.CreateContractDto();
            dto.Title = "Integration Contract";

            // Act
            var response = await client.PostAsJsonAsync("/api/contracts", dto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<ContractDetailDto>();
            Assert.NotNull(created);
            Assert.Equal("Integration Contract", created!.Title);
        }

        [Fact]
        public async Task PatchContractStatus_WithValidStatus_ReturnsOk()
        {
            // Arrange
            await using var factory = new GlmsApiFactory();
            await factory.ResetDatabaseAsync();
            var client = factory.CreateClient();

            // Act
            var response = await client.PatchAsJsonAsync("/api/contracts/1/status", new UpdateContractStatusDto
            {
                ContractStatusId = ContractStatusConstants.OnHoldId
            });

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<ContractDetailDto>();
            Assert.NotNull(updated);
            Assert.Equal(ContractStatusConstants.OnHoldId, updated!.ContractStatusId);
        }

        [Fact]
        public async Task CreateThenReadContract_ReturnsCreatedContract()
        {
            // Arrange
            await using var factory = new GlmsApiFactory();
            await factory.ResetDatabaseAsync();
            var client = factory.CreateClient();
            var dto = TestData.CreateContractDto();
            dto.Title = "Read After Create";

            // Act
            var createResponse = await client.PostAsJsonAsync("/api/contracts", dto);
            var created = await createResponse.Content.ReadFromJsonAsync<ContractDetailDto>();
            var readResponse = await client.GetAsync($"/api/contracts/{created!.ContractId}");

            // Assert
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
            var read = await readResponse.Content.ReadFromJsonAsync<ContractDetailDto>();
            Assert.NotNull(read);
            Assert.Equal("Read After Create", read!.Title);
        }
    }
}
