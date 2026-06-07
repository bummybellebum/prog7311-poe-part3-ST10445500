//ST10445500 - PROG7311 - GLMS POE
//AccountResult

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Api.Services
{
    public class AccountResult
    {
        protected AccountResult(bool succeeded, bool isNotFound, IReadOnlyList<string> errors)
        {
            Succeeded = succeeded;
            IsNotFound = isNotFound;
            Errors = errors;
        }

        public bool Succeeded { get; }
        public bool IsNotFound { get; }
        public IReadOnlyList<string> Errors { get; }

        public static AccountResult Success() => new(true, false, []);

        public static AccountResult Failed(params string[] errors) => new(false, false, errors);

        public static AccountResult Failed(IEnumerable<string> errors) => new(false, false, errors.ToList());

        public static AccountResult NotFound(params string[] errors) => new(false, true, errors);
    }

    public class AccountResult<T> : AccountResult
    {
        private AccountResult(bool succeeded, bool isNotFound, T? value, IReadOnlyList<string> errors)
            : base(succeeded, isNotFound, errors)
        {
            Value = value;
        }

        public T? Value { get; }

        public static AccountResult<T> Success(T value) => new(true, false, value, []);

        public static new AccountResult<T> Failed(params string[] errors) => new(false, false, default, errors);

        public static new AccountResult<T> Failed(IEnumerable<string> errors) => new(false, false, default, errors.ToList());

        public static new AccountResult<T> NotFound(params string[] errors) => new(false, true, default, errors);
    }
}

//.....................................o0oEND OF FILEo0o........................................//

