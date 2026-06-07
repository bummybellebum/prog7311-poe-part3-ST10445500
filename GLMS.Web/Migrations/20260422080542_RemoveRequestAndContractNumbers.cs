using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GLMS.Web.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRequestAndContractNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_RequestNumber",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ContractNumber",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "RequestNumber",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ContractNumber",
                table: "Contracts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestNumber",
                table: "ServiceRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContractNumber",
                table: "Contracts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_RequestNumber",
                table: "ServiceRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ContractNumber",
                table: "Contracts",
                column: "ContractNumber",
                unique: true);
        }
    }
}
