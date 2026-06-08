using GLMS.Web.ApiClients.Models;

//ST10445500 - PROG7311 - GLMS POE
//ContractsApiClient

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.ApiClients
{
	public interface IContractsApiClient
	{
		Task<ApiClientResult<List<ContractListDto>>> GetAllAsync();
		Task<ApiClientResult<List<ContractListDto>>> FilterAsync(int? statusId = null, DateTime? startDate = null, DateTime? endDate = null, int? clientId = null);
		Task<ApiClientResult<ContractDetailDto>> GetByIdAsync(int id);
		Task<ApiClientResult<ContractDetailDto>> GetDetailsAsync(int id);
		Task<ApiClientResult<ContractDetailDto>> CreateAsync(CreateContractDto contract);
		Task<ApiClientResult> UpdateAsync(UpdateContractDto contract);
		Task<ApiClientResult> UpdateStatusAsync(int id, int contractStatusId);
		Task<ApiClientResult> DeleteAsync(int id);
		Task<ApiClientResult<ContractDocumentDto>> GetDocumentByIdAsync(int id);
		Task<ApiClientResult<List<ContractDocumentDto>>> GetDocumentsByContractIdAsync(int contractId);
		Task<ApiClientResult<ContractDocumentDto>> UploadSignedAgreementAsync(int contractId, IFormFile file);
		Task<ApiClientResult<DownloadedFile>> DownloadAgreementAsync(int documentId);
	}

	//..............................................................................//

	public class ContractsApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : ApiClientBase(httpClient, httpContextAccessor), IContractsApiClient
	{

		//..............................................................................//

		public Task<ApiClientResult<List<ContractListDto>>> GetAllAsync()
		{
			return FilterAsync();
		}

		//..............................................................................//

		public async Task<ApiClientResult<List<ContractListDto>>> FilterAsync(
			int? statusId = null,
			DateTime? startDate = null,
			DateTime? endDate = null,
			int? clientId = null)
		{
			var query = new List<string>();

			if (statusId.HasValue)
				query.Add($"statusId={statusId.Value}");

			if (startDate.HasValue)
				query.Add($"startDate={Uri.EscapeDataString(startDate.Value.ToString("O"))}");

			if (endDate.HasValue)
				query.Add($"endDate={Uri.EscapeDataString(endDate.Value.ToString("O"))}");

			if (clientId.HasValue)
				query.Add($"clientId={clientId.Value}");

			var endpoint = query.Count == 0 ? "api/contracts" : $"api/contracts?{string.Join("&", query)}";
			var result = await GetAsync<List<ContractListDto>>(endpoint);
			if (result.IsSuccess)
			{
				result.Data ??= new List<ContractListDto>();
			}

			return result;
		}

		//..............................................................................//

		public Task<ApiClientResult<ContractDetailDto>> GetByIdAsync(int id)
		{
			return GetAsync<ContractDetailDto>($"api/contracts/{id}");
		}

		//..............................................................................//

		public Task<ApiClientResult<ContractDetailDto>> GetDetailsAsync(int id)
		{
			return GetByIdAsync(id);
		}

		//..............................................................................//

		public Task<ApiClientResult<ContractDetailDto>> CreateAsync(CreateContractDto contract)
		{
			return PostAsync<CreateContractDto, ContractDetailDto>("api/contracts", contract);
		}

		//..............................................................................//

		public Task<ApiClientResult> UpdateAsync(UpdateContractDto contract)
		{
			return PutAsync($"api/contracts/{contract.ContractId}", contract);
		}

		//..............................................................................//

		public Task<ApiClientResult> UpdateStatusAsync(int id, int contractStatusId)
		{
			return PatchAsync($"api/contracts/{id}/status", new UpdateContractStatusDto { ContractStatusId = contractStatusId });
		}

		//..............................................................................//

		public Task<ApiClientResult> DeleteAsync(int id)
		{
			return DeleteAsync($"api/contracts/{id}");
		}

		//..............................................................................//

		public Task<ApiClientResult<ContractDocumentDto>> GetDocumentByIdAsync(int id)
		{
			return GetAsync<ContractDocumentDto>($"api/contracts/documents/{id}");
		}

		//..............................................................................//

		public async Task<ApiClientResult<List<ContractDocumentDto>>> GetDocumentsByContractIdAsync(int contractId)
		{
			var result = await GetAsync<List<ContractDocumentDto>>($"api/contracts/{contractId}/documents");
			if (result.IsSuccess)
			{
				result.Data ??= new List<ContractDocumentDto>();
			}

			return result;
		}

		//..............................................................................//

		public Task<ApiClientResult<ContractDocumentDto>> UploadSignedAgreementAsync(int contractId, IFormFile file)
		{
			return PostMultipartFileAsync<ContractDocumentDto>($"api/contracts/{contractId}/signed-agreement", "file", file);
		}

		//..............................................................................//

		public Task<ApiClientResult<DownloadedFile>> DownloadAgreementAsync(int documentId)
		{
			return DownloadFileAsync($"api/contracts/documents/{documentId}/download", "signed-agreement.pdf", "application/pdf");
		}

		//..............................................................................//
	}

	public record DownloadedFile(byte[] Bytes, string ContentType, string FileName);
}

//.....................................o0oEND OF FILEo0o..........................................//
