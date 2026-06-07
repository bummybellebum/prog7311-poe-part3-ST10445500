using Microsoft.AspNetCore.Identity;

//ST10445500 - PROG7311 - GLMS POE
//ApplicationUser

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //navigation properties
        public ICollection<Contract> CreatedContracts { get; set; } = new List<Contract>();
        public ICollection<ContractDocument> UploadedDocuments { get; set; } = new List<ContractDocument>();
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}

//.....................................o0oEND OF FILEo0o........................................//

