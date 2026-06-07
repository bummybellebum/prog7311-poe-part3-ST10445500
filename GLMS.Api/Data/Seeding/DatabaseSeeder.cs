using GLMS.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Api.Data.Seeding
{
	public static class DatabaseSeeder
	{
		public static async Task SeedDatabaseIfNeededAsync(this WebApplication app)
		{
			using var scope = app.Services.CreateScope();

			var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
			var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			bool seedingDisabled = configuration.GetValue<bool>("DatabaseSeeding:Disabled");
			bool forceSeeding = configuration.GetValue<bool>("DatabaseSeeding:Force");

			if (seedingDisabled && !forceSeeding)
			{
				return;
			}

			// Apply migrations when the API starts.
			// This creates the database/tables on a fresh Docker SQL Server volume.
			await context.Database.MigrateAsync();

			bool databaseHasData = await DatabaseHasApplicationDataAsync(context);

			// Normal mode:
			// Seed only when the database is empty.
			if (databaseHasData && !forceSeeding)
			{
				return;
			}

			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

			await DemoSeedData.SeedAsync(
				context,
				userManager,
				roleManager,
				forceSeeding);
		}

		private static async Task<bool> DatabaseHasApplicationDataAsync(ApplicationDbContext context)
		{
			// Do not count EF migration history.
			// Only count real GLMS application data.

			bool hasUsers = await context.Users.AnyAsync();
			bool hasClients = await context.Clients.AnyAsync();
			bool hasContracts = await context.Contracts.AnyAsync();
			bool hasServiceRequests = await context.ServiceRequests.AnyAsync();

			return hasUsers || hasClients || hasContracts || hasServiceRequests;
		}
	}
}
