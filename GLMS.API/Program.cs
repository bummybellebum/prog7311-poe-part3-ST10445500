using GLMS.Api.ApiHelpers;
using GLMS.Api.Data;
using GLMS.Api.Data.Repositories;
using GLMS.Api.Data.Seeding;
using GLMS.Api.Models;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GLMS.Api
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			var isTesting = builder.Environment.IsEnvironment("Testing");
			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
			if (string.IsNullOrWhiteSpace(connectionString) && !isTesting)
			{
				throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
			}

			// Database
			if (!isTesting)
			{
				builder.Services.AddDbContext<ApplicationDbContext>(options =>
					options.UseSqlServer(connectionString));
			}

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

			var jwtKey = builder.Configuration["Jwt:Key"];
			if (string.IsNullOrWhiteSpace(jwtKey) && !isTesting)
			{
				throw new InvalidOperationException("JWT key is missing.");
			}

			jwtKey ??= "01234567890123456789012345678901";

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

				options.Events = new JwtBearerEvents
				{
					OnChallenge = async context =>
					{
						context.HandleResponse();
						context.Response.StatusCode = StatusCodes.Status401Unauthorized;
						context.Response.ContentType = "application/json";
						await context.Response.WriteAsync(JsonSerializer.Serialize(new ApiErrorResponse("Authentication is required.")));
					},
					OnForbidden = async context =>
					{
						context.Response.StatusCode = StatusCodes.Status403Forbidden;
						context.Response.ContentType = "application/json";
						await context.Response.WriteAsync(JsonSerializer.Serialize(new ApiErrorResponse("You do not have permission to access this resource.")));
					}
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
			builder.Services.AddScoped<IUserRepository, UserRepository>();

			// Application services
			builder.Services.AddScoped<IAuthService, AuthService>();
			builder.Services.AddScoped<IAccountService, AccountService>();
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
					options.Filters.Add<ApiExceptionFilter>();
				})
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
				});

			builder.Services.Configure<ApiBehaviorOptions>(options =>
			{
				options.InvalidModelStateResponseFactory = context =>
				{
					var errors = context.ModelState.Values
						.SelectMany(value => value.Errors)
						.Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
							? "The request is invalid."
							: error.ErrorMessage)
						.ToArray();

					return new BadRequestObjectResult(new ApiErrorResponse(errors));
				};
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

			if (!app.Environment.IsEnvironment("Testing"))
			{
				await app.SeedDatabaseIfNeededAsync();
			}

			app.Run();
		}
	}
}
