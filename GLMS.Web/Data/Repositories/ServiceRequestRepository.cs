using Microsoft.EntityFrameworkCore;
using GLMS.Web.Models;

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestRepository

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Data.Repositories
{

    public interface IServiceRequestRepository : IRepository<ServiceRequest>
    {
        Task<ServiceRequest?> GetServiceRequestWithDetailsAsync(int requestId);
        Task<List<ServiceRequest>> GetServiceRequestsByContractAsync(int contractId);
        Task<List<ServiceRequest>> GetServiceRequestsByStatusAsync(int statusId);
        Task<List<ServiceRequest>> GetServiceRequestsByRequestedUserAsync(string userId);
    }

    //..............................................................................//

    //this class handles all the database operations for ServiceRequests
    //it has extra methods for things we do with service requests a lot.
    public class ServiceRequestRepository : Repository<ServiceRequest>, IServiceRequestRepository
    {
        public ServiceRequestRepository(ApplicationDbContext context) : base(context)
        {
        }

        //..............................................................................//

        public override async Task<List<ServiceRequest>> GetAllAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(sr => sr.Contract)
                    .ThenInclude(c => c.Client)
                .Include(sr => sr.ServiceRequestStatus)
                .OrderByDescending(sr => sr.RequestedAt)
                .ToListAsync();
        }

        //..............................................................................//


        //gets one service request with all its info including the contract, status, and who requested it.
        public async Task<ServiceRequest?> GetServiceRequestWithDetailsAsync(int requestId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(sr => sr.Contract)
                .Include(sr => sr.ServiceRequestStatus)
                .Include(sr => sr.RequestedByUser)
                .FirstOrDefaultAsync(sr => sr.ServiceRequestId == requestId);
        }

        //..............................................................................//


        //gets all service requests that belong to a certain contract.

        public async Task<List<ServiceRequest>> GetServiceRequestsByContractAsync(int contractId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(sr => sr.ContractId == contractId)
                .Include(sr => sr.ServiceRequestStatus)
                .Include(sr => sr.RequestedByUser)
                .OrderByDescending(sr => sr.RequestedAt)
                .ToListAsync();
        }

        //..............................................................................//

        //gets all service requests that have a certain status.
        public async Task<List<ServiceRequest>> GetServiceRequestsByStatusAsync(int statusId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(sr => sr.ServiceRequestStatusId == statusId)
                .Include(sr => sr.Contract)
                .Include(sr => sr.RequestedByUser)
                .OrderByDescending(sr => sr.RequestedAt)
                .ToListAsync();
        }

        //..............................................................................//

        //gets all service requests that a specific user created.
        public async Task<List<ServiceRequest>> GetServiceRequestsByRequestedUserAsync(string userId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(sr => sr.RequestedByUserId == userId)
                .Include(sr => sr.Contract)
                .Include(sr => sr.ServiceRequestStatus)
                .OrderByDescending(sr => sr.RequestedAt)
                .ToListAsync();
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o........................................//
