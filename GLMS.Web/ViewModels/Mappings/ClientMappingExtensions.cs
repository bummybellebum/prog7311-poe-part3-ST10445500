using GLMS.Web.ApiClients.Models;
using GLMS.Web.ViewModels.Clients;

//ST10445500 - PROG7311 - GLMS POE
//ClientMappingExtensions

//.....................................o0oSTART OF FILEo0o........................................//

// Mapping prepares API data for the MVC view models used by Razor screens.

//ST10445500 - PROG7311 - GLMS POE
//ClientMappingExtensions

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web.ViewModels.Mappings
{
    public static class ClientMappingExtensions
    {
        public static ClientListItemViewModel ToListViewModel(this ClientListDto client)
        {
            return new ClientListItemViewModel
            {
                ClientId = client.ClientId,
                CompanyName = client.CompanyName,
                Email = client.Email,
                Region = client.Region,
                Country = client.Country,
                IsActive = client.IsActive
            };
        }

        public static ClientDetailsViewModel ToDetailsViewModel(this ClientDetailDto client)
        {
            return new ClientDetailsViewModel
            {
                ClientId = client.ClientId,
                CompanyName = client.CompanyName,
                Email = client.Email,
                Phone = client.Phone,
                Region = client.Region,
                Country = client.Country,
                IsActive = client.IsActive,
                ContractCount = client.Contracts.Count,
                ActiveContractCount = client.Contracts.Count(c => string.Equals(c.ContractStatusName, "Active", StringComparison.OrdinalIgnoreCase)),
                Contracts = client.Contracts
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => c.ToListViewModel())
                    .ToList()
            };
        }

        public static ClientDeleteViewModel ToDeleteViewModel(this ClientDetailDto client)
        {
            return new ClientDeleteViewModel
            {
                ClientId = client.ClientId,
                CompanyName = client.CompanyName,
                Email = client.Email,
                IsActive = client.IsActive
            };
        }
    }
}

//..........................................o0oEND OF FILEo0o..................................................//

//.....................................o0oEND OF FILEo0o..........................................//
