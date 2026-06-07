//ST10445500 - PROG7311 - GLMS POE
//ContractDtos

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web.ViewModels.Api
{
    public class ContractListDto
    {
        public int ContractId { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ContractStatusId { get; set; }
        public string ContractStatusName { get; set; } = string.Empty;
        public string? ServiceLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ContractDetailDto : ContractListDto
    {
        public string? ClientEmail { get; set; }
        public string? Notes { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public List<ContractDocumentDto> Documents { get; set; } = new();
        public List<ServiceRequestListDto> ServiceRequests { get; set; } = new();
    }

    public class CreateContractDto
    {
        public int ClientId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ContractStatusId { get; set; }
        public string? ServiceLevel { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateContractDto : CreateContractDto
    {
        public int ContractId { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateContractStatusDto
    {
        public int ContractStatusId { get; set; }
    }
}

//..........................................o0oEND OF FILEo0o..................................................//
