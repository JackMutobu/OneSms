using Microsoft.EntityFrameworkCore.Migrations;

namespace OneSms.Data.Migrations.V1
{
    public partial class AddDataExtractors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NetworkMessageExtractors",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(nullable: true),
                    RegexPatern = table.Column<string>(nullable: false),
                    OriginatingAddress = table.Column<string>(nullable: false),
                    NetworkId = table.Column<int>(nullable: false),
                    NetworkAction = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkMessageExtractors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NetworkMessageExtractors_Networks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NetworkMessageExtractors_NetworkId",
                table: "NetworkMessageExtractors",
                column: "NetworkId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NetworkMessageExtractors");
        }
    }
}
