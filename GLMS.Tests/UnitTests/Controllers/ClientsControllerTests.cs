using GLMS.Api.Controllers;
using GLMS.Api.DTOs.Clients;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GLMS.Tests.UnitTests.Controllers
{
    public class ClientsControllerTests
    {
        [Fact]
        public async Task GetClient_WithMissingClient_ReturnsNotFound()
        {
            // Arrange
            var clientService = new Mock<IClientService>();
            clientService
                .Setup(service => service.GetClientAsync(99))
                .ReturnsAsync((ClientDetailDto?)null);
            var controller = new ClientsController(clientService.Object);

            // Act
            var result = await controller.GetClient(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_WithValidClient_ReturnsCreatedAtAction()
        {
            // Arrange
            var clientService = new Mock<IClientService>();
            var dto = new CreateClientDto
            {
                CompanyName = "New Client",
                Email = "new@client.local"
            };
            clientService
                .Setup(service => service.CreateClientAsync(dto))
                .ReturnsAsync(new ClientListDto
                {
                    ClientId = 6,
                    CompanyName = dto.CompanyName,
                    Email = dto.Email
                });
            var controller = new ClientsController(clientService.Object);

            // Act
            var result = await controller.Create(dto);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(ClientsController.GetClient), created.ActionName);
            Assert.IsType<ClientListDto>(created.Value);
        }
    }
}
