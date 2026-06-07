using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Api.Models
{
    public class Contract
    {
        [Key]
        public int ContractId { get; set; }

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
        public string ServiceLevel { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //foreign key relationships
        [ForeignKey("ClientId")]
        public Client Client { get; set; } = null!;

        [ForeignKey("ContractStatusId")]
        public ContractStatus ContractStatus { get; set; } = null!;

        [ForeignKey("CreatedByUserId")]
        public ApplicationUser CreatedByUser { get; set; } = null!;

        //navigation properties
        public ICollection<ContractDocument> Documents { get; set; } = new List<ContractDocument>();
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}
