using GLMS.Api.Data.Repositories;
using GLMS.Api.DTOs;
using GLMS.Api.DTOs.Clients;
using GLMS.Api.Models;

//ST10445500 - PROG7311 - GLMS POE
//ClientService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Services
{
    //manages business logic for Clients
    public interface IClientService
    {
        //retrieves all clients from the database
        Task<List<Client>> GetAllAsync(string? search = null);

        //retrieves client list response DTOs
        Task<IReadOnlyList<ClientListDto>> GetListAsync(string? search = null);

        //retrieves a single client by ID
        Task<Client?> GetByIdAsync(int id);

        //retrieves a client with all associated contracts
        Task<Client?> GetWithContractsAsync(int id);

        //retrieves a client detail response DTO
        Task<ClientDetailDto?> GetDetailDtoAsync(int id);

        //creates a new client record
        Task<Client> CreateAsync(Client client);

        //creates a new client from a request DTO
        Task<ClientListDto> CreateAsync(CreateClientDto dto);

        //updates an existing client record
        Task UpdateAsync(Client client);

        //updates an existing client from a request DTO
        Task<ClientDetailDto> UpdateAsync(int id, UpdateClientDto dto);

        //removes a client record from the database
        Task DeleteAsync(int id);
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

        //retrieves all clients from the database.
        public async Task<List<Client>> GetAllAsync(string? search = null)
        {
            return await _clientRepository.GetClientsAsync(search);
        }

        //..............................................................................//

        //retrieves client list response DTOs
        public async Task<IReadOnlyList<ClientListDto>> GetListAsync(string? search = null)
        {
            var clients = await GetAllAsync(search);
            return clients.Select(client => client.ToListDto()).ToList();
        }

        //..............................................................................//

        //retrieves a single client by ID. returns null if invalid or not found
        public async Task<Client?> GetByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _clientRepository.GetByIdAsync(id);
        }

        //..............................................................................//

        //retrieves a client with all associated contracts
        public async Task<Client?> GetWithContractsAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _clientRepository.GetClientWithContractsAsync(id);
        }

        //..............................................................................//

        //retrieves a client detail response DTO
        public async Task<ClientDetailDto?> GetDetailDtoAsync(int id)
        {
            var client = await GetWithContractsAsync(id);
            return client?.ToDetailDto();
        }

        //..............................................................................//

        //creates a new client. Validates all fields and ensures company name uniqueness
        public async Task<Client> CreateAsync(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (string.IsNullOrWhiteSpace(client.CompanyName))
                throw new ArgumentException("Company name is required.", nameof(client.CompanyName));

            if (string.IsNullOrWhiteSpace(client.Email))
                throw new ArgumentException("Email is required.", nameof(client.Email));

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

        //creates a new client from a request DTO
        public async Task<ClientListDto> CreateAsync(CreateClientDto dto)
        {
            var created = await CreateAsync(dto.ToEntity());
            return created.ToListDto();
        }

        //..............................................................................//

        //updates an existing client, validates all fields and ensures company name uniqueness
        public async Task UpdateAsync(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (client.ClientId <= 0)
                throw new ArgumentException("Invalid client ID.", nameof(client.ClientId));

            if (string.IsNullOrWhiteSpace(client.CompanyName))
                throw new ArgumentException("Company name is required.", nameof(client.CompanyName));

            if (string.IsNullOrWhiteSpace(client.Email))
                throw new ArgumentException("Email is required.", nameof(client.Email));

            var existing = await _clientRepository.GetByIdAsync(client.ClientId);
            if (existing == null)
                throw new KeyNotFoundException($"Client with ID {client.ClientId} not found.");

            bool isUnique = await _clientRepository.IsCompanyNameUniqueAsync(client.CompanyName, excludeClientId: client.ClientId);
            if (!isUnique)
                throw new InvalidOperationException($"A client with company name '{client.CompanyName}' already exists.");

            client.UpdatedAt = DateTime.UtcNow;

            _clientRepository.Update(client);
            await _clientRepository.SaveChangesAsync();
        }

        //..............................................................................//

        //updates an existing client from a request DTO
        public async Task<ClientDetailDto> UpdateAsync(int id, UpdateClientDto dto)
        {
            if (id != dto.ClientId)
                throw new ArgumentException("Client ID does not match.");

            var client = await GetByIdAsync(id);
            if (client == null)
                throw new KeyNotFoundException($"Client with ID {id} not found.");

            dto.ApplyTo(client);
            await UpdateAsync(client);

            var detail = await GetWithContractsAsync(id);
            if (detail == null)
                throw new KeyNotFoundException($"Client with ID {id} not found.");

            return detail.ToDetailDto();
        }

        //..............................................................................//

        //removes a client record from the database
        public async Task DeleteAsync(int id)
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
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
