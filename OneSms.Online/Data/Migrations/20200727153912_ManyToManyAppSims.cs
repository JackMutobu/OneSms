using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OneSms.Online.Data.Migrations
{
    public partial class ManyToManyAppSims : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sims_Apps_OneSmsAppAppId",
                table: "Sims");

            migrationBuilder.DropIndex(
                name: "IX_Sims_OneSmsAppAppId",
                table: "Sims");

            migrationBuilder.DropColumn(
                name: "OneSmsAppAppId",
                table: "Sims");

            migrationBuilder.DropColumn(
                name: "OneSmsAppId",
                table: "Sims");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Apps",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AppSims",
                columns: table => new
                {
                    AppId = table.Column<Guid>(nullable: false),
                    SimId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSims", x => new { x.AppId, x.SimId });
                    table.ForeignKey(
                        name: "FK_AppSims_Apps_AppId",
                        column: x => x.AppId,
                        principalTable: "Apps",
                        principalColumn: "AppId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppSims_Sims_SimId",
                        column: x => x.SimId,
                        principalTable: "Sims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSims_SimId",
                table: "AppSims",
                column: "SimId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSims");

            migrationBuilder.AddColumn<Guid>(
                name: "OneSmsAppAppId",
                table: "Sims",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OneSmsAppId",
                table: "Sims",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Apps",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_Sims_OneSmsAppAppId",
                table: "Sims",
                column: "OneSmsAppAppId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sims_Apps_OneSmsAppAppId",
                table: "Sims",
                column: "OneSmsAppAppId",
                principalTable: "Apps",
                principalColumn: "AppId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
