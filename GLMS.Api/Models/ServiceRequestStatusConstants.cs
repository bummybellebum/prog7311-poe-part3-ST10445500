//ST10445500 - PROG7311 - GLMS POE
//ServiceRequestStatusConstants

//.....................................o0oSTART OF FILEo0o........................................//

// These constants stop status and role id values from being repeated through the code.




namespace GLMS.Api.Models
{
	public static class ServiceRequestStatusConstants
	{
		public const int PendingId = 1;
		public const int ApprovedId = 2;
		public const int InProgressId = 3;
		public const int CompletedId = 4;
		public const int CancelledId = 5;

		public const string PendingName = "Pending";
		public const string ApprovedName = "Approved";
		public const string InProgressName = "In Progress";
		public const string CompletedName = "Completed";
		public const string CancelledName = "Cancelled";

		public static bool IsOpenStatus(int statusId)
		{
			return statusId == PendingId || statusId == InProgressId;
		}

		public static bool IsClosedStatus(int statusId)
		{
			return statusId == CompletedId || statusId == CancelledId;
		}
	}
}

//.....................................o0oEND OF FILEo0o..........................................//
