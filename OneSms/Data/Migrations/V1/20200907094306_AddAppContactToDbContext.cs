using Microsoft.EntityFrameworkCore.Migrations;

namespace OneSms.Data.Migrations.V1
{
    public partial class AddAppContactToDbContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppContact_Apps_AppId",
                table: "AppContact");

            migrationBuilder.DropForeignKey(
                name: "FK_AppContact_Contacts_ContactId",
                table: "AppContact");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppContact",
                table: "AppContact");

            migrationBuilder.RenameTable(
                name: "AppContact",
                newName: "AppContacts");

            migrationBuilder.RenameIndex(
                name: "IX_AppContact_ContactId",
                table: "AppContacts",
                newName: "IX_AppContacts_ContactId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppContacts",
                table: "AppContacts",
                columns: new[] { "AppId", "ContactId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AppContacts_Apps_AppId",
                table: "AppContacts",
                column: "AppId",
                principalTable: "Apps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppContacts_Contacts_ContactId",
                table: "AppContacts",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppContacts_Apps_AppId",
                table: "AppContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_AppContacts_Contacts_ContactId",
                table: "AppContacts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppContacts",
                table: "AppContacts");

            migrationBuilder.RenameTable(
                name: "AppContacts",
                newName: "AppContact");

            migrationBuilder.RenameIndex(
                name: "IX_AppContacts_ContactId",
                table: "AppContact",
                newName: "IX_AppContact_ContactId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppContact",
                table: "AppContact",
                columns: new[] { "AppId", "ContactId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AppContact_Apps_AppId",
                table: "AppContact",
                column: "AppId",
                principalTable: "Apps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppContact_Contacts_ContactId",
                table: "AppContact",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
