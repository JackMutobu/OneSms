using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OneSms.Online.Data.Migrations
{
    public partial class UpdateMessageTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SenderNumber",
                table: "SmsTransactions",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "SmsTransactions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MessageTransactionProcessor",
                table: "SmsTransactions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "SmsTransactions",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "WhatsappTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    CompletedTime = table.Column<DateTime>(nullable: false),
                    RecieverNumber = table.Column<string>(nullable: false),
                    SenderNumber = table.Column<string>(nullable: true),
                    TransactionId = table.Column<Guid>(nullable: false),
                    TransactionState = table.Column<int>(nullable: false),
                    MessageTransactionProcessor = table.Column<int>(nullable: false),
                    Label = table.Column<string>(nullable: true),
                    MobileServerId = table.Column<int>(nullable: false),
                    OneSmsAppId = table.Column<Guid>(nullable: false),
                    ImageLinkOne = table.Column<string>(nullable: true),
                    ImageLinkTwo = table.Column<string>(nullable: true),
                    ImageLinkThree = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsappTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhatsappTransactions_MobileServers_MobileServerId",
                        column: x => x.MobileServerId,
                        principalTable: "MobileServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WhatsappTransactions_Apps_OneSmsAppId",
                        column: x => x.OneSmsAppId,
                        principalTable: "Apps",
                        principalColumn: "AppId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappTransactions_MobileServerId",
                table: "WhatsappTransactions",
                column: "MobileServerId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappTransactions_OneSmsAppId",
                table: "WhatsappTransactions",
                column: "OneSmsAppId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WhatsappTransactions");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "SmsTransactions");

            migrationBuilder.DropColumn(
                name: "MessageTransactionProcessor",
                table: "SmsTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "SmsTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "SenderNumber",
                table: "SmsTransactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
