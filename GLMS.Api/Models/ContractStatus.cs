using System.ComponentModel.DataAnnotations;

//ST10445500 - PROG7311 - GLMS POE
//ContractStatus

//.....................................o0oSTART OF FILEo0o........................................//

// This model represents data that the API stores and works with in the database.

namespace GLMS.Api.Models
{
    public class ContractStatus
    {
        [Key]
        public int ContractStatusId { get; set; }

        [Required]
        [StringLength(50)]
        public string StatusName { get; set; } = string.Empty;

        //navigation properties
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
