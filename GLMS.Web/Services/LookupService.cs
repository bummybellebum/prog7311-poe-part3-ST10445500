using GLMS.Web.Models;

//ST10445500 - PROG7311 - GLMS POE
//LookupService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services
{
    //manages lookup data by calling the GLMS API
    public interface ILookupService
    {
        Task<List<ContractStatus>> GetContractStatusesAsync();
        Task<List<ServiceRequestStatus>> GetServiceRequestStatusesAsync();
    }

    //..............................................................................//

    public class LookupService : ApiClientService, ILookupService
    {
        public LookupService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        //..............................................................................//

        public async Task<List<ContractStatus>> GetContractStatusesAsync()
        {
            return await GetAsync<List<ContractStatus>>("api/lookups/contract-statuses") ?? new List<ContractStatus>();
        }

        //..............................................................................//

        public async Task<List<ServiceRequestStatus>> GetServiceRequestStatusesAsync()
        {
            return await GetAsync<List<ServiceRequestStatus>>("api/lookups/service-request-statuses") ?? new List<ServiceRequestStatus>();
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o........................................//
