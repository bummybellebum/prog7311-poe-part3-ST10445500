using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

//ST10445500 - PROG7311 - GLMS POE
//ContractDocumentUploadViewModel

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.ViewModels.Contracts
{
    public class ContractDocumentUploadViewModel
    {
        [Required]
        public int ContractId { get; set; }

        [Required]
        [Display(Name = "Signed Agreement PDF")]
        public IFormFile? File { get; set; }
    }
}

//..........................................o0oEND OF FILEo0o..................................................//