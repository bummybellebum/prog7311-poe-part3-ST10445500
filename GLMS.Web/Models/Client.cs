using System.ComponentModel.DataAnnotations;

//ST10445500 - PROG7311 - GLMS POE
//Client

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required]
        [StringLength(255)]
        public string CompanyName { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(100)]
        public string Region { get; set; }

        [StringLength(100)]
        public string Country { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //navigation properties
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
