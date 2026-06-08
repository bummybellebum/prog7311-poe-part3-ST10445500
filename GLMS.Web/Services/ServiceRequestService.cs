using GLMS.Web.ApiModels;

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services
{
    //manages Service Requests by calling the GLMS API
    public interface IServiceRequestService
    {
        Task<List<ServiceRequestListDto>> GetAllAsync(int? contractId = null, int? statusId = null);
        Task<ServiceRequestDetailDto?> GetByIdAsync(int id);
        Task<ServiceRequestDetailDto?> GetDetailsAsync(int id);
        Task<List<ServiceRequestListDto>> GetByContractIdAsync(int contractId);
        Task<ServiceRequestDetailDto> CreateAsync(CreateServiceRequestDto serviceRequest);
        Task UpdateAsync(UpdateServiceRequestDto serviceRequest);
        Task UpdateStatusAsync(int id, int serviceRequestStatusId);
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

        public async Task<List<ServiceRequestListDto>> GetAllAsync(int? contractId = null, int? statusId = null)
        {
            var query = new List<string>();

            if (contractId.HasValue)
                query.Add($"contractId={contractId.Value}");

            if (statusId.HasValue)
                query.Add($"statusId={statusId.Value}");

            var url = query.Count == 0 ? "api/service-requests" : $"api/service-requests?{string.Join("&", query)}";
            return await GetAsync<List<ServiceRequestListDto>>(url) ?? new List<ServiceRequestListDto>();
        }

        //..............................................................................//

        public Task<ServiceRequestDetailDto?> GetByIdAsync(int id)
        {
            return GetAsync<ServiceRequestDetailDto>($"api/service-requests/{id}");
        }

        //..............................................................................//

        public Task<ServiceRequestDetailDto?> GetDetailsAsync(int id)
        {
            return GetByIdAsync(id);
        }

        //..............................................................................//

        public async Task<List<ServiceRequestListDto>> GetByContractIdAsync(int contractId)
        {
            return await GetAllAsync(contractId);
        }

        //..............................................................................//

        public Task<ServiceRequestDetailDto> CreateAsync(CreateServiceRequestDto serviceRequest)
        {
            return PostAsync<ServiceRequestDetailDto>("api/service-requests", serviceRequest);
        }

        //..............................................................................//

        public Task UpdateAsync(UpdateServiceRequestDto serviceRequest)
        {
            return PutAsync($"api/service-requests/{serviceRequest.ServiceRequestId}", serviceRequest);
        }

        //..............................................................................//

        public Task UpdateStatusAsync(int id, int serviceRequestStatusId)
        {
            return PatchAsync($"api/service-requests/{id}/status", new UpdateServiceRequestStatusDto { ServiceRequestStatusId = serviceRequestStatusId });
        }

        //..............................................................................//

        public Task DeleteAsync(int id)
        {
            return DeleteAsync($"api/service-requests/{id}");
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
