using Microsoft.EntityFrameworkCore.Migrations;

namespace OneSms.Data.Migrations.V1
{
    public partial class AddSmsMinimumTreshold : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MinAirtimeBalance",
                table: "Sims",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinSmsBalance",
                table: "Sims",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinAirtimeBalance",
                table: "Sims");

            migrationBuilder.DropColumn(
                name: "MinSmsBalance",
                table: "Sims");
        }
    }
}
