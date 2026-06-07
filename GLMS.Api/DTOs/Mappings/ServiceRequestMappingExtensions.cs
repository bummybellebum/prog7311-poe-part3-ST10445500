using GLMS.Api.DTOs.ServiceRequests;
using GLMS.Api.Models;

namespace GLMS.Api.DTOs.Mappings
{
    public static class ServiceRequestMappingExtensions
    {
        public static ServiceRequestListDto ToListDto(this ServiceRequest serviceRequest)
        {
            return serviceRequest.ToListDto(serviceRequest.Contract);
        }

        public static ServiceRequestListDto ToListDto(this ServiceRequest serviceRequest, Contract? contract)
        {
            return new ServiceRequestListDto
            {
                ServiceRequestId = serviceRequest.ServiceRequestId,
                ContractId = serviceRequest.ContractId,
                ContractTitle = contract?.Title ?? string.Empty,
                ClientId = contract?.ClientId,
                ClientName = contract?.Client?.CompanyName,
                RequestedByUserId = serviceRequest.RequestedByUserId,
                Description = serviceRequest.Description,
                AmountOriginal = serviceRequest.AmountOriginal,
                OriginalCurrencyCode = serviceRequest.OriginalCurrencyCode,
                ExchangeRateToZAR = serviceRequest.ExchangeRateToZAR,
                AmountZAR = serviceRequest.AmountZAR,
                ServiceRequestStatusId = serviceRequest.ServiceRequestStatusId,
                ServiceRequestStatusName = serviceRequest.ServiceRequestStatus?.StatusName ?? string.Empty,
                RequestedAt = serviceRequest.RequestedAt,
                UpdatedAt = serviceRequest.UpdatedAt
            };
        }

        public static ServiceRequestDetailDto ToDetailDto(this ServiceRequest serviceRequest)
        {
            return new ServiceRequestDetailDto
            {
                ServiceRequestId = serviceRequest.ServiceRequestId,
                ContractId = serviceRequest.ContractId,
                ContractTitle = serviceRequest.Contract?.Title ?? string.Empty,
                ClientId = serviceRequest.Contract?.ClientId,
                ClientName = serviceRequest.Contract?.Client?.CompanyName,
                RequestedByUserId = serviceRequest.RequestedByUserId,
                RequestedByEmail = serviceRequest.RequestedByUser?.Email,
                Description = serviceRequest.Description,
                AmountOriginal = serviceRequest.AmountOriginal,
                OriginalCurrencyCode = serviceRequest.OriginalCurrencyCode,
                ExchangeRateToZAR = serviceRequest.ExchangeRateToZAR,
                AmountZAR = serviceRequest.AmountZAR,
                ServiceRequestStatusId = serviceRequest.ServiceRequestStatusId,
                ServiceRequestStatusName = serviceRequest.ServiceRequestStatus?.StatusName ?? string.Empty,
                RequestedAt = serviceRequest.RequestedAt,
                UpdatedAt = serviceRequest.UpdatedAt
            };
        }

        public static ServiceRequest ToEntity(this CreateServiceRequestDto dto, string requestedByUserId)
        {
            return new ServiceRequest
            {
                ContractId = dto.ContractId,
                RequestedByUserId = requestedByUserId,
                Description = dto.Description,
                AmountOriginal = dto.AmountOriginal,
                OriginalCurrencyCode = dto.OriginalCurrencyCode,
                ServiceRequestStatusId = dto.ServiceRequestStatusId
            };
        }

        public static ServiceRequest ApplyTo(this UpdateServiceRequestDto dto, ServiceRequest serviceRequest)
        {
            serviceRequest.ServiceRequestId = dto.ServiceRequestId;
            serviceRequest.ContractId = dto.ContractId;
            serviceRequest.Description = dto.Description;
            serviceRequest.AmountOriginal = dto.AmountOriginal;
            serviceRequest.OriginalCurrencyCode = dto.OriginalCurrencyCode;
            serviceRequest.ServiceRequestStatusId = dto.ServiceRequestStatusId;

            if (!string.IsNullOrWhiteSpace(dto.RequestedByUserId))
            {
                serviceRequest.RequestedByUserId = dto.RequestedByUserId;
            }

            if (dto.RequestedAt != default)
            {
                serviceRequest.RequestedAt = dto.RequestedAt;
            }

            return serviceRequest;
        }
    }
}
