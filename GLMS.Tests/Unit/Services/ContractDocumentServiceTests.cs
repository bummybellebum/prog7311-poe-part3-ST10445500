using GLMS.Api.Data.Repositories;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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
            var service = CreateService(documentRepository, contractRepository);
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
            documentRepository.Setup(r => r.MarkCurrentDocumentsInactiveAsync(1)).ReturnsAsync(1);
            contractRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CreateContract());
            var service = CreateService(documentRepository, contractRepository);
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
            documentRepository.Verify(r => r.MarkCurrentDocumentsInactiveAsync(1), Times.Once);
            documentRepository.Verify(r => r.AddAsync(document), Times.Once);
            documentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UploadSignedAgreementAsync_ThrowsKeyNotFoundException_WhenContractDoesNotExist()
        {
            // Arrange
            var documentRepository = new Mock<IContractDocumentRepository>();
            var contractRepository = new Mock<IContractRepository>();
            contractRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Contract?)null);
            var service = CreateService(documentRepository, contractRepository);
            var file = CreateFormFile("agreement.pdf", "application/pdf", CreatePdfBytes());

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UploadSignedAgreementAsync(1, file, "user-1"));
        }

        [Fact]
        public async Task UploadSignedAgreementAsync_ThrowsArgumentException_WhenFileIsEmpty()
        {
            // Arrange
            var documentRepository = new Mock<IContractDocumentRepository>();
            var contractRepository = new Mock<IContractRepository>();
            contractRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CreateContract());
            var service = CreateService(documentRepository, contractRepository);
            var file = CreateFormFile("agreement.pdf", "application/pdf", []);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.UploadSignedAgreementAsync(1, file, "user-1"));
        }

        [Fact]
        public async Task UploadSignedAgreementAsync_ThrowsArgumentException_WhenFileExtensionIsNotPdf()
        {
            // Arrange
            var documentRepository = new Mock<IContractDocumentRepository>();
            var contractRepository = new Mock<IContractRepository>();
            contractRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CreateContract());
            var service = CreateService(documentRepository, contractRepository);
            var file = CreateFormFile("agreement.exe", "application/octet-stream", CreatePdfBytes());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.UploadSignedAgreementAsync(1, file, "user-1"));
        }

        [Fact]
        public async Task UploadSignedAgreementAsync_ThrowsArgumentException_WhenRenamedFileIsNotPdf()
        {
            // Arrange
            var documentRepository = new Mock<IContractDocumentRepository>();
            var contractRepository = new Mock<IContractRepository>();
            contractRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CreateContract());
            var service = CreateService(documentRepository, contractRepository);
            var file = CreateFormFile("agreement.pdf", "application/pdf", "MZ fake exe"u8.ToArray());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.UploadSignedAgreementAsync(1, file, "user-1"));
        }

        [Fact]
        public async Task UploadSignedAgreementAsync_CreatesDocumentAndStoresFile_WhenPdfIsValid()
        {
            // Arrange
            var tempRoot = CreateTempRoot();
            try
            {
                var documentRepository = new Mock<IContractDocumentRepository>();
                var contractRepository = new Mock<IContractRepository>();
                ContractDocument? savedDocument = null;

                contractRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CreateContract());
                documentRepository.Setup(r => r.MarkCurrentDocumentsInactiveAsync(1)).ReturnsAsync(1);
                documentRepository.Setup(r => r.AddAsync(It.IsAny<ContractDocument>()))
                    .Callback<ContractDocument>(d => savedDocument = d)
                    .Returns(Task.CompletedTask);

                var service = CreateService(documentRepository, contractRepository, tempRoot);
                var file = CreateFormFile("signed agreement.pdf", "application/pdf", CreatePdfBytes());

                // Act
                var result = await service.UploadSignedAgreementAsync(1, file, "user-1");

                // Assert
                Assert.NotNull(savedDocument);
                Assert.Equal("Signed Agreement", result.DocumentType);
                Assert.Equal("signed agreement.pdf", result.OriginalFileName);
                Assert.StartsWith("contract-1-", result.StoredFileName);
                Assert.EndsWith(".pdf", result.StoredFileName);
                Assert.Equal($"uploads/signed-agreements/{result.StoredFileName}", result.FilePath);
                Assert.Equal("application/pdf", result.ContentType);
                Assert.Equal("user-1", result.UploadedByUserId);
                Assert.True(File.Exists(Path.Combine(tempRoot, result.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString()))));
                documentRepository.Verify(r => r.MarkCurrentDocumentsInactiveAsync(1), Times.Once);
                documentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
            }
            finally
            {
                DeleteTempRoot(tempRoot);
            }
        }

        [Fact]
        public async Task GetSignedAgreementDownloadAsync_ReturnsFile_WhenRelativeLegacyPathExists()
        {
            // Arrange
            var tempRoot = CreateTempRoot();
            try
            {
                var legacyFolder = Path.Combine(tempRoot, "uploads");
                Directory.CreateDirectory(legacyFolder);
                var legacyPath = Path.Combine(legacyFolder, "legacy.pdf");
                await File.WriteAllBytesAsync(legacyPath, CreatePdfBytes());

                var documentRepository = new Mock<IContractDocumentRepository>();
                var contractRepository = new Mock<IContractRepository>();
                documentRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new ContractDocument
                {
                    ContractDocumentId = 5,
                    ContractId = 1,
                    DocumentType = "Signed Agreement",
                    OriginalFileName = "legacy.pdf",
                    StoredFileName = "legacy.pdf",
                    FilePath = "uploads/legacy.pdf",
                    ContentType = "application/pdf",
                    UploadedByUserId = "user-1"
                });

                var service = CreateService(documentRepository, contractRepository, tempRoot);

                // Act
                var result = await service.GetSignedAgreementDownloadAsync(5);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(Path.GetFullPath(legacyPath), result.PhysicalPath);
                Assert.Equal("application/pdf", result.ContentType);
                Assert.Equal("legacy.pdf", result.FileName);
            }
            finally
            {
                DeleteTempRoot(tempRoot);
            }
        }

        [Fact]
        public async Task GetSignedAgreementDownloadAsync_ReturnsNull_WhenPhysicalFileIsMissing()
        {
            // Arrange
            var documentRepository = new Mock<IContractDocumentRepository>();
            var contractRepository = new Mock<IContractRepository>();
            documentRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new ContractDocument
            {
                ContractDocumentId = 5,
                ContractId = 1,
                DocumentType = "Signed Agreement",
                OriginalFileName = "missing.pdf",
                StoredFileName = "missing.pdf",
                FilePath = "uploads/missing.pdf",
                ContentType = "application/pdf",
                UploadedByUserId = "user-1"
            });

            var service = CreateService(documentRepository, contractRepository);

            // Act
            var result = await service.GetSignedAgreementDownloadAsync(5);

            // Assert
            Assert.Null(result);
        }

        private static ContractDocumentService CreateService(
            Mock<IContractDocumentRepository> documentRepository,
            Mock<IContractRepository> contractRepository,
            string? contentRootPath = null,
            string uploadFolder = "uploads/signed-agreements")
        {
            var environment = new Mock<IWebHostEnvironment>();
            environment.Setup(e => e.ContentRootPath).Returns(contentRootPath ?? CreateTempRoot());

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Uploads:SignedAgreementFolder"] = uploadFolder
                })
                .Build();

            return new ContractDocumentService(
                documentRepository.Object,
                contractRepository.Object,
                environment.Object,
                configuration);
        }

        private static Contract CreateContract()
        {
            return new Contract
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
            };
        }

        private static IFormFile CreateFormFile(string fileName, string contentType, byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        private static byte[] CreatePdfBytes()
        {
            return "%PDF-1.4\nTest PDF"u8.ToArray();
        }

        private static string CreateTempRoot()
        {
            return Path.Combine(Path.GetTempPath(), "glms-contract-documents", Guid.NewGuid().ToString("N"));
        }

        private static void DeleteTempRoot(string tempRoot)
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }
}
