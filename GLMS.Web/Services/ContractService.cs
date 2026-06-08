using GLMS.Web.ApiModels;

//ST10445500 - PROG7311 - GLMS POE
//ContractService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services
{
    //manages Contracts by calling the GLMS API
    public interface IContractService
    {
        Task<List<ContractListDto>> GetAllAsync();
        Task<ContractDetailDto?> GetByIdAsync(int id);
        Task<ContractDetailDto?> GetDetailsAsync(int id);
        Task<List<ContractListDto>> FilterAsync(int? statusId = null, DateTime? startDate = null, DateTime? endDate = null, int? clientId = null);
        Task<ContractDetailDto> CreateAsync(CreateContractDto contract);
        Task UpdateAsync(UpdateContractDto contract);
        Task UpdateStatusAsync(int id, int contractStatusId);
        Task DeleteAsync(int id);
    }

    //..............................................................................//

    public class ContractService : ApiClientService, IContractService
    {
        public ContractService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        //..............................................................................//

        public Task<List<ContractListDto>> GetAllAsync()
        {
            return FilterAsync();
        }

        //..............................................................................//

        public Task<ContractDetailDto?> GetByIdAsync(int id)
        {
            return GetAsync<ContractDetailDto>($"api/contracts/{id}");
        }

        //..............................................................................//

        public Task<ContractDetailDto?> GetDetailsAsync(int id)
        {
            return GetByIdAsync(id);
        }

        //..............................................................................//

        public async Task<List<ContractListDto>> FilterAsync(int? statusId = null, DateTime? startDate = null, DateTime? endDate = null, int? clientId = null)
        {
            var query = new List<string>();

            if (statusId.HasValue)
                query.Add($"statusId={statusId.Value}");

            if (startDate.HasValue)
                query.Add($"startDate={Uri.EscapeDataString(startDate.Value.ToString("O"))}");

            if (endDate.HasValue)
                query.Add($"endDate={Uri.EscapeDataString(endDate.Value.ToString("O"))}");

            if (clientId.HasValue)
                query.Add($"clientId={clientId.Value}");

            var url = query.Count == 0 ? "api/contracts" : $"api/contracts?{string.Join("&", query)}";
            return await GetAsync<List<ContractListDto>>(url) ?? new List<ContractListDto>();
        }

        //..............................................................................//

        public Task<ContractDetailDto> CreateAsync(CreateContractDto contract)
        {
            return PostAsync<ContractDetailDto>("api/contracts", contract);
        }

        //..............................................................................//

        public Task UpdateAsync(UpdateContractDto contract)
        {
            return PutAsync($"api/contracts/{contract.ContractId}", contract);
        }

        //..............................................................................//

        public Task UpdateStatusAsync(int id, int contractStatusId)
        {
            return PatchAsync($"api/contracts/{id}/status", new UpdateContractStatusDto { ContractStatusId = contractStatusId });
        }

        //..............................................................................//

        public Task DeleteAsync(int id)
        {
            return DeleteAsync($"api/contracts/{id}");
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
