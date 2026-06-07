using Microsoft.EntityFrameworkCore;
using GLMS.Web.Models;

//ST10445500 - PROG7311 - GLMS POE
//ContractDocumentRepository

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Data.Repositories
{

    //this interface lets us do things with ContractDocument data in the database.
    public interface IContractDocumentRepository : IRepository<ContractDocument>
    {
        Task<List<ContractDocument>> GetDocumentsByContractAsync(int contractId);
        Task<ContractDocument?> GetCurrentDocumentByContractAsync(int contractId);
        Task<ContractDocument?> GetDocumentWithDetailsAsync(int documentId);
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
            return await _dbSet
                .AsNoTracking()
                .Where(d => d.ContractId == contractId)
                .Include(d => d.UploadedByUser)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        //..............................................................................//

        //gets the current/latest document for a contract.
        public async Task<ContractDocument?> GetCurrentDocumentByContractAsync(int contractId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(d => d.ContractId == contractId && d.IsCurrent)
                .Include(d => d.UploadedByUser)
                .FirstOrDefaultAsync();
        }

        //..............................................................................//

        //gets one document with all its info including the contract and who uploaded it.
        public async Task<ContractDocument?> GetDocumentWithDetailsAsync(int documentId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(d => d.Contract)
                .Include(d => d.UploadedByUser)
                .FirstOrDefaultAsync(d => d.ContractDocumentId == documentId);
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o........................................//
