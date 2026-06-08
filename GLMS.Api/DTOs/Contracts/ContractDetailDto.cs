using GLMS.Api.DTOs.Documents;
using GLMS.Api.DTOs.ServiceRequests;

//ST10445500 - PROG7311 - GLMS POE
//ContractDetailDto

//.....................................o0oSTART OF FILEo0o........................................//

// The DTO keeps API input and output simple instead of exposing full EF models.

namespace GLMS.Api.DTOs.Contracts
{
    public class ContractDetailDto
    {
        public int ContractId { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? ClientEmail { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ContractStatusId { get; set; }
        public string ContractStatusName { get; set; } = string.Empty;
        public string? ServiceLevel { get; set; }
        public string? Notes { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ContractDocumentDto> Documents { get; set; } = new();
        public List<ServiceRequestListDto> ServiceRequests { get; set; } = new();
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
