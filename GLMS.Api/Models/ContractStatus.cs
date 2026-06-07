using System.ComponentModel.DataAnnotations;

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
