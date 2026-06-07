//ST10445500 - PROG7311 - GLMS POE
//AccountResult

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services
{
    public class AccountResult
    {
        protected AccountResult(bool succeeded, IReadOnlyList<string> errors)
        {
            Succeeded = succeeded;
            Errors = errors;
        }

        public bool Succeeded { get; }
        public IReadOnlyList<string> Errors { get; }

        public static AccountResult Success() => new(true, []);

        public static AccountResult Failed(params string[] errors) => new(false, errors);

        public static AccountResult Failed(IEnumerable<string> errors) => new(false, errors.ToList());
    }
}

//.....................................o0oEND OF FILEo0o........................................//
