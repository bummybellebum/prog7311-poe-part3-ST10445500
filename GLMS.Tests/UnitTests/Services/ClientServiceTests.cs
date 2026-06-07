using GLMS.Api.Data.Repositories;
using GLMS.Api.DTOs.Clients;
using GLMS.Api.Models;
using GLMS.Api.Services;
using GLMS.Tests.Helpers;
using Moq;

namespace GLMS.Tests.UnitTests.Services
{
    public class ClientServiceTests
    {
        [Fact]
        public async Task CreateClientAsync_WithValidData_ReturnsCreatedClient()
        {
            // Arrange
            var clientRepository = new Mock<IClientRepository>();
            clientRepository
                .Setup(repository => repository.IsCompanyNameUniqueAsync("Acme Logistics", null))
                .ReturnsAsync(true);
            var service = new ClientService(clientRepository.Object);
            var dto = new CreateClientDto
            {
                CompanyName = "Acme Logistics",
                Email = "acme@glms.local"
            };

            // Act
            var result = await service.CreateClientAsync(dto);

            // Assert
            Assert.Equal(dto.CompanyName, result.CompanyName);
            clientRepository.Verify(repository => repository.AddAsync(It.Is<Client>(client =>
                client.CompanyName == dto.CompanyName &&
                client.Email == dto.Email)), Times.Once);
            clientRepository.Verify(repository => repository.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateClientAsync_WithDuplicateCompany_ThrowsValidationError()
        {
            // Arrange
            var clientRepository = new Mock<IClientRepository>();
            clientRepository
                .Setup(repository => repository.IsCompanyNameUniqueAsync("Acme Logistics", null))
                .ReturnsAsync(false);
            var service = new ClientService(clientRepository.Object);
            var dto = new CreateClientDto
            {
                CompanyName = "Acme Logistics",
                Email = "acme@glms.local"
            };

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateClientAsync(dto));

            // Assert
            Assert.Equal("A client with company name 'Acme Logistics' already exists.", exception.Message);
            clientRepository.Verify(repository => repository.SaveChangesAsync(), Times.Never);
        }
    }
}
