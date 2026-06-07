using GLMS.Api.Data.Repositories;
using GLMS.Api.DTOs.Mappings;
using GLMS.Api.DTOs.ServiceRequests;
using GLMS.Api.Models;



namespace GLMS.Api.Services
{
    //manages business logic for Service Requests
    public interface IServiceRequestService
    {
        //retrieves service request list response DTOs
        Task<IReadOnlyList<ServiceRequestListDto>> GetServiceRequestsAsync(int? contractId = null, int? statusId = null);

        //retrieves a service request detail response DTO
        Task<ServiceRequestDetailDto?> GetServiceRequestAsync(int id);

        //creates a new service request from a request DTO
        Task<ServiceRequestDetailDto> CreateServiceRequestAsync(CreateServiceRequestDto dto);

        //updates an existing service request from a request DTO
        Task<ServiceRequestDetailDto> UpdateServiceRequestAsync(int id, UpdateServiceRequestDto dto);

        //updates only the service request status from a request DTO
        Task<ServiceRequestDetailDto> UpdateServiceRequestStatusAsync(int id, UpdateServiceRequestStatusDto dto);

        //removes a service request record from the database
        Task DeleteServiceRequestAsync(int id);
    }

    //..............................................................................//

    //implements business logic for managing Service Requests
    //validates data and coordinates with the repository layer
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IServiceRequestRepository _serviceRequestRepository;
        private readonly IContractRepository _contractRepository;
        private readonly ICurrencyExchangeService _currencyExchangeService;
        private readonly IAccountService _accountService;

        public ServiceRequestService(
            IServiceRequestRepository serviceRequestRepository,
            IContractRepository contractRepository,
            ICurrencyExchangeService currencyExchangeService,
            IAccountService accountService)
        {
            _serviceRequestRepository = serviceRequestRepository;
            _contractRepository = contractRepository;
            _currencyExchangeService = currencyExchangeService;
            _accountService = accountService;
        }

        //..............................................................................//

        //retrieves service request list response DTOs
        public async Task<IReadOnlyList<ServiceRequestListDto>> GetServiceRequestsAsync(int? contractId = null, int? statusId = null)
        {
            var requests = await _serviceRequestRepository.GetFilteredServiceRequestsAsync(contractId, statusId);
            return requests.Select(request => request.ToListDto()).ToList();
        }

        //..............................................................................//

        //retrieves a service request detail response DTO
        public async Task<ServiceRequestDetailDto?> GetServiceRequestAsync(int id)
        {
            if (id <= 0)
                return null;

            var request = await _serviceRequestRepository.GetServiceRequestWithDetailsAsync(id);
            return request?.ToDetailDto();
        }

        //..............................................................................//

        //creates a new service request from a request DTO
        public async Task<ServiceRequestDetailDto> CreateServiceRequestAsync(CreateServiceRequestDto dto)
        {
            var created = await CreateServiceRequestRecordAsync(dto.ToEntity(GetCurrentUserId()));
            var detail = await _serviceRequestRepository.GetServiceRequestWithDetailsAsync(created.ServiceRequestId);
            return (detail ?? created).ToDetailDto();
        }

        //..............................................................................//

        //updates an existing service request from a request DTO
        public async Task<ServiceRequestDetailDto> UpdateServiceRequestAsync(int id, UpdateServiceRequestDto dto)
        {
            if (id != dto.ServiceRequestId)
                throw new ArgumentException("Service request ID does not match.");

            var serviceRequest = await _serviceRequestRepository.GetByIdAsync(id);
            if (serviceRequest == null)
                throw new KeyNotFoundException($"Service request with ID {id} not found.");

            dto.ApplyTo(serviceRequest);
            await UpdateServiceRequestRecordAsync(serviceRequest);

            var detail = await _serviceRequestRepository.GetServiceRequestWithDetailsAsync(id);
            if (detail == null)
                throw new KeyNotFoundException($"Service request with ID {id} not found.");

            return detail.ToDetailDto();
        }

        //..............................................................................//

        //updates only the service request status from a request DTO
        public async Task<ServiceRequestDetailDto> UpdateServiceRequestStatusAsync(int id, UpdateServiceRequestStatusDto dto)
        {
            await UpdateServiceRequestStatusRecordAsync(id, dto.ServiceRequestStatusId);

            var detail = await _serviceRequestRepository.GetServiceRequestWithDetailsAsync(id);
            if (detail == null)
                throw new KeyNotFoundException($"Service request with ID {id} not found.");

            return detail.ToDetailDto();
        }

        //..............................................................................//

        //removes a service request record from the database
        public async Task DeleteServiceRequestAsync(int id)
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

        //creates a new service request and ensures contract is active
        private async Task<ServiceRequest> CreateServiceRequestRecordAsync(ServiceRequest serviceRequest)
        {
            ValidateServiceRequest(serviceRequest);

            if (string.IsNullOrWhiteSpace(serviceRequest.RequestedByUserId))
                throw new ArgumentException("Requested by user ID is required.", nameof(serviceRequest.RequestedByUserId));

            var contract = await _contractRepository.GetContractWithDetailsAsync(serviceRequest.ContractId);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with ID {serviceRequest.ContractId} not found.");

            EnsureContractCanReceiveServiceRequests(contract);
            await ApplyCurrencyConversionAsync(serviceRequest);

            serviceRequest.RequestedAt = DateTime.UtcNow;
            serviceRequest.UpdatedAt = DateTime.UtcNow;

            await _serviceRequestRepository.AddAsync(serviceRequest);
            await _serviceRequestRepository.SaveChangesAsync();

            return serviceRequest;
        }

        //..............................................................................//

        //updates an existing service request
        private async Task UpdateServiceRequestRecordAsync(ServiceRequest serviceRequest)
        {
            ValidateServiceRequest(serviceRequest);

            if (serviceRequest.ServiceRequestId <= 0)
                throw new ArgumentException("Invalid service request ID.", nameof(serviceRequest.ServiceRequestId));

            await ApplyCurrencyConversionAsync(serviceRequest);

            serviceRequest.UpdatedAt = DateTime.UtcNow;

            _serviceRequestRepository.Update(serviceRequest);
            await _serviceRequestRepository.SaveChangesAsync();
        }

        //..............................................................................//

        //updates only the service request status
        private async Task UpdateServiceRequestStatusRecordAsync(int id, int statusId)
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

        //checks the required service request fields
        private static void ValidateServiceRequest(ServiceRequest serviceRequest)
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
        }

        //..............................................................................//

        //business rule: check contract status before allowing service request creation
        private static void EnsureContractCanReceiveServiceRequests(Contract contract)
        {
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
        }

        //..............................................................................//

        //converts the original amount to ZAR using the exchange service
        private async Task ApplyCurrencyConversionAsync(ServiceRequest serviceRequest)
        {
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
        }

        //..............................................................................//

        private string GetCurrentUserId()
        {
            return _accountService.GetCurrentUserId();
        }

        //..............................................................................//
    }
}
