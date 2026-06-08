using GLMS.Web.ApiClients.Models;
using GLMS.Web.ViewModels.Contracts;

//ST10445500 - PROG7311 - GLMS POE
//ContractMappingExtensions

//.....................................o0oSTART OF FILEo0o........................................//

// Mapping prepares API data for the MVC view models used by Razor screens.

//ST10445500 - PROG7311 - GLMS POE
//ContractMappingExtensions

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web.ViewModels.Mappings
{
    public static class ContractMappingExtensions
    {
        public static ContractListItemViewModel ToListViewModel(this ContractListDto contract)
        {
            return new ContractListItemViewModel
            {
                ContractId = contract.ContractId,
                Title = contract.Title,
                ClientName = contract.ClientName,
                ContractStatusName = contract.ContractStatusName,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                CreatedAt = contract.CreatedAt
            };
        }

        public static ContractDetailViewModel ToDetailViewModel(this ContractDetailDto contract)
        {
            return new ContractDetailViewModel
            {
                ContractId = contract.ContractId,
                ClientId = contract.ClientId,
                Title = contract.Title,
                ClientName = contract.ClientName,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                ContractStatusId = contract.ContractStatusId,
                ContractStatusName = contract.ContractStatusName,
                ServiceLevel = contract.ServiceLevel,
                CreatedAt = contract.CreatedAt,
                Notes = contract.Notes
            };
        }

        public static ContractDocumentItemViewModel ToItemViewModel(this ContractDocumentDto document)
        {
            return new ContractDocumentItemViewModel
            {
                ContractDocumentId = document.ContractDocumentId,
                OriginalFileName = document.OriginalFileName,
                UploadedAt = document.UploadedAt
            };
        }

        public static ContractDeleteViewModel ToDeleteViewModel(this ContractDetailDto contract)
        {
            return new ContractDeleteViewModel
            {
                ContractId = contract.ContractId,
                Title = contract.Title,
                ClientName = contract.ClientName,
                ContractStatusName = contract.ContractStatusName
            };
        }
    }
}

//..........................................o0oEND OF FILEo0o..................................................//

//.....................................o0oEND OF FILEo0o..........................................//
