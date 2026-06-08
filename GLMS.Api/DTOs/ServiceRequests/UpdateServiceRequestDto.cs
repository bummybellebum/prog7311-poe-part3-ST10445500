using System.ComponentModel.DataAnnotations;

//ST10445500 - PROG7311 - GLMS POE
//UpdateServiceRequestDto

//.....................................o0oSTART OF FILEo0o........................................//

// The DTO keeps API input and output simple instead of exposing full EF models.

namespace GLMS.Api.DTOs.ServiceRequests
{
    public class UpdateServiceRequestDto
    {
        [Required]
        public int ServiceRequestId { get; set; }

        [Required]
        public int ContractId { get; set; }

        public string RequestedByUserId { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public decimal AmountOriginal { get; set; }

        [Required]
        [StringLength(3)]
        public string OriginalCurrencyCode { get; set; } = string.Empty;

        [Required]
        public int ServiceRequestStatusId { get; set; }

        public DateTime RequestedAt { get; set; }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
