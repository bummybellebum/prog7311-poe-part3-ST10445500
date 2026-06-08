//ST10445500 - PROG7311 - GLMS POE
//ContractDocumentDto

//.....................................o0oSTART OF FILEo0o........................................//

// The DTO keeps API input and output simple instead of exposing full EF models.



namespace GLMS.Api.DTOs.Documents
{
    public class ContractDocumentDto
    {
        public int ContractDocumentId { get; set; }
        public int ContractId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public long FileSizeBytes { get; set; }
        public string UploadedByUserId { get; set; } = string.Empty;
        public string? UploadedByEmail { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool IsCurrent { get; set; }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
