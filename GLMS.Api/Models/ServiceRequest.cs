using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequest

//.....................................o0oSTART OF FILEo0o........................................//

// This model represents data that the API stores and works with in the database.

namespace GLMS.Api.Models
{
    public class ServiceRequest
    {
        [Key]
        public int ServiceRequestId { get; set; }

        [Required]
        public int ContractId { get; set; }

        [Required]
        public string RequestedByUserId { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountOriginal { get; set; }

        [StringLength(3)]
        public string OriginalCurrencyCode { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 6)")]
        public decimal ExchangeRateToZAR { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountZAR { get; set; }

        [Required]
        public int ServiceRequestStatusId { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //foreign key relationships
        [ForeignKey("ContractId")]
        public Contract Contract { get; set; } = null!;

        [ForeignKey("RequestedByUserId")]
        public ApplicationUser RequestedByUser { get; set; } = null!;

        [ForeignKey("ServiceRequestStatusId")]
        public ServiceRequestStatus ServiceRequestStatus { get; set; } = null!;
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
