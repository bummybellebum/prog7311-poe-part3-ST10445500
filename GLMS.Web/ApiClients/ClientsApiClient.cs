using GLMS.Web.ApiModels;

//ST10445500 - PROG7311 - GLMS POE
//ClientsApiClient

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.ApiClients
{
	public interface IClientsApiClient
	{
		Task<ApiClientResult<List<ClientListDto>>> GetAllAsync(string? search = null);
		Task<ApiClientResult<ClientDetailDto>> GetByIdAsync(int id);
		Task<ApiClientResult<ClientDetailDto>> GetWithContractsAsync(int id);
		Task<ApiClientResult<ClientDetailDto>> CreateAsync(CreateClientDto client);
		Task<ApiClientResult> UpdateAsync(UpdateClientDto client);
		Task<ApiClientResult> DeleteAsync(int id);
	}

	//..............................................................................//

	public class ClientsApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : ApiClientBase(httpClient, httpContextAccessor), IClientsApiClient
	{

		//..............................................................................//

		public async Task<ApiClientResult<List<ClientListDto>>> GetAllAsync(string? search = null)
		{
			var endpoint = string.IsNullOrWhiteSpace(search)
				? "api/clients"
				: $"api/clients?search={Uri.EscapeDataString(search)}";

			var result = await GetAsync<List<ClientListDto>>(endpoint);
			if (result.IsSuccess)
			{
				result.Data ??= new List<ClientListDto>();
			}

			return result;
		}

		//..............................................................................//

		public Task<ApiClientResult<ClientDetailDto>> GetByIdAsync(int id)
		{
			return GetAsync<ClientDetailDto>($"api/clients/{id}");
		}

		//..............................................................................//

		public Task<ApiClientResult<ClientDetailDto>> GetWithContractsAsync(int id)
		{
			return GetByIdAsync(id);
		}

		//..............................................................................//

		public Task<ApiClientResult<ClientDetailDto>> CreateAsync(CreateClientDto client)
		{
			return PostAsync<CreateClientDto, ClientDetailDto>("api/clients", client);
		}

		//..............................................................................//

		public Task<ApiClientResult> UpdateAsync(UpdateClientDto client)
		{
			return PutAsync($"api/clients/{client.ClientId}", client);
		}

		//..............................................................................//

		public Task<ApiClientResult> DeleteAsync(int id)
		{
			return DeleteAsync($"api/clients/{id}");
		}

		//..............................................................................//
	}
}

//.....................................o0oEND OF FILEo0o..........................................//
