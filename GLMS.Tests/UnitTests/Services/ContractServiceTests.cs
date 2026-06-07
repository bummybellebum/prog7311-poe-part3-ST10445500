using GLMS.Api.Data.Repositories;
using GLMS.Api.DTOs.Contracts;
using GLMS.Api.Models;
using GLMS.Api.Services;
using GLMS.Tests.Helpers;
using Moq;

namespace GLMS.Tests.UnitTests.Services
{
    public class ContractServiceTests
    {
        [Fact]
        public async Task CreateContractAsync_WithValidData_ReturnsCreatedContract()
        {
            // Arrange
            var service = CreateService(out var contractRepository, out var clientRepository, out _);
            var dto = TestData.CreateContractDto();
            clientRepository
                .Setup(repository => repository.GetByIdAsync(dto.ClientId))
                .ReturnsAsync(TestData.Client(dto.ClientId));

            // Act
            var result = await service.CreateContractAsync(dto);

            // Assert
            Assert.Equal(dto.Title, result.Title);
            Assert.Equal(TestData.UserId, result.CreatedByUserId);
            contractRepository.Verify(repository => repository.AddAsync(It.Is<Contract>(contract =>
                contract.Title == dto.Title &&
                contract.ClientId == dto.ClientId &&
                contract.CreatedByUserId == TestData.UserId)), Times.Once);
            contractRepository.Verify(repository => repository.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateContractAsync_WithInvalidDates_ThrowsValidationError()
        {
            // Arrange
            var service = CreateService(out var contractRepository, out _, out _);
            var dto = TestData.CreateContractDto();
            dto.EndDate = dto.StartDate;

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateContractAsync(dto));

            // Assert
            Assert.Equal("Start date must be before end date.", exception.Message);
            contractRepository.Verify(repository => repository.AddAsync(It.IsAny<Contract>()), Times.Never);
        }

        [Fact]
        public async Task CreateContractAsync_WithMissingClient_ThrowsNotFound()
        {
            // Arrange
            var service = CreateService(out var contractRepository, out var clientRepository, out _);
            var dto = TestData.CreateContractDto();
            clientRepository
                .Setup(repository => repository.GetByIdAsync(dto.ClientId))
                .ReturnsAsync((Client?)null);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateContractAsync(dto));

            // Assert
            Assert.Equal("Client with ID 1 not found.", exception.Message);
            contractRepository.Verify(repository => repository.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateContractStatusAsync_WithValidStatus_UpdatesContractStatus()
        {
            // Arrange
            var service = CreateService(out var contractRepository, out _, out var statusRepository);
            var contract = TestData.Contract(statusId: ContractStatusConstants.DraftId, statusName: ContractStatusConstants.DraftName);
            var activeStatus = TestData.ContractStatus(ContractStatusConstants.ActiveId, ContractStatusConstants.ActiveName);
            contractRepository
                .Setup(repository => repository.GetByIdAsync(contract.ContractId))
                .ReturnsAsync(contract);
            contractRepository
                .Setup(repository => repository.GetContractWithDocumentsAndRequestsAsync(contract.ContractId))
                .ReturnsAsync(contract);
            statusRepository
                .Setup(repository => repository.GetByIdAsync(activeStatus.ContractStatusId))
                .ReturnsAsync(activeStatus);

            // Act
            var result = await service.UpdateContractStatusAsync(contract.ContractId, new UpdateContractStatusDto
            {
                ContractStatusId = activeStatus.ContractStatusId
            });

            // Assert
            Assert.Equal(activeStatus.ContractStatusId, result.ContractStatusId);
            Assert.Equal(activeStatus.StatusName, result.ContractStatusName);
            contractRepository.Verify(repository => repository.Update(contract), Times.Once);
            contractRepository.Verify(repository => repository.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateContractStatusAsync_WithMissingStatus_ThrowsValidationError()
        {
            // Arrange
            var service = CreateService(out var contractRepository, out _, out var statusRepository);
            var contract = TestData.Contract();
            contractRepository
                .Setup(repository => repository.GetByIdAsync(contract.ContractId))
                .ReturnsAsync(contract);
            statusRepository
                .Setup(repository => repository.GetByIdAsync(99))
                .ReturnsAsync((ContractStatus?)null);

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateContractStatusAsync(contract.ContractId, new UpdateContractStatusDto
            {
                ContractStatusId = 99
            }));

            // Assert
            Assert.StartsWith("Contract status with ID 99 was not found.", exception.Message);
            contractRepository.Verify(repository => repository.Update(It.IsAny<Contract>()), Times.Never);
        }

        private static ContractService CreateService(
            out Mock<IContractRepository> contractRepository,
            out Mock<IClientRepository> clientRepository,
            out Mock<IRepository<ContractStatus>> statusRepository)
        {
            contractRepository = new Mock<IContractRepository>();
            clientRepository = new Mock<IClientRepository>();
            statusRepository = new Mock<IRepository<ContractStatus>>();

            var accountService = new Mock<IAccountService>();
            accountService
                .Setup(service => service.GetCurrentUserId())
                .Returns(TestData.UserId);

            return new ContractService(
                contractRepository.Object,
                clientRepository.Object,
                statusRepository.Object,
                accountService.Object);
        }
    }
}
