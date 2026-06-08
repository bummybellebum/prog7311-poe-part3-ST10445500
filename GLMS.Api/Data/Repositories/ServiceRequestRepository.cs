using Microsoft.EntityFrameworkCore;
using GLMS.Api.Models;

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestRepository

//.....................................o0oSTART OF FILEo0o........................................//

// The repository keeps database query code in one layer instead of inside controllers.

namespace GLMS.Api.Data.Repositories
{

    public interface IServiceRequestRepository : IRepository<ServiceRequest>
    {
        Task<ServiceRequest?> GetServiceRequestWithDetailsAsync(int requestId);
        Task<List<ServiceRequest>> GetFilteredServiceRequestsAsync(int? contractId = null, int? statusId = null);
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
            return await GetServiceRequestListQuery()
                .OrderByDescending(sr => sr.RequestedAt)
                .ToListAsync();
        }

        //..............................................................................//


        //gets one service request with all its info including the contract, status, and who requested it.
        public async Task<ServiceRequest?> GetServiceRequestWithDetailsAsync(int requestId)
        {
            return await Query()
                .Include(sr => sr.Contract)
                    .ThenInclude(c => c.Client)
                .Include(sr => sr.ServiceRequestStatus)
                .Include(sr => sr.RequestedByUser)
                .FirstOrDefaultAsync(sr => sr.ServiceRequestId == requestId);
        }

        //..............................................................................//

        //gets service requests using optional list filters.
        public async Task<List<ServiceRequest>> GetFilteredServiceRequestsAsync(int? contractId = null, int? statusId = null)
        {
            var query = GetServiceRequestListQuery();

            if (contractId.HasValue)
                query = query.Where(sr => sr.ContractId == contractId.Value);

            if (statusId.HasValue)
                query = query.Where(sr => sr.ServiceRequestStatusId == statusId.Value);

            return await query
                .OrderByDescending(sr => sr.RequestedAt)
                .ToListAsync();
        }

        //..............................................................................//


        //gets all service requests that belong to a certain contract.

        public async Task<List<ServiceRequest>> GetServiceRequestsByContractAsync(int contractId)
        {
            return await GetFilteredServiceRequestsAsync(contractId: contractId);
        }

        //..............................................................................//

        //gets all service requests that have a certain status.
        public async Task<List<ServiceRequest>> GetServiceRequestsByStatusAsync(int statusId)
        {
            return await GetFilteredServiceRequestsAsync(statusId: statusId);
        }

        //..............................................................................//

        //gets all service requests that a specific user created.
        public async Task<List<ServiceRequest>> GetServiceRequestsByRequestedUserAsync(string userId)
        {
            return await Query()
                .Where(sr => sr.RequestedByUserId == userId)
                .Include(sr => sr.Contract)
                    .ThenInclude(c => c.Client)
                .Include(sr => sr.ServiceRequestStatus)
                .OrderByDescending(sr => sr.RequestedAt)
                .ToListAsync();
        }

        //..............................................................................//

        private IQueryable<ServiceRequest> GetServiceRequestListQuery()
        {
            return Query()
                .Include(sr => sr.Contract)
                    .ThenInclude(c => c.Client)
                .Include(sr => sr.ServiceRequestStatus);
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o........................................//

//.....................................o0oEND OF FILEo0o..........................................//
