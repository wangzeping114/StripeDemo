using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace stripeapi.Migrations
{
    /// <inheritdoc />
    public partial class addAgentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgentAccount",
                table: "WithdrawRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AgentId",
                table: "WithdrawRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AgentAccount",
                table: "DepositRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AgentId",
                table: "DepositRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentAccount",
                table: "WithdrawRecords");

            migrationBuilder.DropColumn(
                name: "AgentId",
                table: "WithdrawRecords");

            migrationBuilder.DropColumn(
                name: "AgentAccount",
                table: "DepositRecords");

            migrationBuilder.DropColumn(
                name: "AgentId",
                table: "DepositRecords");
        }
    }
}
