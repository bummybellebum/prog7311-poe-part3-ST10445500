using GLMS.Api.DTOs.Contracts;
using GLMS.Api.DTOs.Documents;
using GLMS.Api.DTOs.ServiceRequests;
using GLMS.Api.Models;
using Microsoft.AspNetCore.Http;

namespace GLMS.Tests.Helpers
{
    public static class TestData
    {
        public const string UserId = "user-1";

        public static Client Client(int id = 1, string companyName = "Acme Logistics")
        {
            return new Client
            {
                ClientId = id,
                CompanyName = companyName,
                Email = $"client{id}@glms.local",
                Phone = "0123456789",
                Region = "Gauteng",
                Country = "South Africa",
                IsActive = true
            };
        }

        public static ApplicationUser User(string id = UserId)
        {
            return new ApplicationUser
            {
                Id = id,
                UserName = $"{id}@glms.local",
                Email = $"{id}@glms.local",
                FirstName = "Test",
                LastName = "User",
                IsActive = true
            };
        }

        public static ContractStatus ContractStatus(int id, string name)
        {
            return new ContractStatus
            {
                ContractStatusId = id,
                StatusName = name
            };
        }

        public static ServiceRequestStatus ServiceRequestStatus(int id = 1, string name = "Pending")
        {
            return new ServiceRequestStatus
            {
                ServiceRequestStatusId = id,
                StatusName = name
            };
        }

        public static Contract Contract(
            int id = 1,
            int clientId = 1,
            int statusId = ContractStatusConstants.ActiveId,
            string statusName = ContractStatusConstants.ActiveName,
            DateTime? startDate = null)
        {
            var start = startDate ?? new DateTime(2026, 1, 10);

            return new Contract
            {
                ContractId = id,
                ClientId = clientId,
                Title = $"Contract {id}",
                StartDate = start,
                EndDate = start.AddMonths(6),
                ContractStatusId = statusId,
                ContractStatus = ContractStatus(statusId, statusName),
                CreatedByUserId = UserId,
                ServiceLevel = "Gold",
                Notes = "Test contract",
                CreatedAt = start,
                UpdatedAt = start
            };
        }

        public static ServiceRequest ServiceRequest(
            int id = 1,
            int contractId = 1,
            int statusId = 1,
            DateTime? requestedAt = null)
        {
            return new ServiceRequest
            {
                ServiceRequestId = id,
                ContractId = contractId,
                RequestedByUserId = UserId,
                Description = $"Service request {id}",
                AmountOriginal = 100m,
                OriginalCurrencyCode = "USD",
                ExchangeRateToZAR = 18m,
                AmountZAR = 1800m,
                ServiceRequestStatusId = statusId,
                RequestedAt = requestedAt ?? new DateTime(2026, 1, 11),
                UpdatedAt = requestedAt ?? new DateTime(2026, 1, 11)
            };
        }

        public static ContractDocument Document(int id = 1, int contractId = 1, bool isCurrent = true)
        {
            return new ContractDocument
            {
                ContractDocumentId = id,
                ContractId = contractId,
                DocumentType = "Signed Agreement",
                OriginalFileName = $"agreement-{id}.pdf",
                StoredFileName = $"agreement-{id}.pdf",
                FilePath = $"uploads/agreement-{id}.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = 100,
                UploadedByUserId = UserId,
                UploadedAt = new DateTime(2026, 1, 10).AddDays(id),
                IsCurrent = isCurrent
            };
        }

        public static CreateContractDto CreateContractDto()
        {
            return new CreateContractDto
            {
                ClientId = 1,
                Title = "New Contract",
                StartDate = new DateTime(2026, 3, 1),
                EndDate = new DateTime(2026, 9, 1),
                ContractStatusId = ContractStatusConstants.ActiveId,
                ServiceLevel = "Gold",
                Notes = "Created by test"
            };
        }

        public static CreateServiceRequestDto CreateServiceRequestDto()
        {
            return new CreateServiceRequestDto
            {
                ContractId = 1,
                Description = "Need support",
                AmountOriginal = 100m,
                OriginalCurrencyCode = "usd",
                ServiceRequestStatusId = 1
            };
        }

        public static CreateContractDocumentDto CreateDocumentDto()
        {
            return new CreateContractDocumentDto
            {
                ContractId = 1,
                DocumentType = "Signed Agreement",
                OriginalFileName = "contract.pdf",
                StoredFileName = "contract-1.pdf",
                FilePath = "uploads/contract-1.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = 100,
                UploadedByUserId = UserId,
                IsCurrent = true
            };
        }

        public static IFormFile FormFile(string fileName, string contentType, byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        public static byte[] PdfBytes()
        {
            return "%PDF-1.4\nTest PDF"u8.ToArray();
        }
    }
}
