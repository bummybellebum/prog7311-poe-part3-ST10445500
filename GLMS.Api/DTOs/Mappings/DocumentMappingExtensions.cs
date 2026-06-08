using GLMS.Api.DTOs.Documents;
using GLMS.Api.Models;

//ST10445500 - PROG7311 - GLMS POE
//DocumentMappingExtensions

//.....................................o0oSTART OF FILEo0o........................................//

// Mapping keeps the API DTO shapes separate from the EF database models.

namespace GLMS.Api.DTOs.Mappings
{
    public static class DocumentMappingExtensions
    {
        public static ContractDocumentDto ToDto(this ContractDocument document)
        {
            return new ContractDocumentDto
            {
                ContractDocumentId = document.ContractDocumentId,
                ContractId = document.ContractId,
                DocumentType = document.DocumentType,
                OriginalFileName = document.OriginalFileName,
                StoredFileName = document.StoredFileName,
                FilePath = document.FilePath,
                ContentType = document.ContentType,
                FileSizeBytes = document.FileSizeBytes,
                UploadedByUserId = document.UploadedByUserId,
                UploadedByEmail = document.UploadedByUser?.Email,
                UploadedAt = document.UploadedAt,
                IsCurrent = document.IsCurrent
            };
        }

        public static ContractDocument ToEntity(this CreateContractDocumentDto dto)
        {
            return new ContractDocument
            {
                ContractId = dto.ContractId,
                DocumentType = dto.DocumentType,
                OriginalFileName = dto.OriginalFileName,
                StoredFileName = dto.StoredFileName,
                FilePath = dto.FilePath,
                ContentType = dto.ContentType ?? string.Empty,
                FileSizeBytes = dto.FileSizeBytes,
                UploadedByUserId = dto.UploadedByUserId,
                IsCurrent = dto.IsCurrent
            };
        }

        public static ContractDocument ApplyTo(this UpdateContractDocumentDto dto, ContractDocument document)
        {
            document.ContractDocumentId = dto.ContractDocumentId;
            document.ContractId = dto.ContractId;
            document.DocumentType = dto.DocumentType;
            document.OriginalFileName = dto.OriginalFileName;
            document.StoredFileName = dto.StoredFileName;
            document.FilePath = dto.FilePath;
            document.ContentType = dto.ContentType ?? string.Empty;
            document.FileSizeBytes = dto.FileSizeBytes;
            document.UploadedByUserId = dto.UploadedByUserId;
            document.IsCurrent = dto.IsCurrent;

            if (dto.UploadedAt != default)
            {
                document.UploadedAt = dto.UploadedAt;
            }

            return document;
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
