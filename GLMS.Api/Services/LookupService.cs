using GLMS.Api.Data.Repositories;
using GLMS.Api.DTOs;
using GLMS.Api.DTOs.Lookups;
using GLMS.Api.Models;

namespace GLMS.Api.Services
{
    //manages lookup data for statuses and reference information
    public interface ILookupService
    {
        //retrieves all available contract statuses
        Task<List<ContractStatus>> GetContractStatusesAsync();

        //retrieves contract status lookup DTOs
        Task<IReadOnlyList<LookupDto>> GetContractStatusLookupsAsync();

        //retrieves all available service request statuses
        Task<List<ServiceRequestStatus>> GetServiceRequestStatusesAsync();

        //retrieves service request status lookup DTOs
        Task<IReadOnlyList<LookupDto>> GetServiceRequestStatusLookupsAsync();

        //retrieves clients for lookup controls
        Task<IReadOnlyList<LookupDto>> GetClientLookupsAsync();

        //retrieves contracts for lookup controls
        Task<IReadOnlyList<LookupDto>> GetContractLookupsAsync();
    }

    //..............................................................................//

    //implements business logic for managing lookup data
    //provides reference information for statuses and other lookups
    public class LookupService : ILookupService
    {
        private readonly IRepository<ContractStatus> _contractStatusRepository;
        private readonly IRepository<ServiceRequestStatus> _serviceRequestStatusRepository;
        private readonly IClientRepository? _clientRepository;
        private readonly IContractRepository? _contractRepository;

        public LookupService(
            IRepository<ContractStatus> contractStatusRepository,
            IRepository<ServiceRequestStatus> serviceRequestStatusRepository,
            IClientRepository? clientRepository = null,
            IContractRepository? contractRepository = null)
        {
            _contractStatusRepository = contractStatusRepository;
            _serviceRequestStatusRepository = serviceRequestStatusRepository;
            _clientRepository = clientRepository;
            _contractRepository = contractRepository;
        }

        //..............................................................................//

        //retrieves all available contract statuses
        public async Task<List<ContractStatus>> GetContractStatusesAsync()
        {
            return await _contractStatusRepository.GetAllAsync();
        }

        //..............................................................................//

        //retrieves contract status lookup DTOs
        public async Task<IReadOnlyList<LookupDto>> GetContractStatusLookupsAsync()
        {
            var statuses = await GetContractStatusesAsync();
            return statuses.Select(status => status.ToLookupDto()).ToList();
        }

        //..............................................................................//

        //retrieves all available service request statuses
        public async Task<List<ServiceRequestStatus>> GetServiceRequestStatusesAsync()
        {
            return await _serviceRequestStatusRepository.GetAllAsync();
        }

        //..............................................................................//

        //retrieves service request status lookup DTOs
        public async Task<IReadOnlyList<LookupDto>> GetServiceRequestStatusLookupsAsync()
        {
            var statuses = await GetServiceRequestStatusesAsync();
            return statuses.Select(status => status.ToLookupDto()).ToList();
        }

        //..............................................................................//

        //retrieves clients for lookup controls
        public async Task<IReadOnlyList<LookupDto>> GetClientLookupsAsync()
        {
            if (_clientRepository == null)
                throw new InvalidOperationException("Client lookup repository is not configured.");

            var clients = await _clientRepository.GetClientsAsync();
            return clients
                .Select(client => new LookupDto
                {
                    Id = client.ClientId,
                    Name = client.CompanyName
                })
                .ToList();
        }

        //..............................................................................//

        //retrieves contracts for lookup controls
        public async Task<IReadOnlyList<LookupDto>> GetContractLookupsAsync()
        {
            if (_contractRepository == null)
                throw new InvalidOperationException("Contract lookup repository is not configured.");

            var contracts = await _contractRepository.GetAllAsync();
            return contracts
                .Select(contract => new LookupDto
                {
                    Id = contract.ContractId,
                    Name = contract.Title
                })
                .ToList();
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o........................................//

