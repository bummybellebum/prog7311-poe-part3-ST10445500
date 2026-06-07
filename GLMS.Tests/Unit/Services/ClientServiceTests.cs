using GLMS.Api.Data.Repositories;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Moq;

namespace GLMS.Tests.Unit.Services
{
    public class ClientServiceTests
    {
        [Fact]
        public async Task CreateAsync_ThrowsInvalidOperationException_WhenCompanyNameAlreadyExists()
        {
            // Arrange
            var clientRepository = new Mock<IClientRepository>();
            clientRepository.Setup(r => r.IsCompanyNameUniqueAsync("Acme Corp", null)).ReturnsAsync(false);
            var service = new ClientService(clientRepository.Object);
            var input = new Client
            {
                CompanyName = "Acme Corp",
                Email = "contact@acme.com"
            };

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(input));

            // Assert
            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public async Task CreateAsync_AddsClient_WhenInputIsValidAndUnique()
        {
            // Arrange
            var clientRepository = new Mock<IClientRepository>();
            clientRepository.Setup(r => r.IsCompanyNameUniqueAsync("Acme Corp", null)).ReturnsAsync(true);
            var service = new ClientService(clientRepository.Object);
            var input = new Client
            {
                CompanyName = "Acme Corp",
                Email = "contact@acme.com"
            };

            // Act
            var result = await service.CreateAsync(input);

            // Assert
            Assert.Equal("Acme Corp", result.CompanyName);
            clientRepository.Verify(r => r.AddAsync(input), Times.Once);
            clientRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}

