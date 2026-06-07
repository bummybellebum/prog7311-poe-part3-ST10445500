namespace GLMS.Api.Results
{
    public class AuthServiceResult<T>
    {
        private AuthServiceResult(bool succeeded, bool isUnauthorized, T? value, IReadOnlyList<string> errors)
        {
            Succeeded = succeeded;
            IsUnauthorized = isUnauthorized;
            Value = value;
            Errors = errors;
        }

        public bool Succeeded { get; }
        public bool IsUnauthorized { get; }
        public T? Value { get; }
        public IReadOnlyList<string> Errors { get; }

        public static AuthServiceResult<T> Success(T value) => new(true, false, value, []);

        public static AuthServiceResult<T> Failed(params string[] errors) => new(false, false, default, errors);

        public static AuthServiceResult<T> Failed(IEnumerable<string> errors) => new(false, false, default, errors.ToList());

        public static AuthServiceResult<T> Unauthorized(params string[] errors) => new(false, true, default, errors);
    }
}
