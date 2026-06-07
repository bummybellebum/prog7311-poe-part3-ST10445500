using Microsoft.EntityFrameworkCore;
using GLMS.Api.Models;

//ST10445500 - PROG7311 - GLMS POE
//ClientRepository

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Data.Repositories
{
    //..............................................................................//
    
    //this interface lets us do things with Client data in the database.
    public interface IClientRepository : IRepository<Client>
    {
        Task<List<Client>> GetClientsAsync(string? search = null);
        Task<Client?> GetClientWithContractsAsync(int clientId);
        Task<bool> IsCompanyNameUniqueAsync(string companyName, int? excludeClientId = null);
    }

    //..............................................................................//

    //this class handles all the database operations for Clients.
    //it has extra methods for things we do with clients a lot.
    public class ClientRepository : Repository<Client>, IClientRepository
    {
        public ClientRepository(ApplicationDbContext context) : base(context)
        {
        }

        //..............................................................................//

        public override async Task<List<Client>> GetAllAsync()
        {
            return await GetClientsAsync();
        }

        //..............................................................................//

        //gets clients for list pages, optionally searched by company name or email.
        public async Task<List<Client>> GetClientsAsync(string? search = null)
        {
            var query = Query();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim().ToLower();
                query = query.Where(c =>
                    c.CompanyName.ToLower().Contains(normalizedSearch) ||
                    c.Email.ToLower().Contains(normalizedSearch));
            }

            return await query
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
        }

        //..............................................................................//

        //gets one client and also gets all the contracts they have.
        public async Task<Client?> GetClientWithContractsAsync(int clientId)
        {
            return await Query()
                .Include(c => c.Contracts)
                    .ThenInclude(c => c.ContractStatus)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);
        }

        //..............................................................................//

        //checks if a company name is already used by someone else.
        //if we give a client ID, it won't count that client's current name.
        public async Task<bool> IsCompanyNameUniqueAsync(string companyName, int? excludeClientId = null)
        {
            var query = Query()
                .Where(c => c.CompanyName.ToLower() == companyName.ToLower());

            if (excludeClientId.HasValue)
            {
                query = query.Where(c => c.ClientId != excludeClientId.Value);
            }

            return !await query.AnyAsync();
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o........................................//

