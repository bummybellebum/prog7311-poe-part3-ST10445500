using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

//ST10445500 - PROG7311 - GLMS POE
//ContractDocumentUploadViewModel

//.....................................o0oSTART OF FILEo0o........................................//

// The view model contains only the data needed by the MVC screen.

//ST10445500 - PROG7311 - GLMS POE
//ContractDocumentUploadViewModel

//..........................................o0oSTART OF FILEo0o..................................................//

namespace GLMS.Web.ViewModels.Contracts
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

//.....................................o0oEND OF FILEo0o..........................................//
