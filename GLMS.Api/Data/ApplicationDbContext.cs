using GLMS.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

//ST10445500 - PROG7311 - GLMS POE
//ApplicationDbContext

//.....................................o0oSTART OF FILEo0o........................................//

// The DbContext maps the GLMS models to the database tables used by the API.

namespace GLMS.Api.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		//..............................................................................//

		public DbSet<Client> Clients { get; set; } = null!;
		public DbSet<ContractStatus> ContractStatuses { get; set; } = null!;
		public DbSet<Contract> Contracts { get; set; } = null!;
		public DbSet<ContractDocument> ContractDocuments { get; set; } = null!;
		public DbSet<ServiceRequestStatus> ServiceRequestStatuses { get; set; } = null!;
		public DbSet<ServiceRequest> ServiceRequests { get; set; } = null!;

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
				new ServiceRequestStatus { ServiceRequestStatusId = ServiceRequestStatusConstants.PendingId, StatusName = ServiceRequestStatusConstants.PendingName },
				new ServiceRequestStatus { ServiceRequestStatusId = ServiceRequestStatusConstants.ApprovedId, StatusName = ServiceRequestStatusConstants.ApprovedName },
				new ServiceRequestStatus { ServiceRequestStatusId = ServiceRequestStatusConstants.InProgressId, StatusName = ServiceRequestStatusConstants.InProgressName },
				new ServiceRequestStatus { ServiceRequestStatusId = ServiceRequestStatusConstants.CompletedId, StatusName = ServiceRequestStatusConstants.CompletedName },
				new ServiceRequestStatus { ServiceRequestStatusId = ServiceRequestStatusConstants.CancelledId, StatusName = ServiceRequestStatusConstants.CancelledName }
			);

			//..............................................................................//
		}
	}
}

//.....................................o0oEND OF FILEo0o..........................................//
