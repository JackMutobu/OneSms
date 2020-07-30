using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OneSms.Online.Data.Migrations
{
    public partial class AddSmsApp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OneSmsAppAppId",
                table: "SmsTransactions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OneSmsAppId",
                table: "SmsTransactions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SmsTransactions_OneSmsAppAppId",
                table: "SmsTransactions",
                column: "OneSmsAppAppId");

            migrationBuilder.AddForeignKey(
                name: "FK_SmsTransactions_Apps_OneSmsAppAppId",
                table: "SmsTransactions",
                column: "OneSmsAppAppId",
                principalTable: "Apps",
                principalColumn: "AppId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmsTransactions_Apps_OneSmsAppAppId",
                table: "SmsTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SmsTransactions_OneSmsAppAppId",
                table: "SmsTransactions");

            migrationBuilder.DropColumn(
                name: "OneSmsAppAppId",
                table: "SmsTransactions");

            migrationBuilder.DropColumn(
                name: "OneSmsAppId",
                table: "SmsTransactions");
        }
    }
}
