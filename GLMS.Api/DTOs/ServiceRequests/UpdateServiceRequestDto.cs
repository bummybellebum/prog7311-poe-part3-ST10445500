using System.ComponentModel.DataAnnotations;

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
