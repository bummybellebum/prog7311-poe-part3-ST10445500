//ST10445500 - PROG7311 - GLMS POE
//ClientDtos

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web.ApiClients.Models
{
    public class ClientListDto
    {
        public int ClientId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ClientDetailDto : ClientListDto
    {
        public List<ContractListDto> Contracts { get; set; } = new();
    }

    public class CreateClientDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateClientDto : CreateClientDto
    {
        public int ClientId { get; set; }
    }
}

//..........................................o0oEND OF FILEo0o..................................................//
