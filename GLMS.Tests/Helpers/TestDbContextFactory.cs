using GLMS.Api.Data;
using GLMS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Tests.Helpers
{
    public static class TestDbContextFactory
    {
        public static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        public static async Task SeedCoreDataAsync(ApplicationDbContext context)
        {
            var activeStatus = TestData.ContractStatus(ContractStatusConstants.ActiveId, ContractStatusConstants.ActiveName);
            var draftStatus = TestData.ContractStatus(ContractStatusConstants.DraftId, ContractStatusConstants.DraftName);
            var pendingStatus = TestData.ServiceRequestStatus(1, "Pending");
            var completedStatus = TestData.ServiceRequestStatus(4, "Completed");
            var user = TestData.User();

            var firstClient = TestData.Client(1, "Acme Logistics");
            var secondClient = TestData.Client(2, "Global Freight");

            var firstContract = TestData.Contract(1, 1, activeStatus.ContractStatusId, activeStatus.StatusName, new DateTime(2026, 1, 10));
            var secondContract = TestData.Contract(2, 2, activeStatus.ContractStatusId, activeStatus.StatusName, new DateTime(2026, 2, 15));
            var draftContract = TestData.Contract(3, 1, draftStatus.ContractStatusId, draftStatus.StatusName, new DateTime(2026, 1, 20));
            firstContract.ContractStatus = activeStatus;
            secondContract.ContractStatus = activeStatus;
            draftContract.ContractStatus = draftStatus;

            context.Users.Add(user);
            context.Clients.AddRange(firstClient, secondClient);
            context.ContractStatuses.AddRange(activeStatus, draftStatus);
            context.ServiceRequestStatuses.AddRange(pendingStatus, completedStatus);
            context.Contracts.AddRange(firstContract, secondContract, draftContract);
            context.ServiceRequests.AddRange(
                TestData.ServiceRequest(1, 1, 1, new DateTime(2026, 1, 11)),
                TestData.ServiceRequest(2, 2, 4, new DateTime(2026, 2, 16)));
            context.ContractDocuments.AddRange(
                TestData.Document(1, 1, isCurrent: false),
                TestData.Document(2, 1, isCurrent: true));

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        public static async Task SeedIntegrationDataAsync(ApplicationDbContext context)
        {
            context.Users.Add(TestData.User());
            context.Clients.Add(TestData.Client());
            context.ContractStatuses.AddRange(
                TestData.ContractStatus(ContractStatusConstants.DraftId, ContractStatusConstants.DraftName),
                TestData.ContractStatus(ContractStatusConstants.ActiveId, ContractStatusConstants.ActiveName),
                TestData.ContractStatus(ContractStatusConstants.OnHoldId, ContractStatusConstants.OnHoldName),
                TestData.ContractStatus(ContractStatusConstants.ExpiredId, ContractStatusConstants.ExpiredName));
            context.ServiceRequestStatuses.Add(TestData.ServiceRequestStatus());
            var activeContract = TestData.Contract(1, statusId: ContractStatusConstants.ActiveId, statusName: ContractStatusConstants.ActiveName, startDate: new DateTime(2026, 1, 10));
            var expiredContract = TestData.Contract(2, statusId: ContractStatusConstants.ExpiredId, statusName: ContractStatusConstants.ExpiredName, startDate: new DateTime(2025, 1, 10));
            var onHoldContract = TestData.Contract(3, statusId: ContractStatusConstants.OnHoldId, statusName: ContractStatusConstants.OnHoldName, startDate: new DateTime(2026, 2, 10));

            activeContract.Title = "Active Integration Contract";
            expiredContract.Title = "Expired Integration Contract";
            onHoldContract.Title = "On Hold Integration Contract";

            activeContract.ContractStatus = context.ContractStatuses.Local.First(s => s.ContractStatusId == ContractStatusConstants.ActiveId);
            expiredContract.ContractStatus = context.ContractStatuses.Local.First(s => s.ContractStatusId == ContractStatusConstants.ExpiredId);
            onHoldContract.ContractStatus = context.ContractStatuses.Local.First(s => s.ContractStatusId == ContractStatusConstants.OnHoldId);

            context.Contracts.AddRange(activeContract, expiredContract, onHoldContract);

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }
    }
}
