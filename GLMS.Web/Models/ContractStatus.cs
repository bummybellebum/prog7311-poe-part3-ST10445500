using System.ComponentModel.DataAnnotations;

//ST10445500 - PROG7311 - GLMS POE
//ContractStatus

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Models
{
    public class ContractStatus
    {
        [Key]
        public int ContractStatusId { get; set; }

        [Required]
        [StringLength(50)]
        public string StatusName { get; set; }

        //navigation properties
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
