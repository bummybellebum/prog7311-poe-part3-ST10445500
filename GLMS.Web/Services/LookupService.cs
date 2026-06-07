using GLMS.Web.ViewModels.Api;

//ST10445500 - PROG7311 - GLMS POE
//LookupService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services
{
    //manages lookup data by calling the GLMS API
    public interface ILookupService
    {
        Task<List<LookupDto>> GetContractStatusesAsync();
        Task<List<LookupDto>> GetServiceRequestStatusesAsync();
    }

    //..............................................................................//

    public class LookupService : ApiClientService, ILookupService
    {
        public LookupService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        //..............................................................................//

        public async Task<List<LookupDto>> GetContractStatusesAsync()
        {
            return await GetAsync<List<LookupDto>>("api/lookups/contract-statuses") ?? new List<LookupDto>();
        }

        //..............................................................................//

        public async Task<List<LookupDto>> GetServiceRequestStatusesAsync()
        {
            return await GetAsync<List<LookupDto>>("api/lookups/service-request-statuses") ?? new List<LookupDto>();
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o........................................//
