using GLMS.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

//ST10445500 - PROG7311 - GLMS POE
//DatabaseSeeder

//.....................................o0oSTART OF FILEo0o........................................//

// Seed data gives the system known starting records for testing and demo use.

namespace GLMS.Api.Data.Seeding
{
	public static class DatabaseSeeder
	{
		public static async Task SeedDatabaseIfNeededAsync(this WebApplication app)
		{
			using var scope = app.Services.CreateScope();

			var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

			bool seedingDisabled = app.Configuration.GetValue<bool>("DatabaseSeeding:Disabled");
			bool forceSeeding = app.Configuration.GetValue<bool>("DatabaseSeeding:Force");

			if (seedingDisabled && !forceSeeding)
			{
				return;
			}

			await context.Database.MigrateAsync();

			bool databaseHasData = await DatabaseHasApplicationDataAsync(context);

			if (databaseHasData && !forceSeeding)
			{
				await DemoSeedData.EnsureDemoContractFilesAsync(
					context,
					app.Configuration,
					app.Environment);
				return;
			}

			await DemoSeedData.SeedAsync(
				context,
				userManager,
				roleManager,
				app.Configuration,
				app.Environment,
				forceSeeding);
		}

		private static async Task<bool> DatabaseHasApplicationDataAsync(ApplicationDbContext context)
		{
			bool hasUsers = await context.Users.AnyAsync();
			bool hasClients = await context.Clients.AnyAsync();
			bool hasContracts = await context.Contracts.AnyAsync();
			bool hasServiceRequests = await context.ServiceRequests.AnyAsync();

			return hasUsers || hasClients || hasContracts || hasServiceRequests;
		}
	}
}

//.....................................o0oEND OF FILEo0o..........................................//
