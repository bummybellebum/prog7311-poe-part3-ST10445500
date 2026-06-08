using GLMS.Api.DTOs.Clients;
using GLMS.Api.Models;

//ST10445500 - PROG7311 - GLMS POE
//ClientMappingExtensions

//.....................................o0oSTART OF FILEo0o........................................//

// Mapping keeps the API DTO shapes separate from the EF database models.

namespace GLMS.Api.DTOs.Mappings
{
    public static class ClientMappingExtensions
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
                Contracts = client.Contracts.Select(contract => contract.ToListDto()).ToList()
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
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
