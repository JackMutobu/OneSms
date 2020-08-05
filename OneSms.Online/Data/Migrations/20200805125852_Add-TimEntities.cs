using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OneSms.Online.Data.Migrations
{
    public partial class AddTimEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTimServer",
                table: "MobileServers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TimClients",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Names = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    NumberOfMinutes = table.Column<int>(nullable: false),
                    ActivationTime = table.Column<DateTime>(nullable: false),
                    ClientState = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimClients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Number = table.Column<string>(nullable: true),
                    Date = table.Column<string>(nullable: true),
                    Time = table.Column<string>(nullable: true),
                    Minutes = table.Column<string>(nullable: true),
                    Cost = table.Column<int>(nullable: false),
                    ClientId = table.Column<int>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimTransactions_TimClients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "TimClients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimTransactions_ClientId",
                table: "TimTransactions",
                column: "ClientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimTransactions");

            migrationBuilder.DropTable(
                name: "TimClients");

            migrationBuilder.DropColumn(
                name: "IsTimServer",
                table: "MobileServers");
        }
    }
}
