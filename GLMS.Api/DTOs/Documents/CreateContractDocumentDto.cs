using System.ComponentModel.DataAnnotations;

//ST10445500 - PROG7311 - GLMS POE
//CreateContractDocumentDto

//.....................................o0oSTART OF FILEo0o........................................//

// The DTO keeps API input and output simple instead of exposing full EF models.

namespace GLMS.Api.DTOs.Documents
{
    public class CreateContractDocumentDto
    {
        [Required]
        public int ContractId { get; set; }

        [Required]
        [StringLength(50)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string StoredFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContentType { get; set; }

        public long FileSizeBytes { get; set; }

        [Required]
        public string UploadedByUserId { get; set; } = string.Empty;

        public bool IsCurrent { get; set; } = true;
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
