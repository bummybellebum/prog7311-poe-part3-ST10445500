using GLMS.Api.Controllers;
using GLMS.Api.DTOs.Contracts;
using GLMS.Api.DTOs.Documents;
using GLMS.Api.Services;
using GLMS.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GLMS.Tests.UnitTests.Controllers
{
    public class ContractsControllerTests
    {
        [Fact]
        public async Task GetContract_WithMissingContract_ReturnsNotFound()
        {
            // Arrange
            var contractService = new Mock<IContractService>();
            var documentService = new Mock<IContractDocumentService>();
            contractService
                .Setup(service => service.GetContractAsync(99))
                .ReturnsAsync((ContractDetailDto?)null);
            var controller = new ContractsController(contractService.Object, documentService.Object);

            // Act
            var result = await controller.GetContract(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_WithValidContract_ReturnsCreatedAtAction()
        {
            // Arrange
            var contractService = new Mock<IContractService>();
            var documentService = new Mock<IContractDocumentService>();
            var dto = TestData.CreateContractDto();
            contractService
                .Setup(service => service.CreateContractAsync(dto))
                .ReturnsAsync(new ContractDetailDto
                {
                    ContractId = 10,
                    ClientId = dto.ClientId,
                    Title = dto.Title
                });
            var controller = new ContractsController(contractService.Object, documentService.Object);

            // Act
            var result = await controller.Create(dto);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(ContractsController.GetContract), created.ActionName);
            Assert.IsType<ContractDetailDto>(created.Value);
        }

        [Fact]
        public async Task DownloadAgreement_WithMissingFile_ReturnsNotFound()
        {
            // Arrange
            var contractService = new Mock<IContractService>();
            var documentService = new Mock<IContractDocumentService>();
            documentService
                .Setup(service => service.GetSignedAgreementDownloadAsync(5))
                .ReturnsAsync((SignedAgreementDownloadResult?)null);
            var controller = new ContractsController(contractService.Object, documentService.Object);

            // Act
            var result = await controller.DownloadAgreement(5);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
