using GLMS.Api.Controllers;
using GLMS.Api.DTOs.Lookups;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GLMS.Tests.UnitTests.Controllers
{
    public class LookupsControllerTests
    {
        [Fact]
        public async Task GetContractStatuses_ReturnsOkWithLookups()
        {
            // Arrange
            var lookupService = new Mock<ILookupService>();
            lookupService
                .Setup(service => service.GetContractStatusLookupsAsync())
                .ReturnsAsync(new List<LookupDto>
                {
                    new() { Id = 1, Name = "Draft" }
                });
            var controller = new LookupsController(lookupService.Object);

            // Act
            var result = await controller.GetContractStatuses();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var lookups = Assert.IsAssignableFrom<IReadOnlyList<LookupDto>>(ok.Value);
            Assert.Single(lookups);
        }
    }
}
