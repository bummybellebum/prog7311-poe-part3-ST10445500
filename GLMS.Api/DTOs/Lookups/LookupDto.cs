//ST10445500 - PROG7311 - GLMS POE
//LookupDto

//.....................................o0oSTART OF FILEo0o........................................//

// The DTO keeps API input and output simple instead of exposing full EF models.



namespace GLMS.Api.DTOs.Lookups
{
    public class LookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
