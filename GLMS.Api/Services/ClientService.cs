using GLMS.Api.Data.Repositories;
using GLMS.Api.DTOs.Clients;
using GLMS.Api.DTOs.Mappings;
using GLMS.Api.Models;



namespace GLMS.Api.Services
{
    //manages business logic for Clients
    public interface IClientService
    {
        //retrieves client list response DTOs
        Task<IReadOnlyList<ClientListDto>> GetClientsAsync(string? search = null);

        //retrieves a client detail response DTO
        Task<ClientDetailDto?> GetClientAsync(int id);

        //creates a new client from a request DTO
        Task<ClientListDto> CreateClientAsync(CreateClientDto dto);

        //updates an existing client from a request DTO
        Task<ClientDetailDto> UpdateClientAsync(int id, UpdateClientDto dto);

        //removes a client record from the database
        Task DeleteClientAsync(int id);
    }

    //..............................................................................//

    //implements business logic for managing Clients
    //validates data and coordinates with the repository layer.
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;

        public ClientService(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        //..............................................................................//

        //retrieves client list response DTOs
        public async Task<IReadOnlyList<ClientListDto>> GetClientsAsync(string? search = null)
        {
            var clients = await _clientRepository.GetClientsAsync(search);
            return clients.Select(client => client.ToListDto()).ToList();
        }

        //..............................................................................//

        //retrieves a client detail response DTO
        public async Task<ClientDetailDto?> GetClientAsync(int id)
        {
            if (id <= 0)
                return null;

            var client = await _clientRepository.GetClientWithContractsAsync(id);
            return client?.ToDetailDto();
        }

        //..............................................................................//

        //creates a new client from a request DTO
        public async Task<ClientListDto> CreateClientAsync(CreateClientDto dto)
        {
            var created = await CreateClientRecordAsync(dto.ToEntity());
            return created.ToListDto();
        }

        //..............................................................................//

        //updates an existing client from a request DTO
        public async Task<ClientDetailDto> UpdateClientAsync(int id, UpdateClientDto dto)
        {
            if (id != dto.ClientId)
                throw new ArgumentException("Client ID does not match.");

            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
                throw new KeyNotFoundException($"Client with ID {id} not found.");

            dto.ApplyTo(client);
            await UpdateClientRecordAsync(client);

            var detail = await _clientRepository.GetClientWithContractsAsync(id);
            if (detail == null)
                throw new KeyNotFoundException($"Client with ID {id} not found.");

            return detail.ToDetailDto();
        }

        //..............................................................................//

        //removes a client record from the database
        public async Task DeleteClientAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid client ID.", nameof(id));

            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
                throw new KeyNotFoundException($"Client with ID {id} not found.");

            _clientRepository.Delete(client);
            await _clientRepository.SaveChangesAsync();
        }

        //..............................................................................//

        //creates a new client record after validation
        private async Task<Client> CreateClientRecordAsync(Client client)
        {
            ValidateClient(client);

            bool isUnique = await _clientRepository.IsCompanyNameUniqueAsync(client.CompanyName);
            if (!isUnique)
                throw new InvalidOperationException($"A client with company name '{client.CompanyName}' already exists.");

            client.CreatedAt = DateTime.UtcNow;
            client.UpdatedAt = DateTime.UtcNow;

            await _clientRepository.AddAsync(client);
            await _clientRepository.SaveChangesAsync();

            return client;
        }

        //..............................................................................//

        //updates an existing client record after validation
        private async Task UpdateClientRecordAsync(Client client)
        {
            ValidateClient(client);

            if (client.ClientId <= 0)
                throw new ArgumentException("Invalid client ID.", nameof(client.ClientId));

            bool isUnique = await _clientRepository.IsCompanyNameUniqueAsync(client.CompanyName, excludeClientId: client.ClientId);
            if (!isUnique)
                throw new InvalidOperationException($"A client with company name '{client.CompanyName}' already exists.");

            client.UpdatedAt = DateTime.UtcNow;

            _clientRepository.Update(client);
            await _clientRepository.SaveChangesAsync();
        }

        //..............................................................................//

        //checks the required client fields
        private static void ValidateClient(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (string.IsNullOrWhiteSpace(client.CompanyName))
                throw new ArgumentException("Company name is required.", nameof(client.CompanyName));

            if (string.IsNullOrWhiteSpace(client.Email))
                throw new ArgumentException("Email is required.", nameof(client.Email));
        }

        //..............................................................................//
    }
}
