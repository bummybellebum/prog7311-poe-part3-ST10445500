//ST10445500 - PROG7311 - GLMS POE
//ClientListDto

//.....................................o0oSTART OF FILEo0o........................................//

// The DTO keeps API input and output simple instead of exposing full EF models.



namespace GLMS.Api.DTOs.Clients
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
}

//.....................................o0oEND OF FILEo0o..........................................//
