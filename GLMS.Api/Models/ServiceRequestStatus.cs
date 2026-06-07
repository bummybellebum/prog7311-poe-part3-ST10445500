using System.ComponentModel.DataAnnotations;

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestStatus

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Models
{
    public class ServiceRequestStatus
    {
        [Key]
        public int ServiceRequestStatusId { get; set; }

        [Required]
        [StringLength(50)]
        public string StatusName { get; set; }

        //navigation properties
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}

//.....................................o0oEND OF FILEo0o..........................................//

