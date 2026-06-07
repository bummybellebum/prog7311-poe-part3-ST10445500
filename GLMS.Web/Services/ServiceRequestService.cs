using GLMS.Web.Models;

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services
{
    //manages Service Requests by calling the GLMS API
    public interface IServiceRequestService
    {
        Task<List<ServiceRequest>> GetAllAsync();
        Task<ServiceRequest?> GetByIdAsync(int id);
        Task<ServiceRequest?> GetDetailsAsync(int id);
        Task<List<ServiceRequest>> GetByContractIdAsync(int contractId);
        Task<ServiceRequest> CreateAsync(ServiceRequest serviceRequest);
        Task UpdateAsync(ServiceRequest serviceRequest);
        Task DeleteAsync(int id);
    }

    //..............................................................................//

    public class ServiceRequestService : ApiClientService, IServiceRequestService
    {
        public ServiceRequestService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        //..............................................................................//

        public async Task<List<ServiceRequest>> GetAllAsync()
        {
            return await GetAsync<List<ServiceRequest>>("api/servicerequests") ?? new List<ServiceRequest>();
        }

        //..............................................................................//

        public Task<ServiceRequest?> GetByIdAsync(int id)
        {
            return GetAsync<ServiceRequest>($"api/servicerequests/{id}");
        }

        //..............................................................................//

        public Task<ServiceRequest?> GetDetailsAsync(int id)
        {
            return GetByIdAsync(id);
        }

        //..............................................................................//

        public async Task<List<ServiceRequest>> GetByContractIdAsync(int contractId)
        {
            return await GetAsync<List<ServiceRequest>>($"api/servicerequests?contractId={contractId}") ?? new List<ServiceRequest>();
        }

        //..............................................................................//

        public Task<ServiceRequest> CreateAsync(ServiceRequest serviceRequest)
        {
            return PostAsync<ServiceRequest>("api/servicerequests", serviceRequest);
        }

        //..............................................................................//

        public Task UpdateAsync(ServiceRequest serviceRequest)
        {
            return PutAsync($"api/servicerequests/{serviceRequest.ServiceRequestId}", serviceRequest);
        }

        //..............................................................................//

        public Task DeleteAsync(int id)
        {
            return DeleteAsync($"api/servicerequests/{id}");
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
