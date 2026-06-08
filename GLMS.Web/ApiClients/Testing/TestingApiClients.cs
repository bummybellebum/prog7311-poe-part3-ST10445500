using GLMS.Web.ApiModels;

//ST10445500 - PROG7311 - GLMS POE
//TestingApiClients

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.ApiClients.Testing
{
    public class TestingAuthApiClient : IAuthApiClient
    {
        public Task<ApiClientResult<AuthResponseDto>> LoginAsync(LoginRequestDto request)
        {
            var response = new AuthResponseDto
            {
                UserId = "testing-admin",
                Email = request.Email,
                Token = "testing-token",
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1).UtcDateTime,
                FirstName = "Admin",
                LastName = "User",
                Roles = ["Admin"]
            };

            return Task.FromResult(ApiClientResult<AuthResponseDto>.Success(response));
        }

        public Task<ApiClientResult<AuthResponseDto>> GetCurrentAccountAsync()
        {
            return Task.FromResult(ApiClientResult<AuthResponseDto>.Success(new AuthResponseDto
            {
                UserId = "testing-admin",
                Email = "admin@glms.local",
                FirstName = "Admin",
                LastName = "User",
                Roles = ["Admin"]
            }));
        }

        public Task<ApiClientResult<AuthResponseDto>> UpdateCurrentAccountAsync(UpdateAccountProfileDto request)
        {
            return Task.FromResult(ApiClientResult<AuthResponseDto>.Success(new AuthResponseDto
            {
                UserId = "testing-admin",
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Roles = ["Admin"]
            }));
        }

        public Task<ApiClientResult> ChangePasswordAsync(ChangePasswordRequestDto request) => Task.FromResult(ApiClientResult.Success());

        public Task<ApiClientResult<List<UserListDto>>> GetUsersAsync()
        {
            var users = new List<UserListDto>
            {
                new() { UserId = "1", FirstName = "Admin", LastName = "User", Email = "admin@glms.local", Role = "Admin", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-2) },
                new() { UserId = "2", FirstName = "Contracts", LastName = "Manager", Email = "contracts@glms.local", Role = "ContractManager", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-1) },
                new() { UserId = "3", FirstName = "Logistics", LastName = "Manager", Email = "logistics@glms.local", Role = "LogisticsManager", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-1) }
            };

            return Task.FromResult(ApiClientResult<List<UserListDto>>.Success(users));
        }

        public Task<ApiClientResult<UserDetailDto>> GetUserAsync(string userId)
        {
            return Task.FromResult(ApiClientResult<UserDetailDto>.Success(new UserDetailDto
            {
                UserId = userId,
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@glms.local",
                Role = "Admin",
                IsActive = true
            }));
        }

        public Task<ApiClientResult<UserDetailDto>> CreateUserAsync(CreateUserDto request)
        {
            return Task.FromResult(ApiClientResult<UserDetailDto>.Success(new UserDetailDto
            {
                UserId = "created-user",
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Role = request.Role,
                IsActive = request.IsActive
            }));
        }

        public Task<ApiClientResult<UserDetailDto>> UpdateUserAsync(UpdateUserDto request)
        {
            return Task.FromResult(ApiClientResult<UserDetailDto>.Success(new UserDetailDto
            {
                UserId = request.UserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Role = request.Role,
                IsActive = request.IsActive
            }));
        }

        public Task<ApiClientResult<UserDetailDto>> SetUserActiveAsync(string userId, bool isActive)
        {
            return Task.FromResult(ApiClientResult<UserDetailDto>.Success(new UserDetailDto
            {
                UserId = userId,
                Email = "admin@glms.local",
                Role = "Admin",
                IsActive = isActive
            }));
        }

        public Task<ApiClientResult> ResetPasswordAsync(ResetUserPasswordDto request) => Task.FromResult(ApiClientResult.Success());
    }

    //..............................................................................//

    public class TestingClientsApiClient : IClientsApiClient
    {
        public Task<ApiClientResult<List<ClientListDto>>> GetAllAsync(string? search = null)
        {
            var clients = Data.Clients
                .Where(client => string.IsNullOrWhiteSpace(search)
                    || client.CompanyName.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || client.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Task.FromResult(ApiClientResult<List<ClientListDto>>.Success(clients));
        }

        public Task<ApiClientResult<ClientDetailDto>> GetByIdAsync(int id)
        {
            var client = Data.ClientDetails.FirstOrDefault(client => client.ClientId == id);
            return Task.FromResult(client == null
                ? ApiClientResult<ClientDetailDto>.Failure("The requested record could not be found.", StatusCodes.Status404NotFound)
                : ApiClientResult<ClientDetailDto>.Success(client));
        }

        public Task<ApiClientResult<ClientDetailDto>> GetWithContractsAsync(int id) => GetByIdAsync(id);

        public Task<ApiClientResult<ClientDetailDto>> CreateAsync(CreateClientDto client)
        {
            return Task.FromResult(ApiClientResult<ClientDetailDto>.Success(Data.ClientDetails[0]));
        }

        public Task<ApiClientResult> UpdateAsync(UpdateClientDto client) => Task.FromResult(ApiClientResult.Success());

        public Task<ApiClientResult> DeleteAsync(int id) => Task.FromResult(ApiClientResult.Success());
    }

    //..............................................................................//

    public class TestingContractsApiClient : IContractsApiClient
    {
        public Task<ApiClientResult<List<ContractListDto>>> GetAllAsync() => Task.FromResult(ApiClientResult<List<ContractListDto>>.Success(Data.Contracts));

        public Task<ApiClientResult<List<ContractListDto>>> FilterAsync(int? statusId = null, DateTime? startDate = null, DateTime? endDate = null, int? clientId = null)
        {
            var contracts = Data.Contracts.AsEnumerable();

            if (statusId.HasValue)
                contracts = contracts.Where(contract => contract.ContractStatusId == statusId.Value);

            if (clientId.HasValue)
                contracts = contracts.Where(contract => contract.ClientId == clientId.Value);

            return Task.FromResult(ApiClientResult<List<ContractListDto>>.Success(contracts.ToList()));
        }

        public Task<ApiClientResult<ContractDetailDto>> GetByIdAsync(int id)
        {
            var contract = Data.ContractDetails.FirstOrDefault(contract => contract.ContractId == id);
            return Task.FromResult(contract == null
                ? ApiClientResult<ContractDetailDto>.Failure("The requested record could not be found.", StatusCodes.Status404NotFound)
                : ApiClientResult<ContractDetailDto>.Success(contract));
        }

        public Task<ApiClientResult<ContractDetailDto>> GetDetailsAsync(int id) => GetByIdAsync(id);

        public Task<ApiClientResult<ContractDetailDto>> CreateAsync(CreateContractDto contract)
        {
            return Task.FromResult(ApiClientResult<ContractDetailDto>.Success(Data.ContractDetails[0]));
        }

        public Task<ApiClientResult> UpdateAsync(UpdateContractDto contract) => Task.FromResult(ApiClientResult.Success());

        public Task<ApiClientResult> UpdateStatusAsync(int id, int contractStatusId) => Task.FromResult(ApiClientResult.Success());

        public Task<ApiClientResult> DeleteAsync(int id) => Task.FromResult(ApiClientResult.Success());

        public Task<ApiClientResult<ContractDocumentDto>> GetDocumentByIdAsync(int id)
        {
            var document = Data.Documents.FirstOrDefault(document => document.ContractDocumentId == id);
            return Task.FromResult(document == null
                ? ApiClientResult<ContractDocumentDto>.Failure("The requested record could not be found.", StatusCodes.Status404NotFound)
                : ApiClientResult<ContractDocumentDto>.Success(document));
        }

        public Task<ApiClientResult<List<ContractDocumentDto>>> GetDocumentsByContractIdAsync(int contractId)
        {
            var documents = Data.Documents.Where(document => document.ContractId == contractId).ToList();
            return Task.FromResult(ApiClientResult<List<ContractDocumentDto>>.Success(documents));
        }

        public Task<ApiClientResult<ContractDocumentDto>> UploadSignedAgreementAsync(int contractId, IFormFile file)
        {
            return Task.FromResult(ApiClientResult<ContractDocumentDto>.Success(Data.Documents[0]));
        }

        public Task<ApiClientResult<DownloadedFile>> DownloadAgreementAsync(int documentId)
        {
            return Task.FromResult(ApiClientResult<DownloadedFile>.Success(new DownloadedFile([], "application/pdf", "testing-agreement.pdf")));
        }
    }

    //..............................................................................//

    public class TestingServiceRequestsApiClient : IServiceRequestsApiClient
    {
        public Task<ApiClientResult<List<ServiceRequestListDto>>> GetAllAsync(int? contractId = null, int? statusId = null)
        {
            var requests = Data.ServiceRequests.AsEnumerable();

            if (contractId.HasValue)
                requests = requests.Where(request => request.ContractId == contractId.Value);

            if (statusId.HasValue)
                requests = requests.Where(request => request.ServiceRequestStatusId == statusId.Value);

            return Task.FromResult(ApiClientResult<List<ServiceRequestListDto>>.Success(requests.ToList()));
        }

        public Task<ApiClientResult<ServiceRequestDetailDto>> GetByIdAsync(int id)
        {
            var request = Data.ServiceRequestDetails.FirstOrDefault(request => request.ServiceRequestId == id);
            return Task.FromResult(request == null
                ? ApiClientResult<ServiceRequestDetailDto>.Failure("The requested record could not be found.", StatusCodes.Status404NotFound)
                : ApiClientResult<ServiceRequestDetailDto>.Success(request));
        }

        public Task<ApiClientResult<ServiceRequestDetailDto>> GetDetailsAsync(int id) => GetByIdAsync(id);

        public Task<ApiClientResult<List<ServiceRequestListDto>>> GetByContractIdAsync(int contractId) => GetAllAsync(contractId);

        public Task<ApiClientResult<ServiceRequestDetailDto>> CreateAsync(CreateServiceRequestDto serviceRequest)
        {
            return Task.FromResult(ApiClientResult<ServiceRequestDetailDto>.Success(Data.ServiceRequestDetails[0]));
        }

        public Task<ApiClientResult> UpdateAsync(UpdateServiceRequestDto serviceRequest) => Task.FromResult(ApiClientResult.Success());

        public Task<ApiClientResult> UpdateStatusAsync(int id, int serviceRequestStatusId) => Task.FromResult(ApiClientResult.Success());

        public Task<ApiClientResult> DeleteAsync(int id) => Task.FromResult(ApiClientResult.Success());

        public Task<ApiClientResult<Dictionary<string, string>>> GetSupportedCurrenciesAsync()
        {
            var currencies = new Dictionary<string, string>
            {
                ["USD"] = "US Dollar",
                ["EUR"] = "Euro",
                ["ZAR"] = "South African Rand"
            };

            return Task.FromResult(ApiClientResult<Dictionary<string, string>>.Success(currencies));
        }

        public Task<ApiClientResult<decimal>> GetRateToZarAsync(string baseCurrencyCode)
        {
            var rate = string.Equals(baseCurrencyCode, "ZAR", StringComparison.OrdinalIgnoreCase) ? 1m : 18.5m;
            return Task.FromResult(ApiClientResult<decimal>.Success(rate));
        }
    }

    //..............................................................................//

    public class TestingLookupsApiClient : ILookupsApiClient
    {
        public Task<ApiClientResult<List<LookupDto>>> GetContractStatusesAsync()
        {
            return Task.FromResult(ApiClientResult<List<LookupDto>>.Success(Data.ContractStatuses));
        }

        public Task<ApiClientResult<List<LookupDto>>> GetServiceRequestStatusesAsync()
        {
            return Task.FromResult(ApiClientResult<List<LookupDto>>.Success(Data.ServiceRequestStatuses));
        }
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
