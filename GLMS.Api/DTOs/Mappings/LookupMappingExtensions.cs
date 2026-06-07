using GLMS.Api.DTOs.Lookups;
using GLMS.Api.Models;

namespace GLMS.Api.DTOs.Mappings
{
    public static class LookupMappingExtensions
    {
        public static LookupDto ToLookupDto(this ContractStatus status)
        {
            return new LookupDto
            {
                Id = status.ContractStatusId,
                Name = status.StatusName
            };
        }

        public static LookupDto ToLookupDto(this ServiceRequestStatus status)
        {
            return new LookupDto
            {
                Id = status.ServiceRequestStatusId,
                Name = status.StatusName
            };
        }

        public static LookupDto ToLookupDto(this Client client)
        {
            return new LookupDto
            {
                Id = client.ClientId,
                Name = client.CompanyName
            };
        }

        public static LookupDto ToLookupDto(this Contract contract)
        {
            return new LookupDto
            {
                Id = contract.ContractId,
                Name = contract.Title
            };
        }
    }
}
