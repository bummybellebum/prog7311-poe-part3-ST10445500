using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

//ST10445500 - PROG7311 - GLMS POE
//ApplicationUser

//.....................................o0oSTART OF FILEo0o........................................//

// This model represents data that the API stores and works with in the database.

namespace GLMS.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //navigation properties
        public ICollection<Contract> CreatedContracts { get; set; } = new List<Contract>();
        public ICollection<ContractDocument> UploadedDocuments { get; set; } = new List<ContractDocument>();
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}

//.....................................o0oEND OF FILEo0o........................................//

//.....................................o0oEND OF FILEo0o..........................................//
