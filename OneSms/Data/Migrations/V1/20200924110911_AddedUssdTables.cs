using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OneSms.Data.Migrations.V1
{
    public partial class AddedUssdTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UssdActions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false),
                    UssdNumber = table.Column<string>(nullable: false),
                    ActionType = table.Column<int>(nullable: false),
                    KeyLogins = table.Column<string>(nullable: false),
                    KeyProblems = table.Column<string>(nullable: false),
                    NetworkId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UssdActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UssdActions_Networks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UssdActionSteps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: false),
                    IsPlaceHolder = table.Column<bool>(nullable: false),
                    CanSkipe = table.Column<bool>(nullable: false),
                    UssdActionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UssdActionSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UssdActionSteps_UssdActions_UssdActionId",
                        column: x => x.UssdActionId,
                        principalTable: "UssdActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UssdTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<Guid>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    UssdActionId = table.Column<int>(nullable: true),
                    SimId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UssdTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UssdTransactions_Sims_SimId",
                        column: x => x.SimId,
                        principalTable: "Sims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UssdTransactions_UssdActions_UssdActionId",
                        column: x => x.UssdActionId,
                        principalTable: "UssdActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UssdActions_NetworkId",
                table: "UssdActions",
                column: "NetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_UssdActionSteps_UssdActionId",
                table: "UssdActionSteps",
                column: "UssdActionId");

            migrationBuilder.CreateIndex(
                name: "IX_UssdTransactions_SimId",
                table: "UssdTransactions",
                column: "SimId");

            migrationBuilder.CreateIndex(
                name: "IX_UssdTransactions_UssdActionId",
                table: "UssdTransactions",
                column: "UssdActionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UssdActionSteps");

            migrationBuilder.DropTable(
                name: "UssdTransactions");

            migrationBuilder.DropTable(
                name: "UssdActions");
        }
    }
}
