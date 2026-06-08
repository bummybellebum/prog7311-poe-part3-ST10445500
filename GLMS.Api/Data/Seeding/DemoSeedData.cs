using GLMS.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;

//ST10445500 - PROG7311 - GLMS POE
//DemoSeedData

//.....................................o0oSTART OF FILEo0o........................................//

// Seed data gives the system known starting records for testing and demo use.

namespace GLMS.Api.Data.Seeding
{
	public static class DemoSeedData
	{
		public static async Task SeedAsync(
			ApplicationDbContext context,
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager,
			IConfiguration configuration,
			IWebHostEnvironment environment,
			bool forceSeeding)
		{
			await SeedRolesAsync(roleManager);
			await ValidateLookupStatusesAsync(context);

			var users = await SeedUsersAsync(userManager, configuration);

			var clients = await SeedClientsAsync(context);
			var contracts = await SeedContractsAsync(context, clients, users.ContractManager.Id);

			await SeedContractDocumentsAsync(context, contracts, users.Admin.Id, environment);
			await SeedServiceRequestsAsync(context, contracts, users.LogisticsManager.Id);

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

		private static async Task<SeedUsers> SeedUsersAsync(
			UserManager<ApplicationUser> userManager,
			IConfiguration configuration)
		{
			var adminPassword = configuration["SeedAdmin:Password"] ?? "Admin1234!";
			var defaultPassword = configuration["SeedUsers:DefaultPassword"] ?? "User1234!";

			var admin = await CreateUserIfMissingAsync(
				userManager,
				configuration["SeedAdmin:Email"] ?? "admin@glms.co.za",
				adminPassword,
				"Admin",
				"Dude",
				ApplicationRoles.Admin);

			var logisticsManager = await CreateUserIfMissingAsync(
				userManager,
				configuration["SeedLogisticsManager:Email"] ?? "logistics@glms.co.za",
				defaultPassword,
				"Logistics",
				"Manager",
				ApplicationRoles.LogisticsManager);

			var contractManager = await CreateUserIfMissingAsync(
				userManager,
				configuration["SeedContractManager:Email"] ?? "contracts@glms.co.za",
				defaultPassword,
				"Contracts",
				"Manager",
				ApplicationRoles.ContractManager);

			return new SeedUsers
			{
				Admin = admin,
				LogisticsManager = logisticsManager,
				ContractManager = contractManager
			};
		}

		private static async Task<ApplicationUser> CreateUserIfMissingAsync(
			UserManager<ApplicationUser> userManager,
			string email,
			string password,
			string firstName,
			string lastName,
			string role)
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				throw new InvalidOperationException("Seed user email is missing.");
			}

			if (string.IsNullOrWhiteSpace(password))
			{
				throw new InvalidOperationException($"Seed password is missing for {email}.");
			}

			var user = await userManager.FindByEmailAsync(email);

			if (user == null)
			{
				user = new ApplicationUser
				{
					UserName = email,
					Email = email,
					EmailConfirmed = true,
					FirstName = firstName,
					LastName = lastName,
					IsActive = true,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				var createResult = await userManager.CreateAsync(user, password);

				if (!createResult.Succeeded)
				{
					throw new InvalidOperationException(
						$"Failed to seed user '{email}': {GetErrors(createResult)}");
				}
			}
			else
			{
				user.FirstName = firstName;
				user.LastName = lastName;
				user.IsActive = true;
				user.UpdatedAt = DateTime.UtcNow;

				var updateResult = await userManager.UpdateAsync(user);

				if (!updateResult.Succeeded)
				{
					throw new InvalidOperationException(
						$"Failed to update seed user '{email}': {GetErrors(updateResult)}");
				}
			}

			if (!await userManager.IsInRoleAsync(user, role))
			{
				var roleResult = await userManager.AddToRoleAsync(user, role);

				if (!roleResult.Succeeded)
				{
					throw new InvalidOperationException(
						$"Failed to add user '{email}' to role '{role}': {GetErrors(roleResult)}");
				}
			}

			return user;
		}

		private static async Task ValidateLookupStatusesAsync(ApplicationDbContext context)
		{
			bool contractStatusesValid =
				await context.ContractStatuses.AnyAsync(status =>
					status.ContractStatusId == ContractStatusConstants.DraftId &&
					status.StatusName == ContractStatusConstants.DraftName) &&

				await context.ContractStatuses.AnyAsync(status =>
					status.ContractStatusId == ContractStatusConstants.ActiveId &&
					status.StatusName == ContractStatusConstants.ActiveName) &&

				await context.ContractStatuses.AnyAsync(status =>
					status.ContractStatusId == ContractStatusConstants.OnHoldId &&
					status.StatusName == ContractStatusConstants.OnHoldName) &&

				await context.ContractStatuses.AnyAsync(status =>
					status.ContractStatusId == ContractStatusConstants.ExpiredId &&
					status.StatusName == ContractStatusConstants.ExpiredName);

			bool serviceRequestStatusesValid =
				await context.ServiceRequestStatuses.AnyAsync(status =>
					status.ServiceRequestStatusId == ServiceRequestStatusConstants.PendingId &&
					status.StatusName == ServiceRequestStatusConstants.PendingName) &&

				await context.ServiceRequestStatuses.AnyAsync(status =>
					status.ServiceRequestStatusId == ServiceRequestStatusConstants.InProgressId &&
					status.StatusName == ServiceRequestStatusConstants.InProgressName) &&

				await context.ServiceRequestStatuses.AnyAsync(status =>
					status.ServiceRequestStatusId == ServiceRequestStatusConstants.CompletedId &&
					status.StatusName == ServiceRequestStatusConstants.CompletedName) &&

				await context.ServiceRequestStatuses.AnyAsync(status =>
					status.ServiceRequestStatusId == ServiceRequestStatusConstants.CancelledId &&
					status.StatusName == ServiceRequestStatusConstants.CancelledName);

			if (!contractStatusesValid || !serviceRequestStatusesValid)
			{
				throw new InvalidOperationException(
					"Seed lookup statuses are missing or do not match the application constants.");
			}
		}

		private static async Task<Dictionary<string, Client>> SeedClientsAsync(ApplicationDbContext context)
		{
			var now = DateTime.UtcNow;

			var clients = new[]
			{
				new Client
				{
					CompanyName = "CapeRoute Imports",
					Email = "operations@caperoute.co.za",
					Phone = "+27 21 555 0101",
					Region = "Africa",
					Country = "South Africa",
					IsActive = true,
					CreatedAt = now,
					UpdatedAt = now
				},
				new Client
				{
					CompanyName = "Nova Global Freight",
					Email = "contracts@novaglobal.com",
					Phone = "+1 212 555 0188",
					Region = "North America",
					Country = "United States",
					IsActive = true,
					CreatedAt = now,
					UpdatedAt = now
				},
				new Client
				{
					CompanyName = "EuroBridge Distribution",
					Email = "sla@eurobridge.eu",
					Phone = "+49 30 555 0144",
					Region = "Europe",
					Country = "Germany",
					IsActive = true,
					CreatedAt = now,
					UpdatedAt = now
				},
				new Client
				{
					CompanyName = "Sahara Cross-Border Logistics",
					Email = "dispatch@saharalogistics.co.za",
					Phone = "+27 11 555 0199",
					Region = "Africa",
					Country = "South Africa",
					IsActive = true,
					CreatedAt = now,
					UpdatedAt = now
				},
				new Client
				{
					CompanyName = "Pacific Cold Chain",
					Email = "support@pacificcoldchain.com",
					Phone = "+61 2 5550 0177",
					Region = "Asia-Pacific",
					Country = "Australia",
					IsActive = true,
					CreatedAt = now,
					UpdatedAt = now
				}
			};

			foreach (var client in clients)
			{
				if (!await context.Clients.AnyAsync(existing => existing.CompanyName == client.CompanyName))
				{
					context.Clients.Add(client);
				}
			}

			await context.SaveChangesAsync();

			var companyNames = clients.Select(client => client.CompanyName).ToList();

			return await context.Clients
				.Where(client => companyNames.Contains(client.CompanyName))
				.ToDictionaryAsync(client => client.CompanyName);
		}

		private static async Task<Dictionary<string, Contract>> SeedContractsAsync(
			ApplicationDbContext context,
			Dictionary<string, Client> clients,
			string createdByUserId)
		{
			var today = DateTime.UtcNow.Date;
			var now = DateTime.UtcNow;

			var contracts = new[]
			{
				new Contract
				{
					Title = "Global Ocean Freight Agreement",
					ClientId = clients["Nova Global Freight"].ClientId,
					ContractStatusId = ContractStatusConstants.ActiveId,
					CreatedByUserId = createdByUserId,
					StartDate = today.AddMonths(-8),
					EndDate = today.AddMonths(16),
					ServiceLevel = "Premium",
					Notes = "Active international ocean freight contract for high-volume container shipments.",
					CreatedAt = now,
					UpdatedAt = now
				},
				new Contract
				{
					Title = "Express Air Freight SLA",
					ClientId = clients["CapeRoute Imports"].ClientId,
					ContractStatusId = ContractStatusConstants.ActiveId,
					CreatedByUserId = createdByUserId,
					StartDate = today.AddMonths(-3),
					EndDate = today.AddMonths(9),
					ServiceLevel = "Express",
					Notes = "Active SLA for urgent air freight and priority customs clearance.",
					CreatedAt = now,
					UpdatedAt = now
				},
				new Contract
				{
					Title = "Cross Border Road Freight Contract",
					ClientId = clients["Sahara Cross-Border Logistics"].ClientId,
					ContractStatusId = ContractStatusConstants.ActiveId,
					CreatedByUserId = createdByUserId,
					StartDate = today.AddMonths(-5),
					EndDate = today.AddMonths(7),
					ServiceLevel = "Standard",
					Notes = "Active road freight contract for SADC cross-border deliveries.",
					CreatedAt = now,
					UpdatedAt = now
				},
				new Contract
				{
					Title = "Cold Chain Pharmaceutical Contract",
					ClientId = clients["Pacific Cold Chain"].ClientId,
					ContractStatusId = ContractStatusConstants.OnHoldId,
					CreatedByUserId = createdByUserId,
					StartDate = today.AddMonths(-2),
					EndDate = today.AddMonths(10),
					ServiceLevel = "Cold Chain",
					Notes = "Placed on hold while updated compliance documents are reviewed.",
					CreatedAt = now,
					UpdatedAt = now
				},
				new Contract
				{
					Title = "Legacy Port Handling Contract",
					ClientId = clients["EuroBridge Distribution"].ClientId,
					ContractStatusId = ContractStatusConstants.ExpiredId,
					CreatedByUserId = createdByUserId,
					StartDate = today.AddMonths(-18),
					EndDate = today.AddMonths(-1),
					ServiceLevel = "Economy",
					Notes = "Expired contract retained for reporting and lecturer demonstration.",
					CreatedAt = now,
					UpdatedAt = now
				},
				new Contract
				{
					Title = "Draft Warehousing Proposal",
					ClientId = clients["CapeRoute Imports"].ClientId,
					ContractStatusId = ContractStatusConstants.DraftId,
					CreatedByUserId = createdByUserId,
					StartDate = today.AddMonths(1),
					EndDate = today.AddMonths(13),
					ServiceLevel = "Draft",
					Notes = "Draft contract used to demonstrate status filtering.",
					CreatedAt = now,
					UpdatedAt = now
				}
			};

			foreach (var contract in contracts)
			{
				if (!await context.Contracts.AnyAsync(existing => existing.Title == contract.Title))
				{
					context.Contracts.Add(contract);
				}
			}

			await context.SaveChangesAsync();

			var contractTitles = contracts.Select(contract => contract.Title).ToList();

			return await context.Contracts
				.Where(contract => contractTitles.Contains(contract.Title))
				.ToDictionaryAsync(contract => contract.Title);
		}

		private static async Task SeedContractDocumentsAsync(
			ApplicationDbContext context,
			Dictionary<string, Contract> contracts,
			string uploadedByUserId,
			IWebHostEnvironment environment)
		{
			foreach (var contract in contracts.Values)
			{
				bool alreadyHasSignedAgreement = await context.ContractDocuments.AnyAsync(document =>
					document.ContractId == contract.ContractId &&
					document.DocumentType == "Signed Agreement" &&
					document.IsCurrent);

				if (alreadyHasSignedAgreement)
				{
					continue;
				}

				var safeName = ToSafeFileName(contract.Title);
				var storedFileName = $"{safeName}-signed-agreement.pdf";
				var originalFileName = $"{safeName}-original.pdf";

				var file = await CreateDemoPdfFileAsync(environment, storedFileName, contract.Title);

				context.ContractDocuments.Add(new ContractDocument
				{
					ContractId = contract.ContractId,
					UploadedByUserId = uploadedByUserId,
					DocumentType = "Signed Agreement",
					OriginalFileName = originalFileName,
					StoredFileName = storedFileName,
					FilePath = file.RelativePath,
					ContentType = "application/pdf",
					FileSizeBytes = file.SizeBytes,
					IsCurrent = true,
					UploadedAt = DateTime.UtcNow
				});
			}
		}

		private static async Task SeedServiceRequestsAsync(
			ApplicationDbContext context,
			Dictionary<string, Contract> contracts,
			string requestedByUserId)
		{
			var today = DateTime.UtcNow.Date;

			var requests = new[]
			{
				CreateServiceRequest(
					contracts["Global Ocean Freight Agreement"].ContractId,
					requestedByUserId,
					ServiceRequestStatusConstants.InProgressId,
					"Arrange Durban to Rotterdam container shipment",
					12500m,
					"USD",
					18.45m,
					today.AddDays(-10)),

				CreateServiceRequest(
					contracts["Global Ocean Freight Agreement"].ContractId,
					requestedByUserId,
					ServiceRequestStatusConstants.InProgressId,
					"Schedule customs clearance for European delivery",
					4300m,
					"EUR",
					20.10m,
					today.AddDays(-5)),

				CreateServiceRequest(
					contracts["Express Air Freight SLA"].ContractId,
					requestedByUserId,
					ServiceRequestStatusConstants.PendingId,
					"Book express air freight for urgent electronics shipment",
					7200m,
					"USD",
					18.45m,
					today.AddDays(-2)),

				CreateServiceRequest(
					contracts["Cross Border Road Freight Contract"].ContractId,
					requestedByUserId,
					ServiceRequestStatusConstants.CompletedId,
					"Arrange Johannesburg to Gaborone road freight delivery",
					38500m,
					"ZAR",
					1.00m,
					today.AddDays(-20))
			};

			foreach (var request in requests)
			{
				if (!await context.ServiceRequests.AnyAsync(existing =>
					existing.Description == request.Description &&
					existing.ContractId == request.ContractId))
				{
					context.ServiceRequests.Add(request);
				}
			}
		}

		private static ServiceRequest CreateServiceRequest(
			int contractId,
			string requestedByUserId,
			int statusId,
			string description,
			decimal amountOriginal,
			string currencyCode,
			decimal exchangeRateToZar,
			DateTime requestedAt)
		{
			return new ServiceRequest
			{
				ContractId = contractId,
				RequestedByUserId = requestedByUserId,
				ServiceRequestStatusId = statusId,
				Description = description,
				AmountOriginal = amountOriginal,
				OriginalCurrencyCode = currencyCode,
				ExchangeRateToZAR = exchangeRateToZar,
				AmountZAR = Math.Round(amountOriginal * exchangeRateToZar, 2),
				RequestedAt = requestedAt,
				UpdatedAt = DateTime.UtcNow
			};
		}

		private static async Task<DemoFile> CreateDemoPdfFileAsync(
			IWebHostEnvironment environment,
			string storedFileName,
			string contractTitle)
		{
			var webRootPath = environment.WebRootPath;

			if (string.IsNullOrWhiteSpace(webRootPath))
			{
				webRootPath = Path.Combine(environment.ContentRootPath, "wwwroot");
			}

			var relativeFolder = Path.Combine("uploads", "signed-agreements");
			var fullFolder = Path.Combine(webRootPath, relativeFolder);

			Directory.CreateDirectory(fullFolder);

			var fullPath = Path.Combine(fullFolder, storedFileName);

			if (!File.Exists(fullPath))
			{
				var pdfBytes = BuildDemoPdfBytes(contractTitle);
				await File.WriteAllBytesAsync(fullPath, pdfBytes);
			}

			var fileInfo = new FileInfo(fullPath);
			var relativePath = Path.Combine(relativeFolder, storedFileName).Replace("\\", "/");

			return new DemoFile(relativePath, fileInfo.Length);
		}

		private static byte[] BuildDemoPdfBytes(string contractTitle)
		{
			var escapedTitle = EscapePdfText(contractTitle);

			var stream =
				"BT " +
				"/F1 16 Tf " +
				"72 720 Td " +
				"(GLMS Demo Signed Agreement) Tj " +
				"0 -28 Td " +
				$"({escapedTitle}) Tj " +
				"0 -28 Td " +
				"(Generated for lecturer demonstration data.) Tj " +
				"ET";

			var objects = new[]
			{
				"1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n",
				"2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n",
				"3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n",
				"4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n",
				$"5 0 obj\n<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}\nendstream\nendobj\n"
			};

			var builder = new StringBuilder();
			var offsets = new List<int>();

			builder.Append("%PDF-1.4\n");

			foreach (var item in objects)
			{
				offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
				builder.Append(item);
			}

			var xrefOffset = Encoding.ASCII.GetByteCount(builder.ToString());

			builder.Append("xref\n");
			builder.Append($"0 {objects.Length + 1}\n");
			builder.Append("0000000000 65535 f \n");

			foreach (var offset in offsets)
			{
				builder.Append($"{offset:D10} 00000 n \n");
			}

			builder.Append("trailer\n");
			builder.Append($"<< /Size {objects.Length + 1} /Root 1 0 R >>\n");
			builder.Append("startxref\n");
			builder.Append(xrefOffset);
			builder.Append("\n%%EOF");

			return Encoding.ASCII.GetBytes(builder.ToString());
		}

		private static string EscapePdfText(string value)
		{
			return value
				.Replace("\\", "\\\\")
				.Replace("(", "\\(")
				.Replace(")", "\\)");
		}

		private static string ToSafeFileName(string value)
		{
			var characters = value
				.ToLowerInvariant()
				.Select(character => char.IsLetterOrDigit(character) ? character : '-')
				.ToArray();

			var fileName = new string(characters);

			while (fileName.Contains("--"))
			{
				fileName = fileName.Replace("--", "-");
			}

			return fileName.Trim('-');
		}

		private static string GetErrors(IdentityResult result)
		{
			return string.Join("; ", result.Errors.Select(error => error.Description));
		}

		private sealed class SeedUsers
		{
			public ApplicationUser Admin { get; set; } = null!;

			public ApplicationUser LogisticsManager { get; set; } = null!;

			public ApplicationUser ContractManager { get; set; } = null!;
		}

		private sealed record DemoFile(string RelativePath, long SizeBytes);
	}
}

//.....................................o0oEND OF FILEo0o..........................................//
