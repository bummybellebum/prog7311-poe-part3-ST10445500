using GLMS.Data.Repositories;
using GLMS.Models;

namespace GLMS.Services
{
    //manages lookup data for statuses and reference information
    public interface ILookupService
    {
        //retrieves all available contract statuses
        Task<List<ContractStatus>> GetContractStatusesAsync();

        //retrieves all available service request statuses
        Task<List<ServiceRequestStatus>> GetServiceRequestStatusesAsync();
    }

    //..............................................................................//

    //implements business logic for managing lookup data
    //provides reference information for statuses and other lookups
    public class LookupService : ILookupService
    {
        private readonly IRepository<ContractStatus> _contractStatusRepository;
        private readonly IRepository<ServiceRequestStatus> _serviceRequestStatusRepository;

        public LookupService(
            IRepository<ContractStatus> contractStatusRepository,
            IRepository<ServiceRequestStatus> serviceRequestStatusRepository)
        {
            _contractStatusRepository = contractStatusRepository;
            _serviceRequestStatusRepository = serviceRequestStatusRepository;
        }

        //..............................................................................//

        //retrieves all available contract statuses
        public async Task<List<ContractStatus>> GetContractStatusesAsync()
        {
            return await _contractStatusRepository.GetAllAsync();
        }

        //..............................................................................//

        //retrieves all available service request statuses
        public async Task<List<ServiceRequestStatus>> GetServiceRequestStatusesAsync()
        {
            return await _serviceRequestStatusRepository.GetAllAsync();
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o........................................//
