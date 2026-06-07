using GLMS.Api.Data.Repositories;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Moq;

namespace GLMS.Tests.Unit.Services
{
    public class ContractDocumentServiceTests
    {
        [Fact]
        public async Task CreateAsync_ThrowsKeyNotFoundException_WhenContractDoesNotExist()
        {
            // Arrange
            var documentRepository = new Mock<IContractDocumentRepository>();
            var contractRepository = new Mock<IContractRepository>();
            contractRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Contract?)null);
            var service = new ContractDocumentService(documentRepository.Object, contractRepository.Object);
            var document = new ContractDocument
            {
                ContractId = 1,
                DocumentType = "Signed Agreement",
                OriginalFileName = "contract.pdf",
                StoredFileName = "contract-1.pdf",
                FilePath = "uploads/contract-1.pdf",
                UploadedByUserId = "user-1"
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(document));
        }

        [Fact]
        public async Task CreateAsync_AddsDocument_WhenInputIsValid()
        {
            // Arrange
            var documentRepository = new Mock<IContractDocumentRepository>();
            var contractRepository = new Mock<IContractRepository>();
            contractRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Contract
            {
                ContractId = 1,
                Title = "Contract A",
                ClientId = 1,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(30),
                ContractStatusId = 2,
                CreatedByUserId = "user-1",
                ServiceLevel = "Gold",
                Notes = "Test"
            });
            var service = new ContractDocumentService(documentRepository.Object, contractRepository.Object);
            var document = new ContractDocument
            {
                ContractId = 1,
                DocumentType = "Signed Agreement",
                OriginalFileName = "contract.pdf",
                StoredFileName = "contract-1.pdf",
                FilePath = "uploads/contract-1.pdf",
                UploadedByUserId = "user-1"
            };

            // Act
            var result = await service.CreateAsync(document);

            // Assert
            Assert.Equal("contract.pdf", result.OriginalFileName);
            documentRepository.Verify(r => r.AddAsync(document), Times.Once);
            documentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}

