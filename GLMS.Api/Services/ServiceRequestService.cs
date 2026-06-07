using GLMS.Api.Data.Repositories;
using GLMS.Api.DTOs.Mappings;
using GLMS.Api.DTOs.ServiceRequests;
using GLMS.Api.Models;



namespace GLMS.Api.Services
{
    //manages business logic for Service Requests
    public interface IServiceRequestService
    {
        //retrieves all service requests from the database
        Task<List<ServiceRequest>> GetAllAsync(int? contractId = null, int? statusId = null);

        //retrieves service request list response DTOs
        Task<IReadOnlyList<ServiceRequestListDto>> GetListAsync(int? contractId = null, int? statusId = null);

        //retrieves a single service request by ID.
        Task<ServiceRequest?> GetByIdAsync(int id);

        //retrieves a service request with all associated details
        Task<ServiceRequest?> GetDetailsAsync(int id);

        //retrieves a service request detail response DTO
        Task<ServiceRequestDetailDto?> GetDetailDtoAsync(int id);

        //retrieves all service requests associated with a contract
        Task<List<ServiceRequest>> GetByContractIdAsync(int contractId);

        //creates a new service request record
        Task<ServiceRequest> CreateAsync(ServiceRequest serviceRequest);

        //creates a new service request from a request DTO
        Task<ServiceRequestDetailDto> CreateAsync(CreateServiceRequestDto dto);

        //updates an existing service request record
        Task UpdateAsync(ServiceRequest serviceRequest);

        //updates an existing service request from a request DTO
        Task<ServiceRequestDetailDto> UpdateAsync(int id, UpdateServiceRequestDto dto);

        //updates only the service request status
        Task UpdateStatusAsync(int id, int statusId);

        //updates only the service request status from a request DTO
        Task<ServiceRequestDetailDto> UpdateStatusAsync(int id, UpdateServiceRequestStatusDto dto);

        //removes a service request record from the database
        Task DeleteAsync(int id);
    }

    //..............................................................................//

    //implements business logic for managing Service Requests
    //validates data and coordinates with the repository layer
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IServiceRequestRepository _serviceRequestRepository;
        private readonly IContractRepository _contractRepository;
        private readonly ICurrencyExchangeService _currencyExchangeService;
        private readonly ICurrentUserService? _currentUserService;

        public ServiceRequestService(
            IServiceRequestRepository serviceRequestRepository,
            IContractRepository contractRepository,
            ICurrencyExchangeService currencyExchangeService,
            ICurrentUserService? currentUserService = null)
        {
            _serviceRequestRepository = serviceRequestRepository;
            _contractRepository = contractRepository;
            _currencyExchangeService = currencyExchangeService;
            _currentUserService = currentUserService;
        }

        //..............................................................................//

        //retrieves all service requests from the database
        public async Task<List<ServiceRequest>> GetAllAsync(int? contractId = null, int? statusId = null)
        {
            return await _serviceRequestRepository.GetFilteredServiceRequestsAsync(contractId, statusId);
        }

        //..............................................................................//

        //retrieves service request list response DTOs
        public async Task<IReadOnlyList<ServiceRequestListDto>> GetListAsync(int? contractId = null, int? statusId = null)
        {
            var requests = await GetAllAsync(contractId, statusId);
            return requests.Select(request => request.ToListDto()).ToList();
        }

        //..............................................................................//

        //retrieves a single service request by ID, returns null if invalid or not found
        public async Task<ServiceRequest?> GetByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _serviceRequestRepository.GetByIdAsync(id);
        }

        //..............................................................................//

        //retrieves a service request with all associated details
        public async Task<ServiceRequest?> GetDetailsAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _serviceRequestRepository.GetServiceRequestWithDetailsAsync(id);
        }

        //..............................................................................//

        //retrieves a service request detail response DTO
        public async Task<ServiceRequestDetailDto?> GetDetailDtoAsync(int id)
        {
            var request = await GetDetailsAsync(id);
            return request?.ToDetailDto();
        }

        //..............................................................................//

        //retrieves all service requests associated with a contract
        public async Task<List<ServiceRequest>> GetByContractIdAsync(int contractId)
        {
            if (contractId <= 0)
                return new List<ServiceRequest>();

            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null)
                return new List<ServiceRequest>();

            return await _serviceRequestRepository.GetServiceRequestsByContractAsync(contractId);
        }

        //..............................................................................//

        //creates a new service request and ensures contract is active
        public async Task<ServiceRequest> CreateAsync(ServiceRequest serviceRequest)
        {
            if (serviceRequest == null)
                throw new ArgumentNullException(nameof(serviceRequest));

            if (serviceRequest.ContractId <= 0)
                throw new ArgumentException("Valid contract ID is required.", nameof(serviceRequest.ContractId));

            if (string.IsNullOrWhiteSpace(serviceRequest.Description))
                throw new ArgumentException("Description is required.", nameof(serviceRequest.Description));

            if (serviceRequest.AmountOriginal <= 0)
                throw new ArgumentException("Original amount must be greater than zero.", nameof(serviceRequest.AmountOriginal));

            if (string.IsNullOrWhiteSpace(serviceRequest.OriginalCurrencyCode))
                throw new ArgumentException("Original currency code is required.", nameof(serviceRequest.OriginalCurrencyCode));

            if (string.IsNullOrWhiteSpace(serviceRequest.RequestedByUserId))
                throw new ArgumentException("Requested by user ID is required.", nameof(serviceRequest.RequestedByUserId));

            var contract = await _contractRepository.GetContractWithDetailsAsync(serviceRequest.ContractId);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with ID {serviceRequest.ContractId} not found.");

            //business rule: check contract status before allowing service request creation
            string statusName = contract.ContractStatus?.StatusName?.Trim() ?? string.Empty;

            if (string.Equals(statusName, ContractStatusConstants.OnHoldName, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Cannot create a service request for a contract that is on hold.");

            if (string.Equals(statusName, ContractStatusConstants.ExpiredName, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Cannot create a service request for an expired contract.");

            if (!string.Equals(statusName, ContractStatusConstants.ActiveName, StringComparison.OrdinalIgnoreCase))
            {
                var displayStatus = string.IsNullOrWhiteSpace(statusName) ? "Unknown" : statusName;
                throw new InvalidOperationException($"Service requests can only be created for active contracts. This contract status is: {displayStatus}");
            }

            var currencyCode = serviceRequest.OriginalCurrencyCode.Trim().ToUpperInvariant();

            try
            {
                var rate = await _currencyExchangeService.GetRateToZarAsync(currencyCode);
                serviceRequest.OriginalCurrencyCode = currencyCode;
                serviceRequest.ExchangeRateToZAR = rate;
                serviceRequest.AmountZAR = decimal.Round(serviceRequest.AmountOriginal * rate, 2, MidpointRounding.AwayFromZero);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is HttpRequestException || ex is TaskCanceledException)
            {
                throw new InvalidOperationException($"Unable to convert {currencyCode} to ZAR right now. Please try again.", ex);
            }

            serviceRequest.RequestedAt = DateTime.UtcNow;
            serviceRequest.UpdatedAt = DateTime.UtcNow;

            await _serviceRequestRepository.AddAsync(serviceRequest);
            await _serviceRequestRepository.SaveChangesAsync();

            return serviceRequest;
        }

        //..............................................................................//

        //creates a new service request from a request DTO
        public async Task<ServiceRequestDetailDto> CreateAsync(CreateServiceRequestDto dto)
        {
            var created = await CreateAsync(dto.ToEntity(GetCurrentUserId()));
            var detail = await GetDetailsAsync(created.ServiceRequestId);
            return (detail ?? created).ToDetailDto();
        }

        //..............................................................................//

        //updates an existing service request
        public async Task UpdateAsync(ServiceRequest serviceRequest)
        {
            if (serviceRequest == null)
                throw new ArgumentNullException(nameof(serviceRequest));

            if (serviceRequest.ServiceRequestId <= 0)
                throw new ArgumentException("Invalid service request ID.", nameof(serviceRequest.ServiceRequestId));

            var existing = await _serviceRequestRepository.GetByIdAsync(serviceRequest.ServiceRequestId);
            if (existing == null)
                throw new KeyNotFoundException($"Service request with ID {serviceRequest.ServiceRequestId} not found.");

            if (serviceRequest.AmountOriginal <= 0)
                throw new ArgumentException("Original amount must be greater than zero.", nameof(serviceRequest.AmountOriginal));

            if (string.IsNullOrWhiteSpace(serviceRequest.OriginalCurrencyCode))
                throw new ArgumentException("Original currency code is required.", nameof(serviceRequest.OriginalCurrencyCode));

            var currencyCode = serviceRequest.OriginalCurrencyCode.Trim().ToUpperInvariant();

            try
            {
                var rate = await _currencyExchangeService.GetRateToZarAsync(currencyCode);
                serviceRequest.OriginalCurrencyCode = currencyCode;
                serviceRequest.ExchangeRateToZAR = rate;
                serviceRequest.AmountZAR = decimal.Round(serviceRequest.AmountOriginal * rate, 2, MidpointRounding.AwayFromZero);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is HttpRequestException || ex is TaskCanceledException)
            {
                throw new InvalidOperationException($"Unable to convert {currencyCode} to ZAR right now. Please try again.", ex);
            }

            serviceRequest.UpdatedAt = DateTime.UtcNow;

            _serviceRequestRepository.Update(serviceRequest);
            await _serviceRequestRepository.SaveChangesAsync();
        }

        //..............................................................................//

        //updates an existing service request from a request DTO
        public async Task<ServiceRequestDetailDto> UpdateAsync(int id, UpdateServiceRequestDto dto)
        {
            if (id != dto.ServiceRequestId)
                throw new ArgumentException("Service request ID does not match.");

            var serviceRequest = await GetByIdAsync(id);
            if (serviceRequest == null)
                throw new KeyNotFoundException($"Service request with ID {id} not found.");

            dto.ApplyTo(serviceRequest);
            await UpdateAsync(serviceRequest);

            var detail = await GetDetailsAsync(id);
            if (detail == null)
                throw new KeyNotFoundException($"Service request with ID {id} not found.");

            return detail.ToDetailDto();
        }

        //..............................................................................//

        //updates only the service request status
        public async Task UpdateStatusAsync(int id, int statusId)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid service request ID.", nameof(id));

            if (statusId <= 0)
                throw new ArgumentException("Valid service request status ID is required.", nameof(statusId));

            var serviceRequest = await _serviceRequestRepository.GetByIdAsync(id);
            if (serviceRequest == null)
                throw new KeyNotFoundException($"Service request with ID {id} not found.");

            serviceRequest.ServiceRequestStatusId = statusId;
            serviceRequest.UpdatedAt = DateTime.UtcNow;

            _serviceRequestRepository.Update(serviceRequest);
            await _serviceRequestRepository.SaveChangesAsync();
        }

        //..............................................................................//

        //updates only the service request status from a request DTO
        public async Task<ServiceRequestDetailDto> UpdateStatusAsync(int id, UpdateServiceRequestStatusDto dto)
        {
            await UpdateStatusAsync(id, dto.ServiceRequestStatusId);

            var detail = await GetDetailsAsync(id);
            if (detail == null)
                throw new KeyNotFoundException($"Service request with ID {id} not found.");

            return detail.ToDetailDto();
        }

        //..............................................................................//
        
        //removes a service request record from the database
        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid service request ID.", nameof(id));

            var serviceRequest = await _serviceRequestRepository.GetByIdAsync(id);
            if (serviceRequest == null)
                throw new KeyNotFoundException($"Service request with ID {id} not found.");

            _serviceRequestRepository.Delete(serviceRequest);
            await _serviceRequestRepository.SaveChangesAsync();
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


