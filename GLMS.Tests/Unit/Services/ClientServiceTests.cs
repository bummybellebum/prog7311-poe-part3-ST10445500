using GLMS.Api.Data.Repositories;
using GLMS.Api.DTOs.Clients;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Moq;

namespace GLMS.Tests.Unit.Services
{
    public class ClientServiceTests
    {
        [Fact]
        public async Task CreateClientAsync_ThrowsInvalidOperationException_WhenCompanyNameAlreadyExists()
        {
            // Arrange
            var clientRepository = new Mock<IClientRepository>();
            clientRepository.Setup(r => r.IsCompanyNameUniqueAsync("Acme Corp", null)).ReturnsAsync(false);
            var service = new ClientService(clientRepository.Object);
            var input = new CreateClientDto
            {
                CompanyName = "Acme Corp",
                Email = "contact@acme.com"
            };

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateClientAsync(input));

            // Assert
            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public async Task CreateClientAsync_AddsClient_WhenInputIsValidAndUnique()
        {
            // Arrange
            var clientRepository = new Mock<IClientRepository>();
            clientRepository.Setup(r => r.IsCompanyNameUniqueAsync("Acme Corp", null)).ReturnsAsync(true);
            var service = new ClientService(clientRepository.Object);
            var input = new CreateClientDto
            {
                CompanyName = "Acme Corp",
                Email = "contact@acme.com"
            };

            // Act
            var result = await service.CreateClientAsync(input);

            // Assert
            Assert.Equal("Acme Corp", result.CompanyName);
            clientRepository.Verify(r => r.AddAsync(It.Is<Client>(client =>
                client.CompanyName == "Acme Corp" &&
                client.Email == "contact@acme.com")), Times.Once);
            clientRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
