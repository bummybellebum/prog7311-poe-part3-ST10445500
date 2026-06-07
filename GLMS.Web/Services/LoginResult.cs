//ST10445500 - PROG7311 - GLMS POE
//LoginResult

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services
{
    public class LoginResult : AccountResult
    {
        private LoginResult(bool succeeded, bool isLockedOut, bool requiresTwoFactor, IReadOnlyList<string> errors)
            : base(succeeded, errors)
        {
            IsLockedOut = isLockedOut;
            RequiresTwoFactor = requiresTwoFactor;
        }

        public bool IsLockedOut { get; }
        public bool RequiresTwoFactor { get; }

        public static LoginResult SuccessLogin() => new(true, false, false, []);

        public static LoginResult LockedOut() => new(false, true, false, ["This account is locked. Try again later."]);

        public static LoginResult TwoFactorRequired() => new(false, false, true, ["Two-factor sign-in is not enabled for GLMS."]);

        public static LoginResult FailedLogin(params string[] errors) => new(false, false, false, errors);
    }
}

//.....................................o0oEND OF FILEo0o........................................//
