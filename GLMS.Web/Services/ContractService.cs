using GLMS.Web.Models;

//ST10445500 - PROG7311 - GLMS POE
//ContractService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services
{
    //manages Contracts by calling the GLMS API
    public interface IContractService
    {
        Task<List<Contract>> GetAllAsync();
        Task<Contract?> GetByIdAsync(int id);
        Task<Contract?> GetDetailsAsync(int id);
        Task<List<Contract>> FilterAsync(int? statusId = null, DateTime? startDate = null, DateTime? endDate = null, int? clientId = null);
        Task<Contract> CreateAsync(Contract contract);
        Task UpdateAsync(Contract contract);
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

        public Task<List<Contract>> GetAllAsync()
        {
            return FilterAsync();
        }

        //..............................................................................//

        public Task<Contract?> GetByIdAsync(int id)
        {
            return GetAsync<Contract>($"api/contracts/{id}");
        }

        //..............................................................................//

        public Task<Contract?> GetDetailsAsync(int id)
        {
            return GetByIdAsync(id);
        }

        //..............................................................................//

        public async Task<List<Contract>> FilterAsync(int? statusId = null, DateTime? startDate = null, DateTime? endDate = null, int? clientId = null)
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
            return await GetAsync<List<Contract>>(url) ?? new List<Contract>();
        }

        //..............................................................................//

        public Task<Contract> CreateAsync(Contract contract)
        {
            return PostAsync<Contract>("api/contracts", contract);
        }

        //..............................................................................//

        public Task UpdateAsync(Contract contract)
        {
            return PutAsync($"api/contracts/{contract.ContractId}", contract);
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
