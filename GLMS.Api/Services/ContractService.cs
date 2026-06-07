using GLMS.Api.Data.Repositories;
using GLMS.Api.Models;

//ST10445500 - PROG7311 - GLMS POE
//ContractService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Services
{
    //manages business logic for Contracts
    public interface IContractService
    {

        //retrieves all contracts from the database
        Task<List<Contract>> GetAllAsync();


        //retrieves a single contract by ID
        Task<Contract?> GetByIdAsync(int id);

        //retrieves a contract with all associated details
        Task<Contract?> GetDetailsAsync(int id);

        //retrieves contracts filtered by optional status, date range, and client criteria.
        Task<List<Contract>> FilterAsync(int? statusId = null, DateTime? startDate = null, DateTime? endDate = null, int? clientId = null);

        //creates a new contract record
        Task<Contract> CreateAsync(Contract contract);

        //updates an existing contract record
        Task UpdateAsync(Contract contract);

        //updates only the contract status
        Task UpdateStatusAsync(int id, int statusId);

        //removes a contract record from the database
        Task DeleteAsync(int id);
    }

    //..............................................................................//

    //implements business logic for managing Contracts
    //validates data and coordinates with the repository layer
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IRepository<ContractStatus> _contractStatusRepository;

        public ContractService(
            IContractRepository contractRepository,
            IClientRepository clientRepository,
            IRepository<ContractStatus> contractStatusRepository)
        {
            _contractRepository = contractRepository;
            _clientRepository = clientRepository;
            _contractStatusRepository = contractStatusRepository;
        }

        //..............................................................................//

        //retrieves all contracts from the database
        public async Task<List<Contract>> GetAllAsync()
        {
            return await _contractRepository.GetAllAsync();
        }

        //..............................................................................//


        //retrieves a single contract by ID. Returns null if invalid or not found
        public async Task<Contract?> GetByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _contractRepository.GetByIdAsync(id);
        }

        //..............................................................................//

        //retrieves a contract with all associated details
        public async Task<Contract?> GetDetailsAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _contractRepository.GetContractWithDocumentsAndRequestsAsync(id);
        }

        //..............................................................................//

        //retrieves contracts filtered by optional criteria
        public async Task<List<Contract>> FilterAsync(
            int? statusId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? clientId = null)
        {
            return await _contractRepository.GetFilteredContractsAsync(statusId, startDate, endDate, clientId);
        }

        //..............................................................................//

        //creates a new contract, validates all fields
        public async Task<Contract> CreateAsync(Contract contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (string.IsNullOrWhiteSpace(contract.Title))
                throw new ArgumentException("Title is required.", nameof(contract.Title));

            if (contract.ClientId <= 0)
                throw new ArgumentException("Valid client ID is required.", nameof(contract.ClientId));

            if (contract.StartDate >= contract.EndDate)
                throw new ArgumentException("Start date must be before end date.");

            var client = await _clientRepository.GetByIdAsync(contract.ClientId);
            if (client == null)
                throw new KeyNotFoundException($"Client with ID {contract.ClientId} not found.");

            contract.CreatedAt = DateTime.UtcNow;
            contract.UpdatedAt = DateTime.UtcNow;

            await _contractRepository.AddAsync(contract);
            await _contractRepository.SaveChangesAsync();

            return contract;
        }

        //..............................................................................//

        //updates an existing contract, validates all fields
        public async Task UpdateAsync(Contract contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (contract.ContractId <= 0)
                throw new ArgumentException("Invalid contract ID.", nameof(contract.ContractId));

            if (string.IsNullOrWhiteSpace(contract.Title))
                throw new ArgumentException("Title is required.", nameof(contract.Title));

            if (contract.StartDate >= contract.EndDate)
                throw new ArgumentException("Start date must be before end date.");

            var existing = await _contractRepository.GetByIdAsync(contract.ContractId);
            if (existing == null)
                throw new KeyNotFoundException($"Contract with ID {contract.ContractId} not found.");

            var client = await _clientRepository.GetByIdAsync(contract.ClientId);
            if (client == null)
                throw new KeyNotFoundException($"Client with ID {contract.ClientId} not found.");

            contract.UpdatedAt = DateTime.UtcNow;

            _contractRepository.Update(contract);
            await _contractRepository.SaveChangesAsync();
        }

        //..............................................................................//

        //updates only the contract status
        public async Task UpdateStatusAsync(int id, int statusId)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid contract ID.", nameof(id));

            if (statusId <= 0)
                throw new ArgumentException("Valid contract status ID is required.", nameof(statusId));

            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with ID {id} not found.");

            var status = await _contractStatusRepository.GetByIdAsync(statusId);
            if (status == null)
                throw new ArgumentException($"Contract status with ID {statusId} was not found.", nameof(statusId));

            contract.ContractStatusId = status.ContractStatusId;
            contract.ContractStatus = status;
            contract.UpdatedAt = DateTime.UtcNow;

            _contractRepository.Update(contract);
            await _contractRepository.SaveChangesAsync();
        }

        //..............................................................................//
        
        //removes a contract record from the database
        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid contract ID.", nameof(id));

            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with ID {id} not found.");

            _contractRepository.Delete(contract);
            await _contractRepository.SaveChangesAsync();
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//

