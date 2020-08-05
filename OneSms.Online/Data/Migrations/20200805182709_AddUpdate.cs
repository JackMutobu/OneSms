using Microsoft.EntityFrameworkCore.Migrations;

namespace OneSms.Online.Data.Migrations
{
    public partial class AddUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimTransactions_TimClients_ClientId",
                table: "TimTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "ClientId",
                table: "TimTransactions",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TransactionState",
                table: "TimTransactions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_TimTransactions_TimClients_ClientId",
                table: "TimTransactions",
                column: "ClientId",
                principalTable: "TimClients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimTransactions_TimClients_ClientId",
                table: "TimTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionState",
                table: "TimTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "ClientId",
                table: "TimTransactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TimTransactions_TimClients_ClientId",
                table: "TimTransactions",
                column: "ClientId",
                principalTable: "TimClients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
