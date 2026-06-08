using GLMS.Web.ApiClients.Models;

//ST10445500 - PROG7311 - GLMS POE
//LookupsApiClient

//.....................................o0oSTART OF FILEo0o........................................//

// The MVC frontend uses this API client to call the backend with HttpClient.

namespace GLMS.Web.ApiClients
{
	public interface ILookupsApiClient
	{
		Task<ApiClientResult<List<LookupDto>>> GetContractStatusesAsync();
		Task<ApiClientResult<List<LookupDto>>> GetServiceRequestStatusesAsync();
	}

	//..............................................................................//

	public class LookupsApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : ApiClientBase(httpClient, httpContextAccessor), ILookupsApiClient
	{

		//..............................................................................//

		public async Task<ApiClientResult<List<LookupDto>>> GetContractStatusesAsync()
		{
			var result = await GetAsync<List<LookupDto>>("api/lookups/contract-statuses");
			if (result.IsSuccess)
			{
				result.Data ??= new List<LookupDto>();
			}

			return result;
		}

		//..............................................................................//

		public async Task<ApiClientResult<List<LookupDto>>> GetServiceRequestStatusesAsync()
		{
			var result = await GetAsync<List<LookupDto>>("api/lookups/service-request-statuses");
			if (result.IsSuccess)
			{
				result.Data ??= new List<LookupDto>();
			}

			return result;
		}

		//..............................................................................//
	}
}

//.....................................o0oEND OF FILEo0o..........................................//
