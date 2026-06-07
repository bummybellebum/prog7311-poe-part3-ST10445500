using GLMS.Api.Data;
using GLMS.Api.Data.Repositories;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;

namespace GLMS.Api
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

			// Database
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(connectionString));

			// Identity and JWT authentication
			builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
			{
				options.Password.RequireDigit = true;
				options.Password.RequiredLength = 8;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = false;
				options.Password.RequireLowercase = true;
				options.User.RequireUniqueEmail = true;
			})
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddDefaultTokenProviders();

			var jwtKey = builder.Configuration["Jwt:Key"]
				?? throw new InvalidOperationException("JWT key is missing.");

			builder.Services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = builder.Configuration["Jwt:Issuer"],
					ValidAudience = builder.Configuration["Jwt:Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
				};
			});

			builder.Services.AddAuthorization(options =>
			{
				options.FallbackPolicy = new AuthorizationPolicyBuilder()
					.RequireAuthenticatedUser()
					.Build();
			});

			builder.Services.AddHttpContextAccessor();

			// Repositories
			builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
			builder.Services.AddScoped<IClientRepository, ClientRepository>();
			builder.Services.AddScoped<IContractRepository, ContractRepository>();
			builder.Services.AddScoped<IContractDocumentRepository, ContractDocumentRepository>();
			builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();

			// Application services
			builder.Services.AddScoped<IAuthService, AuthService>();
			builder.Services.AddScoped<IAccountService, AccountService>();
			builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
			builder.Services.AddScoped<IClientService, ClientService>();
			builder.Services.AddScoped<IContractService, ContractService>();
			builder.Services.AddScoped<IContractDocumentService, ContractDocumentService>();
			builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
			builder.Services.AddScoped<ILookupService, LookupService>();

			// External HTTP clients
			builder.Services.AddHttpClient<ICurrencyExchangeService, CurrencyExchangeService>((sp, client) =>
			{
				var configuration = sp.GetRequiredService<IConfiguration>();
				var baseUrl = configuration["CurrencyApi:BaseUrl"] ?? "https://api.frankfurter.dev";
				client.BaseAddress = new Uri(baseUrl);
				client.Timeout = TimeSpan.FromSeconds(15);
			});

			// Controllers and Swagger
			builder.Services.AddControllers(options =>
				{
					options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
				})
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
				});

			builder.Services.AddEndpointsApiExplorer();

			builder.Services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "GLMS API",
					Version = "v1",
					Description = "Global Logistics Management System API"
				});

				options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Name = "Authorization",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.Http,
					Scheme = "bearer",
					BearerFormat = "JWT",
					Description = "Enter your JWT token only. Do not type 'Bearer' before it."
				});

				options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
				{
					[new OpenApiSecuritySchemeReference("Bearer", document)] = []
				});
			});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI(options =>
				{
					options.SwaggerEndpoint("/swagger/v1/swagger.json", "GLMS API v1");
					options.RoutePrefix = "swagger";
				});
			}

			app.UseHttpsRedirection();

			app.UseAuthentication();
			app.UseAuthorization();

			app.MapControllers();

			try
			{
				using (var scope = app.Services.CreateScope())
				{
					var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
					db.Database.Migrate();
				}
			}
			catch (Exception ex)
			{
				var logger = app.Services.GetRequiredService<ILogger<Program>>();
				logger.LogError(ex, "An error occurred while migrating or initializing the database.");
				throw;
			}

			SeedDefaultAdminAsync(app).GetAwaiter().GetResult();

			app.Run();
		}

		private static async Task SeedDefaultAdminAsync(WebApplication app)
		{
			using var scope = app.Services.CreateScope();
			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

			var adminEmail = app.Configuration["SeedAdmin:Email"] ?? "admin@gmail.com";
			var adminPassword = app.Configuration["SeedAdmin:Password"];

			foreach (var role in ApplicationRoles.All)
			{
				if (!await roleManager.RoleExistsAsync(role))
				{
					await roleManager.CreateAsync(new IdentityRole(role));
				}
			}

			var adminUser = await userManager.FindByEmailAsync(adminEmail);
			if (adminUser == null)
			{
				if (string.IsNullOrWhiteSpace(adminPassword))
				{
					throw new InvalidOperationException("Seed admin password is missing. Configure SeedAdmin:Password before starting with an empty database.");
				}

				adminUser = new ApplicationUser
				{
					UserName = adminEmail,
					Email = adminEmail,
					EmailConfirmed = true,
					FirstName = "Admin",
					LastName = "Dude",
					IsActive = true,
					CreatedAt = DateTime.UtcNow
				};

				var createResult = await userManager.CreateAsync(adminUser, adminPassword);
				if (!createResult.Succeeded)
				{
					var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
					throw new InvalidOperationException($"Failed to seed default admin user: {errors}");
				}
			}

			if (!await userManager.IsInRoleAsync(adminUser, ApplicationRoles.Admin))
			{
				await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Admin);
			}
		}
	}
}
