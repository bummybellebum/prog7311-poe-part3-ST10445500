using GLMS.Web.ApiClients.Models;
using GLMS.Web.ViewModels.ServiceRequests;

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestMappingExtensions

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web.ViewModels.Mappings
{
    public static class ServiceRequestMappingExtensions
    {
        public static ServiceRequestListItemViewModel ToListViewModel(this ServiceRequestListDto request)
        {
            return new ServiceRequestListItemViewModel
            {
                ServiceRequestId = request.ServiceRequestId,
                ContractId = request.ContractId,
                ContractTitle = request.ContractTitle,
                ServiceRequestStatusName = request.ServiceRequestStatusName,
                AmountOriginal = request.AmountOriginal,
                OriginalCurrencyCode = request.OriginalCurrencyCode,
                AmountZAR = request.AmountZAR,
                RequestedAt = request.RequestedAt
            };
        }

        public static ServiceRequestDetailsViewModel ToDetailsViewModel(this ServiceRequestDetailDto request)
        {
            return new ServiceRequestDetailsViewModel
            {
                ServiceRequestId = request.ServiceRequestId,
                ContractId = request.ContractId,
                ContractTitle = request.ContractTitle,
                ServiceRequestStatusName = request.ServiceRequestStatusName,
                AmountOriginal = request.AmountOriginal,
                OriginalCurrencyCode = request.OriginalCurrencyCode,
                AmountZAR = request.AmountZAR,
                RequestedAt = request.RequestedAt,
                ExchangeRateToZAR = request.ExchangeRateToZAR,
                RequestedByUserId = request.RequestedByUserId,
                RequestedByEmail = request.RequestedByEmail,
                Description = request.Description
            };
        }

        public static ServiceRequestDeleteViewModel ToDeleteViewModel(this ServiceRequestDetailDto request)
        {
            return new ServiceRequestDeleteViewModel
            {
                ServiceRequestId = request.ServiceRequestId,
                ContractTitle = request.ContractTitle,
                ServiceRequestStatusName = request.ServiceRequestStatusName,
                AmountZAR = request.AmountZAR
            };
        }
    }
}

//..........................................o0oEND OF FILEo0o..................................................//
