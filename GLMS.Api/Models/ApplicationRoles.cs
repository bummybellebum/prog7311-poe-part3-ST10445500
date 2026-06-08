//ST10445500 - PROG7311 - GLMS POE
//ApplicationRoles

//.....................................o0oSTART OF FILEo0o........................................//

// This model represents data that the API stores and works with in the database.




namespace GLMS.Api.Models
{
	public static class ApplicationRoles
	{
		public const string Admin = "Admin";
		public const string ContractManager = "ContractManager";
		public const string LogisticsManager = "LogisticsManager";

		public const string AdminOrContractManager = Admin + "," + ContractManager;
		public const string AdminOrLogisticsManager = Admin + "," + LogisticsManager;
		public const string AllRoles = Admin + "," + ContractManager + "," + LogisticsManager;

		public static readonly string[] All = [Admin, ContractManager, LogisticsManager];
	}
}

//.....................................o0oEND OF FILEo0o........................................//

//.....................................o0oEND OF FILEo0o..........................................//
