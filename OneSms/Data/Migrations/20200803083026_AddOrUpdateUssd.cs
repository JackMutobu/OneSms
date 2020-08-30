using Microsoft.EntityFrameworkCore.Migrations;

namespace OneSms.Online.Data.Migrations
{
    public partial class AddOrUpdateUssd : Migration
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

            migrationBuilder.AddColumn<string>(
                name: "UssdNumber",
                table: "UssdActions",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastMessage",
                table: "UssdTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionState",
                table: "UssdTransactions");

            migrationBuilder.DropColumn(
                name: "UssdNumber",
                table: "UssdActions");
        }
    }
}
