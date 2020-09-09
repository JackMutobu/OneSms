using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OneSms.Data.Migrations.V1
{
    public partial class AddContactEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Organization",
                table: "Apps",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Organization = table.Column<string>(nullable: true),
                    JobTitle = table.Column<string>(nullable: true),
                    StreetAddress = table.Column<string>(nullable: true),
                    Zip = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    CountryName = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    Mobile = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    HomePage = table.Column<string>(nullable: true),
                    Image = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppContact",
                columns: table => new
                {
                    AppId = table.Column<Guid>(nullable: false),
                    ContactId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppContact", x => new { x.AppId, x.ContactId });
                    table.ForeignKey(
                        name: "FK_AppContact_Apps_AppId",
                        column: x => x.AppId,
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppContact_Contact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppContact_ContactId",
                table: "AppContact",
                column: "ContactId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppContact");

            migrationBuilder.DropTable(
                name: "Contact");

            migrationBuilder.DropColumn(
                name: "Organization",
                table: "Apps");
        }
    }
}
