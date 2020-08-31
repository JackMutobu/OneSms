using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OneSms.Data.Migrations.V1
{
    public partial class AddMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Apps_AspNetUsers_UserId",
                table: "Apps");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Apps",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "SmsMessages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Body = table.Column<string>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    CompletedTime = table.Column<DateTime>(nullable: false),
                    RecieverNumber = table.Column<string>(nullable: false),
                    SenderNumber = table.Column<string>(nullable: false),
                    TransactionId = table.Column<Guid>(nullable: false),
                    MessageStatus = table.Column<int>(nullable: false),
                    MessageProcessor = table.Column<int>(nullable: false),
                    Tags = table.Column<string>(nullable: false),
                    Label = table.Column<string>(nullable: false),
                    MobileServerId = table.Column<Guid>(nullable: false),
                    AppId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsMessages_Apps_AppId",
                        column: x => x.AppId,
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SmsMessages_MobileServers_MobileServerId",
                        column: x => x.MobileServerId,
                        principalTable: "MobileServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WhatsappMessages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Body = table.Column<string>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    CompletedTime = table.Column<DateTime>(nullable: false),
                    RecieverNumber = table.Column<string>(nullable: false),
                    SenderNumber = table.Column<string>(nullable: false),
                    TransactionId = table.Column<Guid>(nullable: false),
                    MessageStatus = table.Column<int>(nullable: false),
                    MessageProcessor = table.Column<int>(nullable: false),
                    Tags = table.Column<string>(nullable: false),
                    Label = table.Column<string>(nullable: false),
                    MobileServerId = table.Column<Guid>(nullable: false),
                    AppId = table.Column<Guid>(nullable: false),
                    ImageLinkOne = table.Column<string>(nullable: false),
                    ImageLinkTwo = table.Column<string>(nullable: false),
                    ImageLinkThree = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsappMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhatsappMessages_Apps_AppId",
                        column: x => x.AppId,
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WhatsappMessages_MobileServers_MobileServerId",
                        column: x => x.MobileServerId,
                        principalTable: "MobileServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SmsMessages_AppId",
                table: "SmsMessages",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsMessages_MobileServerId",
                table: "SmsMessages",
                column: "MobileServerId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappMessages_AppId",
                table: "WhatsappMessages",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappMessages_MobileServerId",
                table: "WhatsappMessages",
                column: "MobileServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Apps_AspNetUsers_UserId",
                table: "Apps",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Apps_AspNetUsers_UserId",
                table: "Apps");

            migrationBuilder.DropTable(
                name: "SmsMessages");

            migrationBuilder.DropTable(
                name: "WhatsappMessages");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Apps",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddForeignKey(
                name: "FK_Apps_AspNetUsers_UserId",
                table: "Apps",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
