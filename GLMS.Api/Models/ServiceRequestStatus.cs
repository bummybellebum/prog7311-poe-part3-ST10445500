using System.ComponentModel.DataAnnotations;

namespace GLMS.Api.Models
{
    public class ServiceRequestStatus
    {
        [Key]
        public int ServiceRequestStatusId { get; set; }

        [Required]
        [StringLength(50)]
        public string StatusName { get; set; } = string.Empty;

        //navigation properties
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}
