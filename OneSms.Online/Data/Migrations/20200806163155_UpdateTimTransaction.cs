using Microsoft.EntityFrameworkCore.Migrations;

namespace OneSms.Online.Data.Migrations
{
    public partial class UpdateTimTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastMessage",
                table: "TimTransactions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastMessage",
                table: "TimTransactions");
        }
    }
}
