using GLMS.Api.DTOs.Contracts;
using GLMS.Api.Models;

//ST10445500 - PROG7311 - GLMS POE
//ContractMappingExtensions

//.....................................o0oSTART OF FILEo0o........................................//

// Mapping keeps the API DTO shapes separate from the EF database models.

namespace GLMS.Api.DTOs.Mappings
{
    public static class ContractMappingExtensions
    {
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
                Documents = contract.Documents.Select(document => document.ToDto()).ToList(),
                ServiceRequests = contract.ServiceRequests.Select(request => request.ToListDto(contract)).ToList()
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
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
