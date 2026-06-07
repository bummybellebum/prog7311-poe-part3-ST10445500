using GLMS.Api.Data.Repositories;
using GLMS.Api.DTOs.Contracts;
using GLMS.Api.DTOs.Mappings;
using GLMS.Api.Models;



namespace GLMS.Api.Services
{
    //manages business logic for Contracts
    public interface IContractService
    {
        //retrieves filtered contract response DTOs
        Task<IReadOnlyList<ContractListDto>> GetContractsAsync(int? statusId = null, DateTime? startDate = null, DateTime? endDate = null, int? clientId = null);

        //retrieves a contract detail response DTO
        Task<ContractDetailDto?> GetContractAsync(int id);

        //creates a new contract from a request DTO
        Task<ContractDetailDto> CreateContractAsync(CreateContractDto dto);

        //updates an existing contract from a request DTO
        Task<ContractDetailDto> UpdateContractAsync(int id, UpdateContractDto dto);

        //updates only the contract status from a request DTO
        Task<ContractDetailDto> UpdateContractStatusAsync(int id, UpdateContractStatusDto dto);

        //removes a contract record from the database
        Task DeleteContractAsync(int id);
    }

    //..............................................................................//

    //implements business logic for managing Contracts
    //validates data and coordinates with the repository layer
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IRepository<ContractStatus> _contractStatusRepository;
        private readonly ICurrentUserService? _currentUserService;

        public ContractService(
            IContractRepository contractRepository,
            IClientRepository clientRepository,
            IRepository<ContractStatus> contractStatusRepository,
            ICurrentUserService? currentUserService = null)
        {
            _contractRepository = contractRepository;
            _clientRepository = clientRepository;
            _contractStatusRepository = contractStatusRepository;
            _currentUserService = currentUserService;
        }

        //..............................................................................//

        //retrieves filtered contract response DTOs
        public async Task<IReadOnlyList<ContractListDto>> GetContractsAsync(
            int? statusId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? clientId = null)
        {
            var contracts = await _contractRepository.GetFilteredContractsAsync(statusId, startDate, endDate, clientId);
            return contracts.Select(contract => contract.ToListDto()).ToList();
        }

        //..............................................................................//

        //retrieves a contract detail response DTO
        public async Task<ContractDetailDto?> GetContractAsync(int id)
        {
            if (id <= 0)
                return null;

            var contract = await _contractRepository.GetContractWithDocumentsAndRequestsAsync(id);
            return contract?.ToDetailDto();
        }

        //..............................................................................//

        //creates a new contract from a request DTO
        public async Task<ContractDetailDto> CreateContractAsync(CreateContractDto dto)
        {
            var created = await CreateContractRecordAsync(dto.ToEntity(GetCurrentUserId()));
            var detail = await _contractRepository.GetContractWithDocumentsAndRequestsAsync(created.ContractId);
            return (detail ?? created).ToDetailDto();
        }

        //..............................................................................//

        //updates an existing contract from a request DTO
        public async Task<ContractDetailDto> UpdateContractAsync(int id, UpdateContractDto dto)
        {
            if (id != dto.ContractId)
                throw new ArgumentException("Contract ID does not match.");

            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with ID {id} not found.");

            dto.ApplyTo(contract);
            await UpdateContractRecordAsync(contract);

            var detail = await _contractRepository.GetContractWithDocumentsAndRequestsAsync(id);
            if (detail == null)
                throw new KeyNotFoundException($"Contract with ID {id} not found.");

            return detail.ToDetailDto();
        }

        //..............................................................................//

        //updates only the contract status from a request DTO
        public async Task<ContractDetailDto> UpdateContractStatusAsync(int id, UpdateContractStatusDto dto)
        {
            await UpdateContractStatusRecordAsync(id, dto.ContractStatusId);

            var detail = await _contractRepository.GetContractWithDocumentsAndRequestsAsync(id);
            if (detail == null)
                throw new KeyNotFoundException($"Contract with ID {id} not found.");

            return detail.ToDetailDto();
        }

        //..............................................................................//

        //removes a contract record from the database
        public async Task DeleteContractAsync(int id)
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

        //creates a new contract record after validation
        private async Task<Contract> CreateContractRecordAsync(Contract contract)
        {
            ValidateContract(contract);

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

        //updates an existing contract record after validation
        private async Task UpdateContractRecordAsync(Contract contract)
        {
            ValidateContract(contract);

            if (contract.ContractId <= 0)
                throw new ArgumentException("Invalid contract ID.", nameof(contract.ContractId));

            var client = await _clientRepository.GetByIdAsync(contract.ClientId);
            if (client == null)
                throw new KeyNotFoundException($"Client with ID {contract.ClientId} not found.");

            contract.UpdatedAt = DateTime.UtcNow;

            _contractRepository.Update(contract);
            await _contractRepository.SaveChangesAsync();
        }

        //..............................................................................//

        //updates only the contract status
        private async Task UpdateContractStatusRecordAsync(int id, int statusId)
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

        //checks the required contract fields
        private static void ValidateContract(Contract contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (string.IsNullOrWhiteSpace(contract.Title))
                throw new ArgumentException("Title is required.", nameof(contract.Title));

            if (contract.ClientId <= 0)
                throw new ArgumentException("Valid client ID is required.", nameof(contract.ClientId));

            if (contract.StartDate >= contract.EndDate)
                throw new ArgumentException("Start date must be before end date.");
        }

        //..............................................................................//

        private string GetCurrentUserId()
        {
            return _currentUserService?.UserId
                ?? throw new InvalidOperationException("Unable to identify the signed-in user.");
        }

        //..............................................................................//
    }
}
