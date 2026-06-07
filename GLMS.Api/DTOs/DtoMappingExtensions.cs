using GLMS.Api.DTOs.Clients;
using GLMS.Api.DTOs.Contracts;
using GLMS.Api.DTOs.Documents;
using GLMS.Api.DTOs.Lookups;
using GLMS.Api.DTOs.ServiceRequests;
using GLMS.Api.Models;

namespace GLMS.Api.DTOs
{
    public static class DtoMappingExtensions
    {
        public static ClientListDto ToListDto(this Client client)
        {
            return new ClientListDto
            {
                ClientId = client.ClientId,
                CompanyName = client.CompanyName,
                Email = client.Email,
                Phone = client.Phone,
                Region = client.Region,
                Country = client.Country,
                IsActive = client.IsActive,
                CreatedAt = client.CreatedAt,
                UpdatedAt = client.UpdatedAt
            };
        }

        public static ClientDetailDto ToDetailDto(this Client client)
        {
            return new ClientDetailDto
            {
                ClientId = client.ClientId,
                CompanyName = client.CompanyName,
                Email = client.Email,
                Phone = client.Phone,
                Region = client.Region,
                Country = client.Country,
                IsActive = client.IsActive,
                CreatedAt = client.CreatedAt,
                UpdatedAt = client.UpdatedAt,
                Contracts = client.Contracts.Select(c => c.ToListDto()).ToList()
            };
        }

        public static Client ToEntity(this CreateClientDto dto)
        {
            return new Client
            {
                CompanyName = dto.CompanyName,
                Email = dto.Email,
                Phone = dto.Phone ?? string.Empty,
                Region = dto.Region ?? string.Empty,
                Country = dto.Country ?? string.Empty,
                IsActive = dto.IsActive
            };
        }

        public static Client ApplyTo(this UpdateClientDto dto, Client client)
        {
            client.ClientId = dto.ClientId;
            client.CompanyName = dto.CompanyName;
            client.Email = dto.Email;
            client.Phone = dto.Phone ?? string.Empty;
            client.Region = dto.Region ?? string.Empty;
            client.Country = dto.Country ?? string.Empty;
            client.IsActive = dto.IsActive;
            return client;
        }

        public static ContractListDto ToListDto(this Contract contract)
        {
            return new ContractListDto
            {
                ContractId = contract.ContractId,
                ClientId = contract.ClientId,
                ClientName = contract.Client?.CompanyName ?? string.Empty,
                Title = contract.Title,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                ContractStatusId = contract.ContractStatusId,
                ContractStatusName = contract.ContractStatus?.StatusName ?? string.Empty,
                ServiceLevel = contract.ServiceLevel,
                CreatedAt = contract.CreatedAt,
                UpdatedAt = contract.UpdatedAt
            };
        }

        public static ContractDetailDto ToDetailDto(this Contract contract)
        {
            return new ContractDetailDto
            {
                ContractId = contract.ContractId,
                ClientId = contract.ClientId,
                ClientName = contract.Client?.CompanyName ?? string.Empty,
                ClientEmail = contract.Client?.Email,
                Title = contract.Title,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                ContractStatusId = contract.ContractStatusId,
                ContractStatusName = contract.ContractStatus?.StatusName ?? string.Empty,
                ServiceLevel = contract.ServiceLevel,
                Notes = contract.Notes,
                CreatedByUserId = contract.CreatedByUserId,
                CreatedAt = contract.CreatedAt,
                UpdatedAt = contract.UpdatedAt,
                Documents = contract.Documents.Select(d => d.ToDto()).ToList(),
                ServiceRequests = contract.ServiceRequests.Select(sr => sr.ToListDto(contract)).ToList()
            };
        }

        public static Contract ToEntity(this CreateContractDto dto, string createdByUserId)
        {
            return new Contract
            {
                ClientId = dto.ClientId,
                Title = dto.Title,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ContractStatusId = dto.ContractStatusId,
                ServiceLevel = dto.ServiceLevel ?? string.Empty,
                Notes = dto.Notes ?? string.Empty,
                CreatedByUserId = createdByUserId
            };
        }

        public static Contract ApplyTo(this UpdateContractDto dto, Contract contract)
        {
            contract.ContractId = dto.ContractId;
            contract.ClientId = dto.ClientId;
            contract.Title = dto.Title;
            contract.StartDate = dto.StartDate;
            contract.EndDate = dto.EndDate;
            contract.ContractStatusId = dto.ContractStatusId;
            contract.ServiceLevel = dto.ServiceLevel ?? string.Empty;
            contract.Notes = dto.Notes ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(dto.CreatedByUserId))
            {
                contract.CreatedByUserId = dto.CreatedByUserId;
            }

            if (dto.CreatedAt != default)
            {
                contract.CreatedAt = dto.CreatedAt;
            }

            return contract;
        }

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

        public static ContractDocumentDto ToDto(this ContractDocument document)
        {
            return new ContractDocumentDto
            {
                ContractDocumentId = document.ContractDocumentId,
                ContractId = document.ContractId,
                DocumentType = document.DocumentType,
                OriginalFileName = document.OriginalFileName,
                StoredFileName = document.StoredFileName,
                FilePath = document.FilePath,
                ContentType = document.ContentType,
                FileSizeBytes = document.FileSizeBytes,
                UploadedByUserId = document.UploadedByUserId,
                UploadedByEmail = document.UploadedByUser?.Email,
                UploadedAt = document.UploadedAt,
                IsCurrent = document.IsCurrent
            };
        }

        public static ContractDocument ToEntity(this CreateContractDocumentDto dto)
        {
            return new ContractDocument
            {
                ContractId = dto.ContractId,
                DocumentType = dto.DocumentType,
                OriginalFileName = dto.OriginalFileName,
                StoredFileName = dto.StoredFileName,
                FilePath = dto.FilePath,
                ContentType = dto.ContentType ?? string.Empty,
                FileSizeBytes = dto.FileSizeBytes,
                UploadedByUserId = dto.UploadedByUserId,
                IsCurrent = dto.IsCurrent
            };
        }

        public static ContractDocument ApplyTo(this UpdateContractDocumentDto dto, ContractDocument document)
        {
            document.ContractDocumentId = dto.ContractDocumentId;
            document.ContractId = dto.ContractId;
            document.DocumentType = dto.DocumentType;
            document.OriginalFileName = dto.OriginalFileName;
            document.StoredFileName = dto.StoredFileName;
            document.FilePath = dto.FilePath;
            document.ContentType = dto.ContentType ?? string.Empty;
            document.FileSizeBytes = dto.FileSizeBytes;
            document.UploadedByUserId = dto.UploadedByUserId;
            document.IsCurrent = dto.IsCurrent;

            if (dto.UploadedAt != default)
            {
                document.UploadedAt = dto.UploadedAt;
            }

            return document;
        }

        public static LookupDto ToLookupDto(this ContractStatus status)
        {
            return new LookupDto
            {
                Id = status.ContractStatusId,
                Name = status.StatusName
            };
        }

        public static LookupDto ToLookupDto(this ServiceRequestStatus status)
        {
            return new LookupDto
            {
                Id = status.ServiceRequestStatusId,
                Name = status.StatusName
            };
        }
    }
}
