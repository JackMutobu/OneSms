using Microsoft.EntityFrameworkCore.Migrations;

namespace OneSms.Data.Migrations.V1
{
    public partial class UpdateUssdTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastMessage",
                table: "UssdTransactions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransactionState",
                table: "UssdTransactions",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastMessage",
                table: "UssdTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionState",
                table: "UssdTransactions");
        }
    }
}
