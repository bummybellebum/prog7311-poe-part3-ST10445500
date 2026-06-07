using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

//ST10445500 - PROG7311 - GLMS POE
//ContractDocument

//.....................................o0oSTART OF FILEo0o........................................//
namespace GLMS.Models
{
    public class ContractDocument
    {
        [Key]
        public int ContractDocumentId { get; set; }

        [Required]
        public int ContractId { get; set; }

        [Required]
        [StringLength(50)]
        public string DocumentType { get; set; }

        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; }

        [Required]
        [StringLength(255)]
        public string StoredFileName { get; set; }

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; }

        [StringLength(100)]
        public string ContentType { get; set; }

        public long FileSizeBytes { get; set; }

        [Required]
        public string UploadedByUserId { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public bool IsCurrent { get; set; } = true;

        //foreign key relationships
        [ForeignKey("ContractId")]
        public Contract Contract { get; set; }

        [ForeignKey("UploadedByUserId")]
        public ApplicationUser UploadedByUser { get; set; }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
