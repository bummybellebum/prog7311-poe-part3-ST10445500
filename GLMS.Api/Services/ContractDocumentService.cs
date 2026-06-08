using GLMS.Api.Data.Repositories;
using GLMS.Api.DTOs.Documents;
using GLMS.Api.DTOs.Mappings;
using GLMS.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

//ST10445500 - PROG7311 - GLMS POE
//ContractDocumentService

//.....................................o0oSTART OF FILEo0o........................................//

// The service keeps business rules and validation away from the controller.

namespace GLMS.Api.Services
{
    public record SignedAgreementDownloadResult(string PhysicalPath, string ContentType, string FileName);

    //..............................................................................//

    //manages business logic for Contract Documents
    public interface IContractDocumentService
    {
        //retrieves document response DTOs associated with a contract
        Task<IReadOnlyList<ContractDocumentDto>> GetContractDocumentsAsync(int contractId);

        //retrieves a document response DTO by ID
        Task<ContractDocumentDto?> GetContractDocumentAsync(int id);

        //creates a new contract document record from a request DTO
        Task<ContractDocumentDto> CreateContractDocumentAsync(int contractId, CreateContractDocumentDto dto);

        //updates an existing document record from a request DTO
        Task<ContractDocumentDto> UpdateContractDocumentAsync(int documentId, UpdateContractDocumentDto dto);

        //removes a document record from the database
        Task DeleteContractDocumentAsync(int id);

        //uploads a signed agreement PDF and returns a response DTO
        Task<ContractDocumentDto> UploadSignedAgreementAsync(int contractId, IFormFile file);

        //gets the physical file info needed to download a signed agreement
        Task<SignedAgreementDownloadResult?> GetSignedAgreementDownloadAsync(int documentId);
    }

    //..............................................................................//

    //implements business logic for managing Contract Documents
    //validates data and coordinates with the repository layer
    public class ContractDocumentService : IContractDocumentService
    {
        private readonly IContractDocumentRepository _documentRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly IAccountService _accountService;

        public ContractDocumentService(
            IContractDocumentRepository documentRepository,
            IContractRepository contractRepository,
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IAccountService accountService)
        {
            _documentRepository = documentRepository;
            _contractRepository = contractRepository;
            _environment = environment;
            _configuration = configuration;
            _accountService = accountService;
        }

        //..............................................................................//

        //retrieves document response DTOs associated with a contract
        public async Task<IReadOnlyList<ContractDocumentDto>> GetContractDocumentsAsync(int contractId)
        {
            var documents = await GetDocumentsByContractIdAsync(contractId);
            return documents.Select(document => document.ToDto()).ToList();
        }

        //..............................................................................//

        //retrieves a document response DTO by ID
        public async Task<ContractDocumentDto?> GetContractDocumentAsync(int id)
        {
            if (id <= 0)
                return null;

            var document = await _documentRepository.GetByIdAsync(id);
            return document?.ToDto();
        }

        //..............................................................................//

        //creates a new contract document record from a request DTO
        public async Task<ContractDocumentDto> CreateContractDocumentAsync(int contractId, CreateContractDocumentDto dto)
        {
            if (contractId != dto.ContractId)
                throw new ArgumentException("Contract ID does not match.");

            var created = await CreateDocumentRecordAsync(dto.ToEntity());
            return created.ToDto();
        }

        //..............................................................................//

        //updates an existing document record from a request DTO
        public async Task<ContractDocumentDto> UpdateContractDocumentAsync(int documentId, UpdateContractDocumentDto dto)
        {
            if (documentId != dto.ContractDocumentId)
                throw new ArgumentException("Document ID does not match.");

            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
                throw new KeyNotFoundException($"Document with ID {documentId} not found.");

            dto.ApplyTo(document);
            await UpdateDocumentRecordAsync(document);

            return document.ToDto();
        }

        //..............................................................................//

        //removes a document record from the database
        public async Task DeleteContractDocumentAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid document ID.", nameof(id));

            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
                throw new KeyNotFoundException($"Document with ID {id} not found.");

            _documentRepository.Delete(document);
            await _documentRepository.SaveChangesAsync();
        }

        //..............................................................................//

        //uploads a signed agreement PDF and returns a response DTO
        public async Task<ContractDocumentDto> UploadSignedAgreementAsync(int contractId, IFormFile file)
        {
            var created = await UploadSignedAgreementRecordAsync(contractId, file, GetCurrentUserId());
            return created.ToDto();
        }

        //..............................................................................//

        //resolves the stored document path for downloading
        public async Task<SignedAgreementDownloadResult?> GetSignedAgreementDownloadAsync(int documentId)
        {
            if (documentId <= 0)
                return null;

            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
                return null;

            var physicalPath = ResolvePhysicalPath(document);
            if (physicalPath == null)
                return null;

            return new SignedAgreementDownloadResult(
                physicalPath,
                string.IsNullOrWhiteSpace(document.ContentType) ? "application/pdf" : document.ContentType,
                string.IsNullOrWhiteSpace(document.OriginalFileName) ? "signed-agreement.pdf" : document.OriginalFileName);
        }

        //..............................................................................//

        //retrieves all documents associated with a contract
        private async Task<List<ContractDocument>> GetDocumentsByContractIdAsync(int contractId)
        {
            if (contractId <= 0)
                return new List<ContractDocument>();

            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null)
                return new List<ContractDocument>();

            return await _documentRepository.GetDocumentsByContractAsync(contractId);
        }

        //..............................................................................//

        //creates a new document record after validation
        private async Task<ContractDocument> CreateDocumentRecordAsync(ContractDocument document)
        {
            ValidateDocument(document);

            var contract = await _contractRepository.GetByIdAsync(document.ContractId);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with ID {document.ContractId} not found.");

            document.UploadedAt = DateTime.UtcNow;

            if (document.IsCurrent)
            {
                await _documentRepository.MarkCurrentDocumentsInactiveAsync(document.ContractId);
            }

            await _documentRepository.AddAsync(document);
            await _documentRepository.SaveChangesAsync();

            return document;
        }

        //..............................................................................//

        //updates an existing document record after validation
        private async Task UpdateDocumentRecordAsync(ContractDocument document)
        {
            ValidateDocument(document);

            if (document.ContractDocumentId <= 0)
                throw new ArgumentException("Invalid document ID.", nameof(document.ContractDocumentId));

            _documentRepository.Update(document);
            await _documentRepository.SaveChangesAsync();
        }

        //..............................................................................//

        //uploads a signed agreement PDF, saves it to disk, and stores metadata only
        private async Task<ContractDocument> UploadSignedAgreementRecordAsync(int contractId, IFormFile file, string uploadedByUserId)
        {
            if (contractId <= 0)
                throw new ArgumentException("Valid contract ID is required.", nameof(contractId));

            if (string.IsNullOrWhiteSpace(uploadedByUserId))
                throw new ArgumentException("Uploaded by user is required.", nameof(uploadedByUserId));

            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with ID {contractId} not found.");

            // File validation happens before saving so unsafe uploads are rejected early.
            await ValidatePdfFileAsync(file);

            var uploadFolder = GetUploadFolder();
            Directory.CreateDirectory(uploadFolder);

            var storedFileName = BuildSafeStoredFileName(contractId);
            var physicalPath = await SaveFileAsync(file, uploadFolder, storedFileName);

            var document = new ContractDocument
            {
                ContractId = contractId,
                DocumentType = "Signed Agreement",
                OriginalFileName = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                FilePath = BuildDatabaseFilePath(storedFileName),
                ContentType = "application/pdf",
                FileSizeBytes = file.Length,
                UploadedByUserId = uploadedByUserId,
                IsCurrent = true
            };

            try
            {
                return await CreateDocumentRecordAsync(document);
            }
            catch
            {
                // If the database save fails, remove the physical file so storage does not get out of sync.
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }

                throw;
            }
        }

        //..............................................................................//

        //checks the required document fields
        private static void ValidateDocument(ContractDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (document.ContractId <= 0)
                throw new ArgumentException("Valid contract ID is required.", nameof(document.ContractId));

            if (string.IsNullOrWhiteSpace(document.OriginalFileName))
                throw new ArgumentException("Original file name is required.", nameof(document.OriginalFileName));

            if (string.IsNullOrWhiteSpace(document.StoredFileName))
                throw new ArgumentException("Stored file name is required.", nameof(document.StoredFileName));

            if (string.IsNullOrWhiteSpace(document.FilePath))
                throw new ArgumentException("File path is required.", nameof(document.FilePath));
        }

        //..............................................................................//

        //checks that the uploaded file is a PDF
        private async Task ValidatePdfFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Please select a PDF file to upload.", nameof(file));

            var extension = Path.GetExtension(file.FileName);
            if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Only PDF files are allowed for signed agreements.", nameof(file));

            var contentType = file.ContentType ?? string.Empty;
            var validContentType = string.IsNullOrWhiteSpace(contentType)
                || string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
                || string.Equals(contentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase);

            if (!validContentType)
                throw new ArgumentException("Only PDF files are allowed for signed agreements.", nameof(file));

            var header = new byte[5];
            await using var stream = file.OpenReadStream();
            var bytesRead = await stream.ReadAsync(header.AsMemory(0, header.Length));
            // Checking the PDF header is stronger than only trusting the file extension.
            var hasPdfHeader = bytesRead == header.Length
                && header[0] == '%'
                && header[1] == 'P'
                && header[2] == 'D'
                && header[3] == 'F'
                && header[4] == '-';

            if (!hasPdfHeader)
                throw new ArgumentException("Only valid PDF files are allowed for signed agreements.", nameof(file));
        }

        //..............................................................................//

        private string GetCurrentUserId()
        {
            return _accountService.GetCurrentUserId();
        }

        //..............................................................................//

        private static string BuildSafeStoredFileName(int contractId)
        {
            return $"contract-{contractId}-{Guid.NewGuid():N}.pdf";
        }

        //..............................................................................//

        private async Task<string> SaveFileAsync(IFormFile file, string uploadFolder, string storedFileName)
        {
            var fullPath = Path.GetFullPath(Path.Combine(uploadFolder, storedFileName));
            // This prevents a file name or path from saving outside the configured upload folder.
            if (!IsPathInsideBase(fullPath, uploadFolder))
                throw new InvalidOperationException("Invalid upload path.");

            await using var stream = File.Create(fullPath);
            await file.CopyToAsync(stream);

            return fullPath;
        }

        //..............................................................................//

        private string BuildDatabaseFilePath(string storedFileName)
        {
            var folder = GetRelativeUploadFolderForDatabase();
            return $"{folder}/{storedFileName}".Replace("\\", "/");
        }

        //..............................................................................//

        private string GetUploadFolder()
        {
            var configuredFolder = _configuration["Uploads:SignedAgreementFolder"];
            var folder = string.IsNullOrWhiteSpace(configuredFolder)
                ? "uploads/signed-agreements"
                : configuredFolder;

            if (Path.IsPathRooted(folder))
                return Path.GetFullPath(folder);

            return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, folder));
        }

        //..............................................................................//

        private string GetRelativeUploadFolderForDatabase()
        {
            var configuredFolder = _configuration["Uploads:SignedAgreementFolder"];
            if (string.IsNullOrWhiteSpace(configuredFolder) || Path.IsPathRooted(configuredFolder))
                return "uploads/signed-agreements";

            return CleanRelativeFolder(configuredFolder);
        }

        //..............................................................................//

        private static string CleanRelativeFolder(string folder)
        {
            var parts = folder
                .Replace("\\", "/")
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Where(part => part != ".");

            if (parts.Any(part => part == ".."))
                throw new InvalidOperationException("Invalid upload folder configuration.");

            return string.Join("/", parts);
        }

        //..............................................................................//

        private string? ResolvePhysicalPath(ContractDocument document)
        {
            foreach (var candidate in GetDownloadPathCandidates(document))
            {
                var fullPath = Path.GetFullPath(candidate);
                if (IsSafeDownloadPath(fullPath) && File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return null;
        }

        //..............................................................................//

        private IEnumerable<string> GetDownloadPathCandidates(ContractDocument document)
        {
            var candidates = new List<string>();

            if (!string.IsNullOrWhiteSpace(document.FilePath))
            {
                var normalizedFilePath = document.FilePath
                    .Replace("/", Path.DirectorySeparatorChar.ToString())
                    .Replace("\\", Path.DirectorySeparatorChar.ToString());

                if (Path.IsPathRooted(normalizedFilePath))
                {
                    candidates.Add(normalizedFilePath);
                }
				else
				{
					candidates.Add(Path.Combine(_environment.ContentRootPath, normalizedFilePath));

					if (!string.IsNullOrWhiteSpace(_environment.WebRootPath))
					{
						candidates.Add(Path.Combine(_environment.WebRootPath, normalizedFilePath));
					}
				}
			}

            if (!string.IsNullOrWhiteSpace(document.StoredFileName))
            {
                candidates.Add(Path.Combine(GetUploadFolder(), Path.GetFileName(document.StoredFileName)));
            }

            return candidates.Distinct(StringComparer.OrdinalIgnoreCase);
        }

        //..............................................................................//

        private bool IsSafeDownloadPath(string fullPath)
        {
            return IsPathInsideBase(fullPath, _environment.ContentRootPath)
                || IsPathInsideBase(fullPath, GetUploadFolder());
        }

        //..............................................................................//

        private static bool IsPathInsideBase(string fullPath, string basePath)
        {
            var normalizedFullPath = Path.GetFullPath(fullPath);
            var normalizedBasePath = Path.GetFullPath(basePath);

            if (!normalizedBasePath.EndsWith(Path.DirectorySeparatorChar))
            {
                normalizedBasePath += Path.DirectorySeparatorChar;
            }

            return normalizedFullPath.StartsWith(normalizedBasePath, StringComparison.OrdinalIgnoreCase);
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
