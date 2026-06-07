namespace GLMS.Api.Responses
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
