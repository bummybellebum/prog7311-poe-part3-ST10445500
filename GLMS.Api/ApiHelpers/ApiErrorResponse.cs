//ST10445500 - PROG7311 - GLMS POE
//ApiErrorResponse

//.....................................o0oSTART OF FILEo0o........................................//

// This helper keeps common API response details in one place.



namespace GLMS.Api.ApiHelpers
{
    public class ApiErrorResponse
    {
        public ApiErrorResponse(IEnumerable<string> errors)
        {
            Errors = errors
                .Where(error => !string.IsNullOrWhiteSpace(error))
                .ToArray();
        }

        public ApiErrorResponse(params string[] errors)
            : this((IEnumerable<string>)errors)
        {
        }

        public IReadOnlyList<string> Errors { get; }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
