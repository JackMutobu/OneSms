using Microsoft.EntityFrameworkCore.Migrations;

namespace OneSms.Online.Data.Migrations
{
    public partial class AddNullableSimToUssdTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UssdTransactions_Sims_SimCardId",
                table: "UssdTransactions");

            migrationBuilder.DropIndex(
                name: "IX_UssdTransactions_SimCardId",
                table: "UssdTransactions");

            migrationBuilder.DropColumn(
                name: "SimCardId",
                table: "UssdTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "SimId",
                table: "UssdTransactions",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_UssdTransactions_SimId",
                table: "UssdTransactions",
                column: "SimId");

            migrationBuilder.AddForeignKey(
                name: "FK_UssdTransactions_Sims_SimId",
                table: "UssdTransactions",
                column: "SimId",
                principalTable: "Sims",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UssdTransactions_Sims_SimId",
                table: "UssdTransactions");

            migrationBuilder.DropIndex(
                name: "IX_UssdTransactions_SimId",
                table: "UssdTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "SimId",
                table: "UssdTransactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SimCardId",
                table: "UssdTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UssdTransactions_SimCardId",
                table: "UssdTransactions",
                column: "SimCardId");

            migrationBuilder.AddForeignKey(
                name: "FK_UssdTransactions_Sims_SimCardId",
                table: "UssdTransactions",
                column: "SimCardId",
                principalTable: "Sims",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
