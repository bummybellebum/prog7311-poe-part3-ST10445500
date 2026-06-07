using System.ComponentModel.DataAnnotations;

namespace GLMS.Api.DTOs.ServiceRequests
{
    public class CreateServiceRequestDto
    {
        [Required]
        public int ContractId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public decimal AmountOriginal { get; set; }

        [Required]
        [StringLength(3)]
        public string OriginalCurrencyCode { get; set; } = string.Empty;

        [Required]
        public int ServiceRequestStatusId { get; set; }
    }
}
