using GLMS.Api.Data.Repositories;
using GLMS.Api.Models;

//ST10445500 - PROG7311 - GLMS POE
//ContractDocumentService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Services
{
    //manages business logic for Contract Documents
    public interface IContractDocumentService
    {
        //retrieves a document by ID.
        Task<ContractDocument?> GetByIdAsync(int id);

        //retrieves all documents associated with a contract.
        Task<List<ContractDocument>> GetByContractIdAsync(int contractId);

        //retrieves the current active document for a contract.
        Task<ContractDocument?> GetCurrentByContractIdAsync(int contractId);

        //creates a new contract document record.
        Task<ContractDocument> CreateAsync(ContractDocument document);

        //updates an existing document record.
        Task UpdateAsync(ContractDocument document);

        //removes a document record from the database.
        Task DeleteAsync(int id);
    }

    //..............................................................................//

    //implements business logic for managing Contract Documents
    //validates data and coordinates with the repository layer
    public class ContractDocumentService : IContractDocumentService
    {
        private readonly IContractDocumentRepository _documentRepository;
        private readonly IContractRepository _contractRepository;

        public ContractDocumentService(
            IContractDocumentRepository documentRepository,
            IContractRepository contractRepository)
        {
            _documentRepository = documentRepository;
            _contractRepository = contractRepository;
        }

        //..............................................................................//

        //retrieves a document by ID
        public async Task<ContractDocument?> GetByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _documentRepository.GetByIdAsync(id);
        }

        //..............................................................................//

        //retrieves all documents associated with a contract.
        public async Task<List<ContractDocument>> GetByContractIdAsync(int contractId)
        {
            if (contractId <= 0)
                return new List<ContractDocument>();

            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null)
                return new List<ContractDocument>();

            return await _documentRepository.GetDocumentsByContractAsync(contractId);
        }

        //..............................................................................//

        //retrieves the current active document for a contract
        public async Task<ContractDocument?> GetCurrentByContractIdAsync(int contractId)
        {
            if (contractId <= 0)
                return null;

            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null)
                return null;

            return await _documentRepository.GetCurrentDocumentByContractAsync(contractId);
        }

        //..............................................................................//

        //creates a new document record. all fields are required
        public async Task<ContractDocument> CreateAsync(ContractDocument document)
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

            var contract = await _contractRepository.GetByIdAsync(document.ContractId);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with ID {document.ContractId} not found.");

            document.UploadedAt = DateTime.UtcNow;

            await _documentRepository.AddAsync(document);
            await _documentRepository.SaveChangesAsync();

            return document;
        }

        //..............................................................................//

        //updates an existing document record
        public async Task UpdateAsync(ContractDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (document.ContractDocumentId <= 0)
                throw new ArgumentException("Invalid document ID.", nameof(document.ContractDocumentId));

            var existing = await _documentRepository.GetByIdAsync(document.ContractDocumentId);
            if (existing == null)
                throw new KeyNotFoundException($"Document with ID {document.ContractDocumentId} not found.");

            _documentRepository.Update(document);
            await _documentRepository.SaveChangesAsync();
        }

        //..............................................................................//

        //removes a document record from the database
        public async Task DeleteAsync(int id)
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
    }
}

//......................................o0oEND OF FILEo0o.........................................//
