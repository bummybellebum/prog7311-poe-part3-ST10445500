using GLMS.Api.Data.Repositories;
using GLMS.Api.Models;
using GLMS.Api.Services;
using GLMS.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;

namespace GLMS.Tests.UnitTests.Services
{
    public class ContractDocumentServiceTests
    {
        [Fact]
        public async Task UploadSignedAgreementAsync_WithPdfFile_ReturnsStoredDocument()
        {
            // Arrange
            var tempRoot = CreateTempRoot();
            try
            {
                var service = CreateService(tempRoot, out var documentRepository, out var contractRepository);
                ContractDocument? savedDocument = null;
                contractRepository
                    .Setup(repository => repository.GetByIdAsync(1))
                    .ReturnsAsync(TestData.Contract());
                documentRepository
                    .Setup(repository => repository.AddAsync(It.IsAny<ContractDocument>()))
                    .Callback<ContractDocument>(document => savedDocument = document)
                    .Returns(Task.CompletedTask);
                var file = TestData.FormFile("signed agreement.pdf", "application/pdf", TestData.PdfBytes());

                // Act
                var result = await service.UploadSignedAgreementAsync(1, file);

                // Assert
                Assert.NotNull(savedDocument);
                Assert.Equal("Signed Agreement", result.DocumentType);
                Assert.Equal("signed agreement.pdf", result.OriginalFileName);
                Assert.StartsWith("contract-1-", result.StoredFileName);
                Assert.EndsWith(".pdf", result.StoredFileName);
                Assert.Equal("application/pdf", result.ContentType);
                Assert.Equal(TestData.UserId, result.UploadedByUserId);
                Assert.True(File.Exists(Path.Combine(tempRoot, result.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString()))));
                documentRepository.Verify(repository => repository.MarkCurrentDocumentsInactiveAsync(1), Times.Once);
                documentRepository.Verify(repository => repository.SaveChangesAsync(), Times.Once);
            }
            finally
            {
                DeleteTempRoot(tempRoot);
            }
        }

        [Fact]
        public async Task UploadSignedAgreementAsync_WithExeFile_ThrowsValidationError()
        {
            // Arrange
            var service = CreateService(out _, out var contractRepository);
            contractRepository
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(TestData.Contract());
            var file = TestData.FormFile("agreement.exe", "application/octet-stream", TestData.PdfBytes());

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.UploadSignedAgreementAsync(1, file));

            // Assert
            Assert.StartsWith("Only PDF files are allowed for signed agreements.", exception.Message);
        }

        [Fact]
        public async Task UploadSignedAgreementAsync_WithInvalidPdfContent_ThrowsValidationError()
        {
            // Arrange
            var service = CreateService(out _, out var contractRepository);
            contractRepository
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(TestData.Contract());
            var file = TestData.FormFile("agreement.pdf", "application/pdf", "not a pdf"u8.ToArray());

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.UploadSignedAgreementAsync(1, file));

            // Assert
            Assert.StartsWith("Only valid PDF files are allowed for signed agreements.", exception.Message);
        }

        [Fact]
        public async Task UploadSignedAgreementAsync_WithEmptyFile_ThrowsValidationError()
        {
            // Arrange
            var service = CreateService(out _, out var contractRepository);
            contractRepository
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(TestData.Contract());
            var file = TestData.FormFile("agreement.pdf", "application/pdf", []);

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.UploadSignedAgreementAsync(1, file));

            // Assert
            Assert.StartsWith("Please select a PDF file to upload.", exception.Message);
        }

        [Fact]
        public async Task UploadSignedAgreementAsync_WithNullFile_ThrowsValidationError()
        {
            // Arrange
            var service = CreateService(out _, out var contractRepository);
            contractRepository
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(TestData.Contract());

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.UploadSignedAgreementAsync(1, null!));

            // Assert
            Assert.StartsWith("Please select a PDF file to upload.", exception.Message);
        }

        private static ContractDocumentService CreateService(
            out Mock<IContractDocumentRepository> documentRepository,
            out Mock<IContractRepository> contractRepository)
        {
            return CreateService(CreateTempRoot(), out documentRepository, out contractRepository);
        }

        private static ContractDocumentService CreateService(
            string contentRoot,
            out Mock<IContractDocumentRepository> documentRepository,
            out Mock<IContractRepository> contractRepository)
        {
            documentRepository = new Mock<IContractDocumentRepository>();
            contractRepository = new Mock<IContractRepository>();
            documentRepository
                .Setup(repository => repository.MarkCurrentDocumentsInactiveAsync(It.IsAny<int>()))
                .ReturnsAsync(1);

            var environment = new Mock<IWebHostEnvironment>();
            environment
                .Setup(env => env.ContentRootPath)
                .Returns(contentRoot);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Uploads:SignedAgreementFolder"] = "uploads/signed-agreements"
                })
                .Build();

            var accountService = new Mock<IAccountService>();
            accountService
                .Setup(service => service.GetCurrentUserId())
                .Returns(TestData.UserId);

            return new ContractDocumentService(
                documentRepository.Object,
                contractRepository.Object,
                environment.Object,
                configuration,
                accountService.Object);
        }

        private static string CreateTempRoot()
        {
            return Path.Combine(Path.GetTempPath(), "glms-tests", Guid.NewGuid().ToString("N"));
        }

        private static void DeleteTempRoot(string tempRoot)
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }
}
