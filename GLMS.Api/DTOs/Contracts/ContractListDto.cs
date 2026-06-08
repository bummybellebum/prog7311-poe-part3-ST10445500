//ST10445500 - PROG7311 - GLMS POE
//ContractListDto

//.....................................o0oSTART OF FILEo0o........................................//

// The DTO keeps API input and output simple instead of exposing full EF models.



namespace GLMS.Api.DTOs.Contracts
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
}

//.....................................o0oEND OF FILEo0o..........................................//
