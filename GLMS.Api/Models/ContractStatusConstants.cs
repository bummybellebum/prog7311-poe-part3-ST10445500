//ST10445500 - PROG7311 - GLMS POE
//ContractStatusConstants

//.....................................o0oSTART OF FILEo0o........................................//

// These constants stop status and role id values from being repeated through the code.




namespace GLMS.Api.Models
{
    public static class ContractStatusConstants
    {
        public const int DraftId = 1;
        public const int ActiveId = 2;
        public const int OnHoldId = 3;
        public const int ExpiredId = 4;

        public const string DraftName = "Draft";
        public const string ActiveName = "Active";
        public const string OnHoldName = "On Hold";
        public const string ExpiredName = "Expired";
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
