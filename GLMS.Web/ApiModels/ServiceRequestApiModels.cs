//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestDtos

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web.ApiModels
{
    public class ServiceRequestListDto
    {
        public int ServiceRequestId { get; set; }
        public int ContractId { get; set; }
        public string ContractTitle { get; set; } = string.Empty;
        public int? ClientId { get; set; }
        public string? ClientName { get; set; }
        public string RequestedByUserId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal AmountOriginal { get; set; }
        public string OriginalCurrencyCode { get; set; } = string.Empty;
        public decimal ExchangeRateToZAR { get; set; }
        public decimal AmountZAR { get; set; }
        public int ServiceRequestStatusId { get; set; }
        public string ServiceRequestStatusName { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ServiceRequestDetailDto : ServiceRequestListDto
    {
        public string? RequestedByEmail { get; set; }
    }

    public class CreateServiceRequestDto
    {
        public int ContractId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal AmountOriginal { get; set; }
        public string OriginalCurrencyCode { get; set; } = string.Empty;
        public int ServiceRequestStatusId { get; set; }
    }

    public class UpdateServiceRequestDto : CreateServiceRequestDto
    {
        public int ServiceRequestId { get; set; }
        public string RequestedByUserId { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
    }

    public class UpdateServiceRequestStatusDto
    {
        public int ServiceRequestStatusId { get; set; }
    }
}

//..........................................o0oEND OF FILEo0o..................................................//
