using GLMS.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace GLMS.Api.Data.Seeding
{
	public static class DemoSeedData
	{
		public static async Task SeedAsync(
			ApplicationDbContext context,
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager,
			bool forceSeeding)
		{
			await SeedRolesAsync(roleManager);
			await SeedUsersAsync(userManager);

			// Later we will add the real demo data here:
			// await SeedClientsAsync(context);
			// await SeedContractsAsync(context);
			// await SeedServiceRequestsAsync(context);
			// await SeedContractDocumentsAsync(context);

			await context.SaveChangesAsync();
		}

		private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
		{
			string[] roles =
			{
				"Admin",
				"Manager",
				"Staff"
			};

			foreach (string role in roles)
			{
				bool roleExists = await roleManager.RoleExistsAsync(role);

				if (!roleExists)
				{
					await roleManager.CreateAsync(new IdentityRole(role));
				}
			}
		}

		private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
		{
			// Placeholder for now.
			// Once the exact ApplicationUser fields are confirmed,
			// we will add lecturer-demo users here.

			await Task.CompletedTask;
		}
	}
}
