using GLMS.Api.Data.Repositories;
using GLMS.Api.Models;
using GLMS.Api.Services;
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
            var contractStatusRepository = new Mock<IRepository<ContractStatus>>();
            var service = new ContractService(contractRepository.Object, clientRepository.Object, contractStatusRepository.Object);
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
            var contractStatusRepository = new Mock<IRepository<ContractStatus>>();
            clientRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Client { ClientId = 1, CompanyName = "Acme", Email = "a@a.com" });
            var service = new ContractService(contractRepository.Object, clientRepository.Object, contractStatusRepository.Object);
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

        [Fact]
        public async Task UpdateStatusAsync_UpdatesContractStatus_WhenStatusExists()
        {
            // Arrange
            var contractRepository = new Mock<IContractRepository>();
            var clientRepository = new Mock<IClientRepository>();
            var contractStatusRepository = new Mock<IRepository<ContractStatus>>();
            var service = new ContractService(contractRepository.Object, clientRepository.Object, contractStatusRepository.Object);
            var contract = CreateContractWithStatus(ContractStatusConstants.DraftId, ContractStatusConstants.DraftName);
            var activeStatus = new ContractStatus
            {
                ContractStatusId = ContractStatusConstants.ActiveId,
                StatusName = ContractStatusConstants.ActiveName
            };

            contractRepository
                .Setup(r => r.GetByIdAsync(contract.ContractId))
                .ReturnsAsync(contract);

            contractStatusRepository
                .Setup(r => r.GetByIdAsync(ContractStatusConstants.ActiveId))
                .ReturnsAsync(activeStatus);

            // Act
            await service.UpdateStatusAsync(contract.ContractId, ContractStatusConstants.ActiveId);

            // Assert
            Assert.Equal(ContractStatusConstants.ActiveId, contract.ContractStatusId);
            Assert.Same(activeStatus, contract.ContractStatus);
            contractRepository.Verify(r => r.Update(contract), Times.Once);
            contractRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateStatusAsync_ThrowsKeyNotFoundException_WhenContractDoesNotExist()
        {
            // Arrange
            var contractRepository = new Mock<IContractRepository>();
            var clientRepository = new Mock<IClientRepository>();
            var contractStatusRepository = new Mock<IRepository<ContractStatus>>();
            var service = new ContractService(contractRepository.Object, clientRepository.Object, contractStatusRepository.Object);

            contractRepository
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Contract?)null);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateStatusAsync(99, ContractStatusConstants.ActiveId));

            // Assert
            Assert.Equal("Contract with ID 99 not found.", exception.Message);
            contractStatusRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
            contractRepository.Verify(r => r.Update(It.IsAny<Contract>()), Times.Never);
            contractRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateStatusAsync_ThrowsArgumentException_WhenStatusIdIsInvalid()
        {
            // Arrange
            var contractRepository = new Mock<IContractRepository>();
            var clientRepository = new Mock<IClientRepository>();
            var contractStatusRepository = new Mock<IRepository<ContractStatus>>();
            var service = new ContractService(contractRepository.Object, clientRepository.Object, contractStatusRepository.Object);

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateStatusAsync(1, 0));

            // Assert
            Assert.StartsWith("Valid contract status ID is required.", exception.Message);
            contractRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
            contractStatusRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
            contractRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateStatusAsync_ThrowsArgumentException_WhenStatusDoesNotExist()
        {
            // Arrange
            var contractRepository = new Mock<IContractRepository>();
            var clientRepository = new Mock<IClientRepository>();
            var contractStatusRepository = new Mock<IRepository<ContractStatus>>();
            var service = new ContractService(contractRepository.Object, clientRepository.Object, contractStatusRepository.Object);
            var contract = CreateContractWithStatus(ContractStatusConstants.DraftId, ContractStatusConstants.DraftName);

            contractRepository
                .Setup(r => r.GetByIdAsync(contract.ContractId))
                .ReturnsAsync(contract);

            contractStatusRepository
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((ContractStatus?)null);

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateStatusAsync(contract.ContractId, 999));

            // Assert
            Assert.StartsWith("Contract status with ID 999 was not found.", exception.Message);
            contractRepository.Verify(r => r.Update(It.IsAny<Contract>()), Times.Never);
            contractRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        private static Contract CreateContractWithStatus(int statusId, string statusName)
        {
            return new Contract
            {
                ContractId = 1,
                ClientId = 1,
                Title = "Contract A",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddMonths(6),
                ContractStatusId = statusId,
                CreatedByUserId = "creator-1",
                ContractStatus = new ContractStatus
                {
                    ContractStatusId = statusId,
                    StatusName = statusName
                }
            };
        }
    }
}

