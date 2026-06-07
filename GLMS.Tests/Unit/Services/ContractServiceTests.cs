using GLMS.Api.Data.Repositories;
using GLMS.Api.DTOs.Contracts;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Moq;

namespace GLMS.Tests.Unit.Services
{
    public class ContractServiceTests
    {
        [Fact]
        public async Task CreateContractAsync_ThrowsArgumentException_WhenStartDateIsNotBeforeEndDate()
        {
            // Arrange
            var service = CreateService(
                out _,
                out _,
                out _);
            var contract = new CreateContractDto
            {
                Title = "Contract A",
                ClientId = 1,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date,
                ContractStatusId = ContractStatusConstants.DraftId,
                ServiceLevel = "Gold",
                Notes = "Test"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateContractAsync(contract));
        }

        [Fact]
        public async Task CreateContractAsync_AddsContract_WhenInputIsValid()
        {
            // Arrange
            var service = CreateService(
                out var contractRepository,
                out var clientRepository,
                out _);

            clientRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Client { ClientId = 1, CompanyName = "Acme", Email = "a@a.com" });
            var contract = new CreateContractDto
            {
                Title = "Contract A",
                ClientId = 1,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(10),
                ContractStatusId = ContractStatusConstants.DraftId,
                ServiceLevel = "Gold",
                Notes = "Test"
            };

            // Act
            var result = await service.CreateContractAsync(contract);

            // Assert
            Assert.Equal("Contract A", result.Title);
            contractRepository.Verify(r => r.AddAsync(It.Is<Contract>(saved =>
                saved.Title == "Contract A" &&
                saved.ClientId == 1 &&
                saved.CreatedByUserId == "user-1")), Times.Once);
            contractRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateContractStatusAsync_UpdatesContractStatus_WhenStatusExists()
        {
            // Arrange
            var service = CreateService(
                out var contractRepository,
                out _,
                out var contractStatusRepository);
            var contract = CreateContractWithStatus(ContractStatusConstants.DraftId, ContractStatusConstants.DraftName);
            var activeStatus = new ContractStatus
            {
                ContractStatusId = ContractStatusConstants.ActiveId,
                StatusName = ContractStatusConstants.ActiveName
            };

            contractRepository
                .Setup(r => r.GetByIdAsync(contract.ContractId))
                .ReturnsAsync(contract);

            contractRepository
                .Setup(r => r.GetContractWithDocumentsAndRequestsAsync(contract.ContractId))
                .ReturnsAsync(contract);

            contractStatusRepository
                .Setup(r => r.GetByIdAsync(ContractStatusConstants.ActiveId))
                .ReturnsAsync(activeStatus);

            // Act
            await service.UpdateContractStatusAsync(contract.ContractId, new UpdateContractStatusDto
            {
                ContractStatusId = ContractStatusConstants.ActiveId
            });

            // Assert
            Assert.Equal(ContractStatusConstants.ActiveId, contract.ContractStatusId);
            Assert.Same(activeStatus, contract.ContractStatus);
            contractRepository.Verify(r => r.Update(contract), Times.Once);
            contractRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateContractStatusAsync_ThrowsKeyNotFoundException_WhenContractDoesNotExist()
        {
            // Arrange
            var service = CreateService(
                out var contractRepository,
                out _,
                out var contractStatusRepository);

            contractRepository
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Contract?)null);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateContractStatusAsync(99, new UpdateContractStatusDto
            {
                ContractStatusId = ContractStatusConstants.ActiveId
            }));

            // Assert
            Assert.Equal("Contract with ID 99 not found.", exception.Message);
            contractStatusRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
            contractRepository.Verify(r => r.Update(It.IsAny<Contract>()), Times.Never);
            contractRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateContractStatusAsync_ThrowsArgumentException_WhenStatusIdIsInvalid()
        {
            // Arrange
            var service = CreateService(
                out var contractRepository,
                out _,
                out var contractStatusRepository);

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateContractStatusAsync(1, new UpdateContractStatusDto
            {
                ContractStatusId = 0
            }));

            // Assert
            Assert.StartsWith("Valid contract status ID is required.", exception.Message);
            contractRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
            contractStatusRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
            contractRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateContractStatusAsync_ThrowsArgumentException_WhenStatusDoesNotExist()
        {
            // Arrange
            var service = CreateService(
                out var contractRepository,
                out _,
                out var contractStatusRepository);
            var contract = CreateContractWithStatus(ContractStatusConstants.DraftId, ContractStatusConstants.DraftName);

            contractRepository
                .Setup(r => r.GetByIdAsync(contract.ContractId))
                .ReturnsAsync(contract);

            contractStatusRepository
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((ContractStatus?)null);

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateContractStatusAsync(contract.ContractId, new UpdateContractStatusDto
            {
                ContractStatusId = 999
            }));

            // Assert
            Assert.StartsWith("Contract status with ID 999 was not found.", exception.Message);
            contractRepository.Verify(r => r.Update(It.IsAny<Contract>()), Times.Never);
            contractRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        private static ContractService CreateService(
            out Mock<IContractRepository> contractRepository,
            out Mock<IClientRepository> clientRepository,
            out Mock<IRepository<ContractStatus>> contractStatusRepository)
        {
            contractRepository = new Mock<IContractRepository>();
            clientRepository = new Mock<IClientRepository>();
            contractStatusRepository = new Mock<IRepository<ContractStatus>>();

            var currentUserService = new Mock<ICurrentUserService>();
            currentUserService.Setup(s => s.UserId).Returns("user-1");

            return new ContractService(
                contractRepository.Object,
                clientRepository.Object,
                contractStatusRepository.Object,
                currentUserService.Object);
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
