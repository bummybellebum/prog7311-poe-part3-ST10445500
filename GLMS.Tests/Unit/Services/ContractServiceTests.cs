using GLMS.Web.Data.Repositories;
using GLMS.Web.Models;
using GLMS.Web.Services;
using Moq;

namespace GLMS.Tests.Unit.Services
{
    public class ContractServiceTests
    {
        [Fact]
        public async Task CreateAsync_ThrowsArgumentException_WhenStartDateIsNotBeforeEndDate()
        {
            // Arrange
            var contractRepository = new Mock<IContractRepository>();
            var clientRepository = new Mock<IClientRepository>();
            var service = new ContractService(contractRepository.Object, clientRepository.Object);
            var contract = new Contract
            {
                Title = "Contract A",
                ClientId = 1,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date,
                CreatedByUserId = "user-1",
                ServiceLevel = "Gold",
                Notes = "Test"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(contract));
        }

        [Fact]
        public async Task CreateAsync_AddsContract_WhenInputIsValid()
        {
            // Arrange
            var contractRepository = new Mock<IContractRepository>();
            var clientRepository = new Mock<IClientRepository>();
            clientRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Client { ClientId = 1, CompanyName = "Acme", Email = "a@a.com" });
            var service = new ContractService(contractRepository.Object, clientRepository.Object);
            var contract = new Contract
            {
                Title = "Contract A",
                ClientId = 1,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(10),
                CreatedByUserId = "user-1",
                ServiceLevel = "Gold",
                Notes = "Test"
            };

            // Act
            var result = await service.CreateAsync(contract);

            // Assert
            Assert.Equal("Contract A", result.Title);
            contractRepository.Verify(r => r.AddAsync(contract), Times.Once);
            contractRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
