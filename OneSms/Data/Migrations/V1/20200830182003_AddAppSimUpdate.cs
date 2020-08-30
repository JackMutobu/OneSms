using Microsoft.EntityFrameworkCore.Migrations;

namespace OneSms.Data.Migrations.V1
{
    public partial class AddAppSimUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationSim_Apps_AppId",
                table: "ApplicationSim");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationSim_Sims_SimId",
                table: "ApplicationSim");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationSim",
                table: "ApplicationSim");

            migrationBuilder.RenameTable(
                name: "ApplicationSim",
                newName: "AppSims");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationSim_SimId",
                table: "AppSims",
                newName: "IX_AppSims_SimId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppSims",
                table: "AppSims",
                columns: new[] { "AppId", "SimId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AppSims_Apps_AppId",
                table: "AppSims",
                column: "AppId",
                principalTable: "Apps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppSims_Sims_SimId",
                table: "AppSims",
                column: "SimId",
                principalTable: "Sims",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppSims_Apps_AppId",
                table: "AppSims");

            migrationBuilder.DropForeignKey(
                name: "FK_AppSims_Sims_SimId",
                table: "AppSims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppSims",
                table: "AppSims");

            migrationBuilder.RenameTable(
                name: "AppSims",
                newName: "ApplicationSim");

            migrationBuilder.RenameIndex(
                name: "IX_AppSims_SimId",
                table: "ApplicationSim",
                newName: "IX_ApplicationSim_SimId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationSim",
                table: "ApplicationSim",
                columns: new[] { "AppId", "SimId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationSim_Apps_AppId",
                table: "ApplicationSim",
                column: "AppId",
                principalTable: "Apps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationSim_Sims_SimId",
                table: "ApplicationSim",
                column: "SimId",
                principalTable: "Sims",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
