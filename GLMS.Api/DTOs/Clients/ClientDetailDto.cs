using GLMS.Api.DTOs.Contracts;

namespace GLMS.Api.DTOs.Clients
{
    public class ClientDetailDto
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
        public List<ContractListDto> Contracts { get; set; } = new();
    }
}
