using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

//ST10445500 - PROG7311 - GLMS POE
//Contract

//.....................................o0oSTART OF FILEo0o........................................//

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
        public string Title { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Required]
        public int ContractStatusId { get; set; }

        [StringLength(100)]
        public string ServiceLevel { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        [Required]
        public string CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //foreign key relationships
        [ForeignKey("ClientId")]
        public Client Client { get; set; }

        [ForeignKey("ContractStatusId")]
        public ContractStatus ContractStatus { get; set; }

        [ForeignKey("CreatedByUserId")]
        public ApplicationUser CreatedByUser { get; set; }

        //navigation properties
        public ICollection<ContractDocument> Documents { get; set; } = new List<ContractDocument>();
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}

//.....................................o0oEND OF FILEo0o..........................................//

