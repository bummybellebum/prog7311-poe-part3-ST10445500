using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

//ST10445500 - PROG7311 - GLMS POE
//ApplicationDbContextFactory

//.....................................o0oSTART OF FILEo0o........................................//

// The factory lets EF tooling create the DbContext without running the full API.

namespace GLMS.Api.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=(localdb)\\mssqllocaldb;Database=GLMSDesignTimeDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
