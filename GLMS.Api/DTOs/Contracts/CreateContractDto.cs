using System.ComponentModel.DataAnnotations;

namespace GLMS.Api.DTOs.Contracts
{
    public class CreateContractDto
    {
        [Required]
        public int ClientId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Required]
        public int ContractStatusId { get; set; }

        [StringLength(100)]
        public string? ServiceLevel { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}
