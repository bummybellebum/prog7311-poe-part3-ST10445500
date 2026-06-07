using System.ComponentModel.DataAnnotations;

namespace GLMS.Api.DTOs.Documents
{
    public class UpdateContractDocumentDto
    {
        [Required]
        public int ContractDocumentId { get; set; }

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

        public DateTime UploadedAt { get; set; }
        public bool IsCurrent { get; set; }
    }
}
