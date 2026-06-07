using GLMS.Web.ViewModels.Api;

//ST10445500 - PROG7311 - GLMS POE
//ContractDocumentService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services
{
    //manages Contract Documents by calling the GLMS API
    public interface IContractDocumentService
    {
        Task<ContractDocumentDto?> GetByIdAsync(int id);
        Task<List<ContractDocumentDto>> GetByContractIdAsync(int contractId);
        Task<ContractDocumentDto?> GetCurrentByContractIdAsync(int contractId);
        Task<ContractDocumentDto> CreateAsync(ContractDocumentDto document);
        Task UpdateAsync(ContractDocumentDto document);
        Task DeleteAsync(int id);
        Task<ContractDocumentDto> UploadSignedAgreementAsync(int contractId, IFormFile file);
        Task<DownloadedFile?> DownloadAgreementAsync(int documentId);
    }

    //..............................................................................//

    public class ContractDocumentService : ApiClientService, IContractDocumentService
    {
        public ContractDocumentService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        //..............................................................................//

        public Task<ContractDocumentDto?> GetByIdAsync(int id)
        {
            return GetAsync<ContractDocumentDto>($"api/contracts/documents/{id}");
        }

        //..............................................................................//

        public async Task<List<ContractDocumentDto>> GetByContractIdAsync(int contractId)
        {
            return await GetAsync<List<ContractDocumentDto>>($"api/contracts/{contractId}/documents") ?? new List<ContractDocumentDto>();
        }

        //..............................................................................//

        public async Task<ContractDocumentDto?> GetCurrentByContractIdAsync(int contractId)
        {
            var documents = await GetByContractIdAsync(contractId);
            return documents.FirstOrDefault(d => d.IsCurrent);
        }

        //..............................................................................//

        public Task<ContractDocumentDto> CreateAsync(ContractDocumentDto document)
        {
            return PostAsync<ContractDocumentDto>($"api/contracts/{document.ContractId}/documents", document);
        }

        //..............................................................................//

        public Task UpdateAsync(ContractDocumentDto document)
        {
            return PutAsync($"api/contracts/documents/{document.ContractDocumentId}", document);
        }

        //..............................................................................//

        public Task DeleteAsync(int id)
        {
            return DeleteAsync($"api/contracts/documents/{id}");
        }

        //..............................................................................//

        public async Task<ContractDocumentDto> UploadSignedAgreementAsync(int contractId, IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType ?? "application/pdf");
            content.Add(fileContent, "file", file.FileName);

            using var request = CreateRequest(HttpMethod.Post, $"api/contracts/{contractId}/signed-agreement");
            request.Content = content;
            using var response = await SendAsync(request);
            await EnsureSuccessAsync(response);

            return (await ReadAsync<ContractDocumentDto>(response))!;
        }

        //..............................................................................//

        public async Task<DownloadedFile?> DownloadAgreementAsync(int documentId)
        {
            using var request = CreateRequest(HttpMethod.Get, $"api/contracts/documents/{documentId}/download");
            using var response = await SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            await EnsureSuccessAsync(response);

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? "signed-agreement.pdf";
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/pdf";

            return new DownloadedFile(bytes, contentType, fileName);
        }

        //..............................................................................//
    }

    public record DownloadedFile(byte[] Bytes, string ContentType, string FileName);
}

//......................................o0oEND OF FILEo0o.........................................//
