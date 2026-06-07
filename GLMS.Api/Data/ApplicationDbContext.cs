using GLMS.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

//ST10445500 - PROG7311 - GLMS POE
//ApplicationDbContext

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		//..............................................................................//

		public DbSet<Client> Clients { get; set; }
		public DbSet<ContractStatus> ContractStatuses { get; set; }
		public DbSet<Contract> Contracts { get; set; }
		public DbSet<ContractDocument> ContractDocuments { get; set; }
		public DbSet<ServiceRequestStatus> ServiceRequestStatuses { get; set; }
		public DbSet<ServiceRequest> ServiceRequests { get; set; }

		//..............................................................................//

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			//set up how the tables connect to each other
			builder.Entity<Contract>()
				.HasOne(c => c.Client)
				.WithMany(cl => cl.Contracts)
				.HasForeignKey(c => c.ClientId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Contract>()
				.HasOne(c => c.ContractStatus)
				.WithMany(cs => cs.Contracts)
				.HasForeignKey(c => c.ContractStatusId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Contract>()
				.HasOne(c => c.CreatedByUser)
				.WithMany(u => u.CreatedContracts)
				.HasForeignKey(c => c.CreatedByUserId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<ContractDocument>()
				.HasOne(cd => cd.Contract)
				.WithMany(c => c.Documents)
				.HasForeignKey(cd => cd.ContractId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<ContractDocument>()
				.HasOne(cd => cd.UploadedByUser)
				.WithMany(u => u.UploadedDocuments)
				.HasForeignKey(cd => cd.UploadedByUserId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<ServiceRequest>()
				.HasOne(sr => sr.Contract)
				.WithMany(c => c.ServiceRequests)
				.HasForeignKey(sr => sr.ContractId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<ServiceRequest>()
				.HasOne(sr => sr.RequestedByUser)
				.WithMany(u => u.ServiceRequests)
				.HasForeignKey(sr => sr.RequestedByUserId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<ServiceRequest>()
				.HasOne(sr => sr.ServiceRequestStatus)
				.WithMany(srs => srs.ServiceRequests)
				.HasForeignKey(sr => sr.ServiceRequestStatusId)
				.OnDelete(DeleteBehavior.Restrict);

			//..............................................................................//

			//add the default contract statuses to the database
			builder.Entity<ContractStatus>().HasData(
				new ContractStatus { ContractStatusId = ContractStatusConstants.DraftId, StatusName = ContractStatusConstants.DraftName },
				new ContractStatus { ContractStatusId = ContractStatusConstants.ActiveId, StatusName = ContractStatusConstants.ActiveName },
				new ContractStatus { ContractStatusId = ContractStatusConstants.OnHoldId, StatusName = ContractStatusConstants.OnHoldName },
				new ContractStatus { ContractStatusId = ContractStatusConstants.ExpiredId, StatusName = ContractStatusConstants.ExpiredName }
			);

			//..............................................................................//

			//add the default service request statuses to the database
			builder.Entity<ServiceRequestStatus>().HasData(
				new ServiceRequestStatus { ServiceRequestStatusId = 1, StatusName = "Pending" },
				new ServiceRequestStatus { ServiceRequestStatusId = 2, StatusName = "Approved" },
				new ServiceRequestStatus { ServiceRequestStatusId = 3, StatusName = "In Progress" },
				new ServiceRequestStatus { ServiceRequestStatusId = 4, StatusName = "Completed" },
				new ServiceRequestStatus { ServiceRequestStatusId = 5, StatusName = "Cancelled" }
			);

			//..............................................................................//
		}
	}
}

//.....................................o0oEND OF FILEo0o........................................//

