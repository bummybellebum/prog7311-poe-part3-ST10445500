using GLMS.Web.ApiClients.Models;
using System.Text.Json.Serialization;

//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestsApiClient

//.....................................o0oSTART OF FILEo0o........................................//

// The MVC frontend uses this API client to call the backend with HttpClient.

namespace GLMS.Web.ApiClients
{
	public interface IServiceRequestsApiClient
	{
		Task<ApiClientResult<List<ServiceRequestListDto>>> GetAllAsync(int? contractId = null, int? statusId = null);
		Task<ApiClientResult<ServiceRequestDetailDto>> GetByIdAsync(int id);
		Task<ApiClientResult<ServiceRequestDetailDto>> GetDetailsAsync(int id);
		Task<ApiClientResult<List<ServiceRequestListDto>>> GetByContractIdAsync(int contractId);
		Task<ApiClientResult<ServiceRequestDetailDto>> CreateAsync(CreateServiceRequestDto serviceRequest);
		Task<ApiClientResult> UpdateAsync(UpdateServiceRequestDto serviceRequest);
		Task<ApiClientResult> UpdateStatusAsync(int id, int serviceRequestStatusId);
		Task<ApiClientResult> DeleteAsync(int id);
		Task<ApiClientResult<Dictionary<string, string>>> GetSupportedCurrenciesAsync();
		Task<ApiClientResult<decimal>> GetRateToZarAsync(string baseCurrencyCode);
	}

	//..............................................................................//

	public class ServiceRequestsApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : ApiClientBase(httpClient, httpContextAccessor), IServiceRequestsApiClient
	{

		//..............................................................................//

		public async Task<ApiClientResult<List<ServiceRequestListDto>>> GetAllAsync(int? contractId = null, int? statusId = null)
		{
			var query = new List<string>();

			if (contractId.HasValue)
				query.Add($"contractId={contractId.Value}");

			if (statusId.HasValue)
				query.Add($"statusId={statusId.Value}");

			var endpoint = query.Count == 0 ? "api/service-requests" : $"api/service-requests?{string.Join("&", query)}";
			var result = await GetAsync<List<ServiceRequestListDto>>(endpoint);
			if (result.IsSuccess)
			{
				result.Data ??= new List<ServiceRequestListDto>();
			}

			return result;
		}

		//..............................................................................//

		public Task<ApiClientResult<ServiceRequestDetailDto>> GetByIdAsync(int id)
		{
			return GetAsync<ServiceRequestDetailDto>($"api/service-requests/{id}");
		}

		//..............................................................................//

		public Task<ApiClientResult<ServiceRequestDetailDto>> GetDetailsAsync(int id)
		{
			return GetByIdAsync(id);
		}

		//..............................................................................//

		public Task<ApiClientResult<List<ServiceRequestListDto>>> GetByContractIdAsync(int contractId)
		{
			return GetAllAsync(contractId);
		}

		//..............................................................................//

		public Task<ApiClientResult<ServiceRequestDetailDto>> CreateAsync(CreateServiceRequestDto serviceRequest)
		{
			return PostAsync<CreateServiceRequestDto, ServiceRequestDetailDto>("api/service-requests", serviceRequest);
		}

		//..............................................................................//

		public Task<ApiClientResult> UpdateAsync(UpdateServiceRequestDto serviceRequest)
		{
			return PutAsync($"api/service-requests/{serviceRequest.ServiceRequestId}", serviceRequest);
		}

		//..............................................................................//

		public Task<ApiClientResult> UpdateStatusAsync(int id, int serviceRequestStatusId)
		{
			return PatchAsync($"api/service-requests/{id}/status", new UpdateServiceRequestStatusDto { ServiceRequestStatusId = serviceRequestStatusId });
		}

		//..............................................................................//

		public Task<ApiClientResult> DeleteAsync(int id)
		{
			return DeleteAsync($"api/service-requests/{id}");
		}

		//..............................................................................//

		public async Task<ApiClientResult<Dictionary<string, string>>> GetSupportedCurrenciesAsync()
		{
			var result = await GetAsync<Dictionary<string, string>>("api/service-requests/currencies");
			if (result.IsSuccess)
			{
				result.Data ??= new Dictionary<string, string>();
			}

			return result;
		}

		//..............................................................................//

		public async Task<ApiClientResult<decimal>> GetRateToZarAsync(string baseCurrencyCode)
		{
			var result = await GetAsync<ExchangeRateResponse>($"api/service-requests/exchange-rate?currencyCode={Uri.EscapeDataString(baseCurrencyCode)}");
			return result.IsSuccess
				? ApiClientResult<decimal>.Success(result.Data?.Rate ?? 0m)
				: ApiClientResult<decimal>.Failure(result.ErrorMessage ?? "Unable to load exchange rate.", result.StatusCode);
		}

		//..............................................................................//

		private class ExchangeRateResponse
		{
			[JsonPropertyName("rate")]
			public decimal Rate { get; set; }
		}
	}
}

//.....................................o0oEND OF FILEo0o..........................................//
