using Microsoft.EntityFrameworkCore.Migrations;

namespace OneSms.Data.Migrations.V1
{
    public partial class AddContactEntityMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppContact_Contact_ContactId",
                table: "AppContact");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contact",
                table: "Contact");

            migrationBuilder.RenameTable(
                name: "Contact",
                newName: "Contacts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contacts",
                table: "Contacts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppContact_Contacts_ContactId",
                table: "AppContact",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppContact_Contacts_ContactId",
                table: "AppContact");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contacts",
                table: "Contacts");

            migrationBuilder.RenameTable(
                name: "Contacts",
                newName: "Contact");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contact",
                table: "Contact",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppContact_Contact_ContactId",
                table: "AppContact",
                column: "ContactId",
                principalTable: "Contact",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
