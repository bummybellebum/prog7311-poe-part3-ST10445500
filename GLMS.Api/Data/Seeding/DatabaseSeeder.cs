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
