//ST10445500 - PROG7311 - GLMS POE
//AuthResponseDto

//.....................................o0oSTART OF FILEo0o........................................//

// The DTO keeps API input and output simple instead of exposing full EF models.



namespace GLMS.Api.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
