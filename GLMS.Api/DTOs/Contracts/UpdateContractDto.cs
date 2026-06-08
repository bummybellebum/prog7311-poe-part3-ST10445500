using System.ComponentModel.DataAnnotations;

//ST10445500 - PROG7311 - GLMS POE
//UpdateContractDto

//.....................................o0oSTART OF FILEo0o........................................//

// The DTO keeps API input and output simple instead of exposing full EF models.

namespace GLMS.Api.DTOs.Contracts
{
    public class UpdateContractDto
    {
        [Required]
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
        public string? ServiceLevel { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public string CreatedByUserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
