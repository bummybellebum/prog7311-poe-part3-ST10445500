using Microsoft.EntityFrameworkCore;
using GLMS.Api.Models;

//ST10445500 - PROG7311 - GLMS POE
//ContractDocumentRepository

//.....................................o0oSTART OF FILEo0o........................................//

// The repository keeps database query code in one layer instead of inside controllers.

namespace GLMS.Api.Data.Repositories
{

    //this interface lets us do things with ContractDocument data in the database.
    public interface IContractDocumentRepository : IRepository<ContractDocument>
    {
        Task<List<ContractDocument>> GetDocumentsByContractAsync(int contractId);
        Task<ContractDocument?> GetCurrentDocumentByContractAsync(int contractId);
        Task<ContractDocument?> GetDocumentWithDetailsAsync(int documentId);
        Task<int> MarkCurrentDocumentsInactiveAsync(int contractId);
    }

    //..............................................................................//

    //this class handles all the database operations for ContractDocuments.
    //it has extra methods for things we do with documents a lot.
    public class ContractDocumentRepository : Repository<ContractDocument>, IContractDocumentRepository
    {
        public ContractDocumentRepository(ApplicationDbContext context) : base(context)
        {
        }

        //..............................................................................//

        //gets all documents that belong to a certain contract.
        public async Task<List<ContractDocument>> GetDocumentsByContractAsync(int contractId)
        {
            return await Query()
                .Where(d => d.ContractId == contractId)
                .Include(d => d.UploadedByUser)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        //..............................................................................//

        //gets the current/latest document for a contract.
        public async Task<ContractDocument?> GetCurrentDocumentByContractAsync(int contractId)
        {
            return await Query()
                .Where(d => d.ContractId == contractId && d.IsCurrent)
                .Include(d => d.UploadedByUser)
                .OrderByDescending(d => d.UploadedAt)
                .FirstOrDefaultAsync();
        }

        //..............................................................................//

        //gets one document with all its info including the contract and who uploaded it.
        public async Task<ContractDocument?> GetDocumentWithDetailsAsync(int documentId)
        {
            return await Query()
                .Include(d => d.Contract)
                .Include(d => d.UploadedByUser)
                .FirstOrDefaultAsync(d => d.ContractDocumentId == documentId);
        }

        //..............................................................................//

        //marks current documents for a contract as no longer current.
        public async Task<int> MarkCurrentDocumentsInactiveAsync(int contractId)
        {
            var query = TrackedQuery().Where(d => d.ContractId == contractId && d.IsCurrent);

            if (_context.Database.IsRelational())
            {
                return await query.ExecuteUpdateAsync(setters => setters
                    .SetProperty(d => d.IsCurrent, false));
            }

            var currentDocuments = await query.ToListAsync();
            foreach (var document in currentDocuments)
            {
                document.IsCurrent = false;
            }

            await _context.SaveChangesAsync();
            return currentDocuments.Count;
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o........................................//

//.....................................o0oEND OF FILEo0o..........................................//
