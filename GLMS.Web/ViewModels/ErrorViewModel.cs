//ST10445500 - PROG7311 - GLMS POE
//ErrorViewModel

//.....................................o0oSTART OF FILEo0o........................................//

// The view model contains only the data needed by the MVC screen.



namespace GLMS.Web.ViewModels
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
