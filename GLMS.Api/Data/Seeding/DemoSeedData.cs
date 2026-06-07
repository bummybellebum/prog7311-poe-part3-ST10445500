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
			IConfiguration configuration,
			bool forceSeeding)
		{
			await SeedRolesAsync(roleManager);
			await SeedDefaultAdminAsync(userManager, configuration);

			// Later we add the demo app data here:
			// await SeedClientsAsync(context);
			// await SeedContractsAsync(context);
			// await SeedServiceRequestsAsync(context);
			// await SeedContractDocumentsAsync(context);

			await context.SaveChangesAsync();
		}

		private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
		{
			foreach (var role in ApplicationRoles.All)
			{
				if (!await roleManager.RoleExistsAsync(role))
				{
					var result = await roleManager.CreateAsync(new IdentityRole(role));

					if (!result.Succeeded)
					{
						throw new InvalidOperationException(
							$"Failed to seed role '{role}': {GetErrors(result)}");
					}
				}
			}
		}

		private static async Task SeedDefaultAdminAsync(
			UserManager<ApplicationUser> userManager,
			IConfiguration configuration)
		{
			var adminEmail = configuration["SeedAdmin:Email"] ?? "admin@gmail.com";
			var adminPassword = configuration["SeedAdmin:Password"] ?? "Admin1234!";

			if (string.IsNullOrWhiteSpace(adminEmail))
			{
				throw new InvalidOperationException("Seed admin email is missing.");
			}

			if (string.IsNullOrWhiteSpace(adminPassword))
			{
				throw new InvalidOperationException(
					"Seed admin password is missing. Configure SeedAdmin:Password before starting with an empty database.");
			}

			var adminUser = await userManager.FindByEmailAsync(adminEmail);

			if (adminUser == null)
			{
				adminUser = new ApplicationUser
				{
					UserName = adminEmail,
					Email = adminEmail,
					EmailConfirmed = true,
					FirstName = "Admin",
					LastName = "Dude",
					IsActive = true,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				var createResult = await userManager.CreateAsync(adminUser, adminPassword);

				if (!createResult.Succeeded)
				{
					throw new InvalidOperationException(
						$"Failed to seed default admin user: {GetErrors(createResult)}");
				}
			}

			if (!await userManager.IsInRoleAsync(adminUser, ApplicationRoles.Admin))
			{
				var roleResult = await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Admin);

				if (!roleResult.Succeeded)
				{
					throw new InvalidOperationException(
						$"Failed to add default admin user to Admin role: {GetErrors(roleResult)}");
				}
			}
		}

		private static string GetErrors(IdentityResult result)
		{
			return string.Join("; ", result.Errors.Select(error => error.Description));
		}
	}
}
