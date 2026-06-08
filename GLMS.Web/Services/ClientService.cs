using GLMS.Web.ApiModels;

//ST10445500 - PROG7311 - GLMS POE
//ClientService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services
{
    //manages Clients by calling the GLMS API
    public interface IClientService
    {
        Task<List<ClientListDto>> GetAllAsync(string? search = null);
        Task<ClientDetailDto?> GetByIdAsync(int id);
        Task<ClientDetailDto?> GetWithContractsAsync(int id);
        Task<ClientDetailDto> CreateAsync(CreateClientDto client);
        Task UpdateAsync(UpdateClientDto client);
        Task DeleteAsync(int id);
    }

    //..............................................................................//

    public class ClientService : ApiClientService, IClientService
    {
        public ClientService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        //..............................................................................//

        public async Task<List<ClientListDto>> GetAllAsync(string? search = null)
        {
            var url = string.IsNullOrWhiteSpace(search)
                ? "api/clients"
                : $"api/clients?search={Uri.EscapeDataString(search)}";

            return await GetAsync<List<ClientListDto>>(url) ?? new List<ClientListDto>();
        }

        //..............................................................................//

        public Task<ClientDetailDto?> GetByIdAsync(int id)
        {
            return GetAsync<ClientDetailDto>($"api/clients/{id}");
        }

        //..............................................................................//

        public Task<ClientDetailDto?> GetWithContractsAsync(int id)
        {
            return GetByIdAsync(id);
        }

        //..............................................................................//

        public Task<ClientDetailDto> CreateAsync(CreateClientDto client)
        {
            return PostAsync<ClientDetailDto>("api/clients", client);
        }

        //..............................................................................//

        public Task UpdateAsync(UpdateClientDto client)
        {
            return PutAsync($"api/clients/{client.ClientId}", client);
        }

        //..............................................................................//

        public Task DeleteAsync(int id)
        {
            return DeleteAsync($"api/clients/{id}");
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
