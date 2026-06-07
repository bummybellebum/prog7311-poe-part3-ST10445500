using GLMS.Web.Models;

//ST10445500 - PROG7311 - GLMS POE
//ClientService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services
{
    //manages Clients by calling the GLMS API
    public interface IClientService
    {
        Task<List<Client>> GetAllAsync();
        Task<Client?> GetByIdAsync(int id);
        Task<Client?> GetWithContractsAsync(int id);
        Task<Client> CreateAsync(Client client);
        Task UpdateAsync(Client client);
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

        public async Task<List<Client>> GetAllAsync()
        {
            return await GetAsync<List<Client>>("api/clients") ?? new List<Client>();
        }

        //..............................................................................//

        public Task<Client?> GetByIdAsync(int id)
        {
            return GetAsync<Client>($"api/clients/{id}");
        }

        //..............................................................................//

        public Task<Client?> GetWithContractsAsync(int id)
        {
            return GetByIdAsync(id);
        }

        //..............................................................................//

        public Task<Client> CreateAsync(Client client)
        {
            return PostAsync<Client>("api/clients", client);
        }

        //..............................................................................//

        public Task UpdateAsync(Client client)
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
