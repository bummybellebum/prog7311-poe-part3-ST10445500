using GLMS.Web.ViewModels.Account;
using GLMS.Web.ApiModels;

//ST10445500 - PROG7311 - GLMS POE
//TestingApiServices

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services.Testing
{
    public class TestingClientService : IClientService
    {
        public Task<List<ClientListDto>> GetAllAsync(string? search = null)
        {
            var clients = Data.Clients
                .Where(client => string.IsNullOrWhiteSpace(search)
                    || client.CompanyName.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || client.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Task.FromResult(clients);
        }

        public Task<ClientDetailDto?> GetByIdAsync(int id)
        {
            return Task.FromResult(Data.ClientDetails.FirstOrDefault(client => client.ClientId == id));
        }

        public Task<ClientDetailDto?> GetWithContractsAsync(int id) => GetByIdAsync(id);

        public Task<ClientDetailDto> CreateAsync(CreateClientDto client)
        {
            return Task.FromResult(Data.ClientDetails[0]);
        }

        public Task UpdateAsync(UpdateClientDto client) => Task.CompletedTask;

        public Task DeleteAsync(int id) => Task.CompletedTask;
    }

    //..............................................................................//

    public class TestingContractService : IContractService
    {
        public Task<List<ContractListDto>> GetAllAsync() => Task.FromResult(Data.Contracts);

        public Task<ContractDetailDto?> GetByIdAsync(int id)
        {
            return Task.FromResult(Data.ContractDetails.FirstOrDefault(contract => contract.ContractId == id));
        }

        public Task<ContractDetailDto?> GetDetailsAsync(int id) => GetByIdAsync(id);

        public Task<List<ContractListDto>> FilterAsync(int? statusId = null, DateTime? startDate = null, DateTime? endDate = null, int? clientId = null)
        {
            var contracts = Data.Contracts.AsEnumerable();

            if (statusId.HasValue)
                contracts = contracts.Where(contract => contract.ContractStatusId == statusId.Value);

            if (clientId.HasValue)
                contracts = contracts.Where(contract => contract.ClientId == clientId.Value);

            return Task.FromResult(contracts.ToList());
        }

        public Task<ContractDetailDto> CreateAsync(CreateContractDto contract)
        {
            return Task.FromResult(Data.ContractDetails[0]);
        }

        public Task UpdateAsync(UpdateContractDto contract) => Task.CompletedTask;

        public Task UpdateStatusAsync(int id, int contractStatusId) => Task.CompletedTask;

        public Task DeleteAsync(int id) => Task.CompletedTask;
    }

    //..............................................................................//

    public class TestingServiceRequestService : IServiceRequestService
    {
        public Task<List<ServiceRequestListDto>> GetAllAsync(int? contractId = null, int? statusId = null)
        {
            var requests = Data.ServiceRequests.AsEnumerable();

            if (contractId.HasValue)
                requests = requests.Where(request => request.ContractId == contractId.Value);

            if (statusId.HasValue)
                requests = requests.Where(request => request.ServiceRequestStatusId == statusId.Value);

            return Task.FromResult(requests.ToList());
        }

        public Task<ServiceRequestDetailDto?> GetByIdAsync(int id)
        {
            return Task.FromResult(Data.ServiceRequestDetails.FirstOrDefault(request => request.ServiceRequestId == id));
        }

        public Task<ServiceRequestDetailDto?> GetDetailsAsync(int id) => GetByIdAsync(id);

        public Task<List<ServiceRequestListDto>> GetByContractIdAsync(int contractId) => GetAllAsync(contractId);

        public Task<ServiceRequestDetailDto> CreateAsync(CreateServiceRequestDto serviceRequest)
        {
            return Task.FromResult(Data.ServiceRequestDetails[0]);
        }

        public Task UpdateAsync(UpdateServiceRequestDto serviceRequest) => Task.CompletedTask;

        public Task UpdateStatusAsync(int id, int serviceRequestStatusId) => Task.CompletedTask;

        public Task DeleteAsync(int id) => Task.CompletedTask;
    }

    //..............................................................................//

    public class TestingLookupService : ILookupService
    {
        public Task<List<LookupDto>> GetContractStatusesAsync() => Task.FromResult(Data.ContractStatuses);

        public Task<List<LookupDto>> GetServiceRequestStatusesAsync() => Task.FromResult(Data.ServiceRequestStatuses);
    }

    //..............................................................................//

    public class TestingContractDocumentService : IContractDocumentService
    {
        public Task<ContractDocumentDto?> GetByIdAsync(int id)
        {
            return Task.FromResult(Data.Documents.FirstOrDefault(document => document.ContractDocumentId == id));
        }

        public Task<List<ContractDocumentDto>> GetByContractIdAsync(int contractId)
        {
            return Task.FromResult(Data.Documents.Where(document => document.ContractId == contractId).ToList());
        }

        public async Task<ContractDocumentDto?> GetCurrentByContractIdAsync(int contractId)
        {
            var documents = await GetByContractIdAsync(contractId);
            return documents.FirstOrDefault(document => document.IsCurrent);
        }

        public Task<ContractDocumentDto> CreateAsync(ContractDocumentDto document) => Task.FromResult(document);

        public Task UpdateAsync(ContractDocumentDto document) => Task.CompletedTask;

        public Task DeleteAsync(int id) => Task.CompletedTask;

        public Task<ContractDocumentDto> UploadSignedAgreementAsync(int contractId, IFormFile file)
        {
            return Task.FromResult(Data.Documents[0]);
        }

        public Task<DownloadedFile?> DownloadAgreementAsync(int documentId)
        {
            return Task.FromResult<DownloadedFile?>(new DownloadedFile([], "application/pdf", "testing-agreement.pdf"));
        }
    }

    //..............................................................................//

    public class TestingCurrencyExchangeService : ICurrencyExchangeService
    {
        public Task<IReadOnlyDictionary<string, string>> GetSupportedCurrenciesAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyDictionary<string, string> currencies = new Dictionary<string, string>
            {
                ["USD"] = "US Dollar",
                ["EUR"] = "Euro",
                ["ZAR"] = "South African Rand"
            };

            return Task.FromResult(currencies);
        }

        public Task<decimal> GetRateToZarAsync(string baseCurrencyCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(string.Equals(baseCurrencyCode, "ZAR", StringComparison.OrdinalIgnoreCase) ? 1m : 18.5m);
        }

        public async Task<decimal> ConvertToZarAsync(decimal amount, string baseCurrencyCode, CancellationToken cancellationToken = default)
        {
            var rate = await GetRateToZarAsync(baseCurrencyCode, cancellationToken);
            return decimal.Round(amount * rate, 2, MidpointRounding.AwayFromZero);
        }
    }

    //..............................................................................//

    public class TestingAccountService : IAccountService
    {
        public Task<LoginResult> LoginAsync(LoginViewModel vm) => Task.FromResult(LoginResult.SuccessLogin());

        public Task LogoutAsync() => Task.CompletedTask;

        public Task<ProfileViewModel?> GetProfileAsync(System.Security.Claims.ClaimsPrincipal user)
        {
            return Task.FromResult<ProfileViewModel?>(new ProfileViewModel
            {
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@glms.local"
            });
        }

        public Task<AccountResult> UpdateProfileAsync(System.Security.Claims.ClaimsPrincipal user, ProfileViewModel vm) => Task.FromResult(AccountResult.Success());

        public Task<AccountResult> ChangePasswordAsync(System.Security.Claims.ClaimsPrincipal user, ChangePasswordViewModel vm) => Task.FromResult(AccountResult.Success());

        public Task<IReadOnlyList<AdminUserListItemViewModel>> GetUsersAsync()
        {
            IReadOnlyList<AdminUserListItemViewModel> users =
            [
                new AdminUserListItemViewModel { UserId = "1", FirstName = "Admin", LastName = "User", Email = "admin@glms.local", Role = "Admin", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-2) },
                new AdminUserListItemViewModel { UserId = "2", FirstName = "Contracts", LastName = "Manager", Email = "contracts@glms.local", Role = "ContractManager", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-1) },
                new AdminUserListItemViewModel { UserId = "3", FirstName = "Logistics", LastName = "Manager", Email = "logistics@glms.local", Role = "LogisticsManager", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-1) }
            ];

            return Task.FromResult(users);
        }

        public Task<AdminUserEditViewModel?> GetUserForEditAsync(string userId)
        {
            return Task.FromResult<AdminUserEditViewModel?>(new AdminUserEditViewModel
            {
                UserId = userId,
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@glms.local",
                Role = "Admin",
                IsActive = true
            });
        }

        public Task<AccountResult> CreateUserAsync(AdminUserCreateViewModel vm) => Task.FromResult(AccountResult.Success());

        public Task<AccountResult> UpdateUserAsync(AdminUserEditViewModel vm) => Task.FromResult(AccountResult.Success());

        public Task<AccountResult> SetUserActiveAsync(string userId, bool isActive) => Task.FromResult(AccountResult.Success());

        public Task<AccountResult> ResetPasswordAsync(AdminResetPasswordViewModel vm) => Task.FromResult(AccountResult.Success());
    }

    //..............................................................................//

    internal static class Data
    {
        public static readonly List<LookupDto> ContractStatuses =
        [
            new LookupDto { Id = 1, Name = "Draft" },
            new LookupDto { Id = 2, Name = "Active" },
            new LookupDto { Id = 3, Name = "On Hold" },
            new LookupDto { Id = 4, Name = "Expired" }
        ];

        public static readonly List<LookupDto> ServiceRequestStatuses =
        [
            new LookupDto { Id = 1, Name = "Pending" },
            new LookupDto { Id = 2, Name = "In Progress" },
            new LookupDto { Id = 3, Name = "Completed" },
            new LookupDto { Id = 4, Name = "Cancelled" }
        ];

        public static readonly List<ClientListDto> Clients =
        [
            new ClientListDto { ClientId = 1, CompanyName = "CapeRoute Imports", Email = "operations@caperoute.co.za", Region = "Africa", Country = "South Africa", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-5), UpdatedAt = DateTime.UtcNow },
            new ClientListDto { ClientId = 2, CompanyName = "Nova Global Freight", Email = "contracts@novaglobal.com", Region = "North America", Country = "United States", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-4), UpdatedAt = DateTime.UtcNow },
            new ClientListDto { ClientId = 3, CompanyName = "EuroBridge Distribution", Email = "sla@eurobridge.eu", Region = "Europe", Country = "Germany", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-3), UpdatedAt = DateTime.UtcNow }
        ];

        public static readonly List<ContractListDto> Contracts =
        [
            new ContractListDto { ContractId = 1, ClientId = 2, ClientName = "Nova Global Freight", Title = "Global Ocean Freight Agreement", ContractStatusId = 2, ContractStatusName = "Active", ServiceLevel = "Premium", StartDate = DateTime.Today.AddMonths(-8), EndDate = DateTime.Today.AddMonths(16), CreatedAt = DateTime.UtcNow.AddDays(-60), UpdatedAt = DateTime.UtcNow },
            new ContractListDto { ContractId = 2, ClientId = 1, ClientName = "CapeRoute Imports", Title = "Express Air Freight SLA", ContractStatusId = 2, ContractStatusName = "Active", ServiceLevel = "Express", StartDate = DateTime.Today.AddMonths(-3), EndDate = DateTime.Today.AddMonths(9), CreatedAt = DateTime.UtcNow.AddDays(-30), UpdatedAt = DateTime.UtcNow },
            new ContractListDto { ContractId = 3, ClientId = 3, ClientName = "EuroBridge Distribution", Title = "Legacy Port Handling Contract", ContractStatusId = 4, ContractStatusName = "Expired", ServiceLevel = "Economy", StartDate = DateTime.Today.AddMonths(-18), EndDate = DateTime.Today.AddMonths(-1), CreatedAt = DateTime.UtcNow.AddDays(-20), UpdatedAt = DateTime.UtcNow }
        ];

        public static readonly List<ContractDocumentDto> Documents =
        [
            new ContractDocumentDto { ContractDocumentId = 1, ContractId = 1, DocumentType = "Signed Agreement", OriginalFileName = "global-ocean-freight-agreement.pdf", StoredFileName = "global-ocean-freight-agreement.pdf", FilePath = "uploads/signed-agreements/global-ocean-freight-agreement.pdf", ContentType = "application/pdf", FileSizeBytes = 24000, UploadedByUserId = "testing-admin", UploadedByEmail = "admin@glms.local", UploadedAt = DateTime.UtcNow.AddDays(-10), IsCurrent = true }
        ];

        public static readonly List<ServiceRequestListDto> ServiceRequests =
        [
            new ServiceRequestListDto { ServiceRequestId = 101, ContractId = 1, ContractTitle = "Global Ocean Freight Agreement", ClientId = 2, ClientName = "Nova Global Freight", RequestedByUserId = "testing-admin", Description = "Arrange Durban to Rotterdam shipment", AmountOriginal = 12500m, OriginalCurrencyCode = "USD", ExchangeRateToZAR = 18.5m, AmountZAR = 231250m, ServiceRequestStatusId = 2, ServiceRequestStatusName = "In Progress", RequestedAt = DateTime.UtcNow.AddDays(-3), UpdatedAt = DateTime.UtcNow },
            new ServiceRequestListDto { ServiceRequestId = 102, ContractId = 2, ContractTitle = "Express Air Freight SLA", ClientId = 1, ClientName = "CapeRoute Imports", RequestedByUserId = "testing-admin", Description = "Book urgent electronics shipment", AmountOriginal = 7200m, OriginalCurrencyCode = "USD", ExchangeRateToZAR = 18.5m, AmountZAR = 133200m, ServiceRequestStatusId = 1, ServiceRequestStatusName = "Pending", RequestedAt = DateTime.UtcNow.AddDays(-1), UpdatedAt = DateTime.UtcNow }
        ];

        public static readonly List<ClientDetailDto> ClientDetails = Clients
            .Select(client => new ClientDetailDto
            {
                ClientId = client.ClientId,
                CompanyName = client.CompanyName,
                Email = client.Email,
                Phone = client.Phone,
                Region = client.Region,
                Country = client.Country,
                IsActive = client.IsActive,
                CreatedAt = client.CreatedAt,
                UpdatedAt = client.UpdatedAt,
                Contracts = Contracts.Where(contract => contract.ClientId == client.ClientId).ToList()
            })
            .ToList();

        public static readonly List<ContractDetailDto> ContractDetails = Contracts
            .Select(contract => new ContractDetailDto
            {
                ContractId = contract.ContractId,
                ClientId = contract.ClientId,
                ClientName = contract.ClientName,
                ClientEmail = Clients.First(client => client.ClientId == contract.ClientId).Email,
                Title = contract.Title,
                ContractStatusId = contract.ContractStatusId,
                ContractStatusName = contract.ContractStatusName,
                ServiceLevel = contract.ServiceLevel,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                CreatedAt = contract.CreatedAt,
                UpdatedAt = contract.UpdatedAt,
                CreatedByUserId = "testing-admin",
                Notes = "Rendered from test-only MVC sample data.",
                Documents = Documents.Where(document => document.ContractId == contract.ContractId).ToList(),
                ServiceRequests = ServiceRequests.Where(request => request.ContractId == contract.ContractId).ToList()
            })
            .ToList();

        public static readonly List<ServiceRequestDetailDto> ServiceRequestDetails = ServiceRequests
            .Select(request => new ServiceRequestDetailDto
            {
                ServiceRequestId = request.ServiceRequestId,
                ContractId = request.ContractId,
                ContractTitle = request.ContractTitle,
                ClientId = request.ClientId,
                ClientName = request.ClientName,
                RequestedByUserId = request.RequestedByUserId,
                RequestedByEmail = "admin@glms.local",
                Description = request.Description,
                AmountOriginal = request.AmountOriginal,
                OriginalCurrencyCode = request.OriginalCurrencyCode,
                ExchangeRateToZAR = request.ExchangeRateToZAR,
                AmountZAR = request.AmountZAR,
                ServiceRequestStatusId = request.ServiceRequestStatusId,
                ServiceRequestStatusName = request.ServiceRequestStatusName,
                RequestedAt = request.RequestedAt,
                UpdatedAt = request.UpdatedAt
            })
            .ToList();
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
