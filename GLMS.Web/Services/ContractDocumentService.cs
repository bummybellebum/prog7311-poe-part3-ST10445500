using GLMS.Web.Models;

//ST10445500 - PROG7311 - GLMS POE
//ContractDocumentService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services
{
    //manages Contract Documents by calling the GLMS API
    public interface IContractDocumentService
    {
        Task<ContractDocument?> GetByIdAsync(int id);
        Task<List<ContractDocument>> GetByContractIdAsync(int contractId);
        Task<ContractDocument?> GetCurrentByContractIdAsync(int contractId);
        Task<ContractDocument> CreateAsync(ContractDocument document);
        Task UpdateAsync(ContractDocument document);
        Task DeleteAsync(int id);
        Task<ContractDocument> UploadSignedAgreementAsync(int contractId, IFormFile file);
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

        public Task<ContractDocument?> GetByIdAsync(int id)
        {
            return GetAsync<ContractDocument>($"api/contracts/documents/{id}");
        }

        //..............................................................................//

        public async Task<List<ContractDocument>> GetByContractIdAsync(int contractId)
        {
            return await GetAsync<List<ContractDocument>>($"api/contracts/{contractId}/documents") ?? new List<ContractDocument>();
        }

        //..............................................................................//

        public async Task<ContractDocument?> GetCurrentByContractIdAsync(int contractId)
        {
            var documents = await GetByContractIdAsync(contractId);
            return documents.FirstOrDefault(d => d.IsCurrent);
        }

        //..............................................................................//

        public Task<ContractDocument> CreateAsync(ContractDocument document)
        {
            return PostAsync<ContractDocument>($"api/contracts/{document.ContractId}/documents", document);
        }

        //..............................................................................//

        public Task UpdateAsync(ContractDocument document)
        {
            return PutAsync($"api/contracts/documents/{document.ContractDocumentId}", document);
        }

        //..............................................................................//

        public Task DeleteAsync(int id)
        {
            return DeleteAsync($"api/contracts/documents/{id}");
        }

        //..............................................................................//

        public async Task<ContractDocument> UploadSignedAgreementAsync(int contractId, IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType ?? "application/pdf");
            content.Add(fileContent, "file", file.FileName);

            using var request = CreateRequest(HttpMethod.Post, $"api/contracts/{contractId}/signed-agreement");
            request.Content = content;
            using var response = await HttpClient.SendAsync(request);
            await EnsureSuccessAsync(response);

            return (await ReadAsync<ContractDocument>(response))!;
        }

        //..............................................................................//

        public async Task<DownloadedFile?> DownloadAgreementAsync(int documentId)
        {
            using var request = CreateRequest(HttpMethod.Get, $"api/contracts/documents/{documentId}/download");
            using var response = await HttpClient.SendAsync(request);

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
