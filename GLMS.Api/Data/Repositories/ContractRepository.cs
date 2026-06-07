using GLMS.Api.Models;
using Microsoft.EntityFrameworkCore;

//ST10445500 - PROG7311 - GLMS POE
//ContractRepository

//.....................................o0oSTART OF FILEo0o........................................//
namespace GLMS.Api.Data.Repositories
{
	//this interface lets us do things with Contract data in the database.
	public interface IContractRepository : IRepository<Contract>
	{
		Task<Contract?> GetContractWithDetailsAsync(int contractId);
		Task<Contract?> GetContractWithDocumentsAndRequestsAsync(int contractId);
		Task<List<Contract>> GetContractsByStatusAsync(int statusId);
		Task<List<Contract>> GetContractsByClientAsync(int clientId);
		Task<List<Contract>> GetContractsByDateRangeAsync(DateTime startDate, DateTime endDate);
	}

	//..............................................................................//

	//this class handles all the database operations for Contracts
	//it has extra methods for things we do with contracts a lot.
	public class ContractRepository : Repository<Contract>, IContractRepository
	{
		public ContractRepository(ApplicationDbContext context) : base(context)
		{
		}

		//..............................................................................//

		public override async Task<List<Contract>> GetAllAsync()
		{
			return await _dbSet
				.AsNoTracking()
				.Include(c => c.Client)
				.Include(c => c.ContractStatus)
				.OrderByDescending(c => c.CreatedAt)
				.ToListAsync();
		}

		//..............................................................................//

		//gets one contract with the client info and status info
		public async Task<Contract?> GetContractWithDetailsAsync(int contractId)
		{
			return await _dbSet
				.AsNoTracking()
				.Include(c => c.Client)
				.Include(c => c.ContractStatus)
				.Include(c => c.CreatedByUser)
				.FirstOrDefaultAsync(c => c.ContractId == contractId);
		}

		//..............................................................................//

		//gets one contract with all its documents and service requests.
		public async Task<Contract?> GetContractWithDocumentsAndRequestsAsync(int contractId)
		{
			return await _dbSet
				.AsNoTracking()
				.Include(c => c.Client)
				.Include(c => c.ContractStatus)
				.Include(c => c.CreatedByUser)
				.Include(c => c.Documents)
				.Include(c => c.ServiceRequests)
					.ThenInclude(sr => sr.ServiceRequestStatus)
				.FirstOrDefaultAsync(c => c.ContractId == contractId);
		}

		//..............................................................................//

		//gets all contracts that have a certain status.
		public async Task<List<Contract>> GetContractsByStatusAsync(int statusId)
		{
			return await _dbSet
				.AsNoTracking()
				.Where(c => c.ContractStatusId == statusId)
				.Include(c => c.Client)
				.Include(c => c.ContractStatus)
				.ToListAsync();
		}

		//...............................................................................//

		//gets all contracts that belong to a specific client.
		public async Task<List<Contract>> GetContractsByClientAsync(int clientId)
		{
			return await _dbSet
				.AsNoTracking()
				.Where(c => c.ClientId == clientId)
				.Include(c => c.ContractStatus)
				.Include(c => c.CreatedByUser)
				.ToListAsync();
		}

		//...............................................................................//

		//gets all contracts that started between two dates.
		public async Task<List<Contract>> GetContractsByDateRangeAsync(DateTime startDate, DateTime endDate)
		{
			return await _dbSet
				.AsNoTracking()
				.Where(c => c.StartDate >= startDate && c.StartDate <= endDate)
				.Include(c => c.Client)
				.Include(c => c.ContractStatus)
				.OrderBy(c => c.StartDate)
				.ToListAsync();
		}

		//...............................................................................//
	}
}

//.....................................o0oEND OF FILEo0o........................................//
