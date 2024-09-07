using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class TagImplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TagImplication",
                columns: table => new
                {
                    FromId = table.Column<int>(type: "int", nullable: false),
                    ToId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagImplication", x => new { x.FromId, x.ToId });
                    table.ForeignKey(
                        name: "FK_TagImplication_FromId",
                        column: x => x.FromId,
                        principalTable: "Tag",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TagImplication_ToId",
                        column: x => x.ToId,
                        principalTable: "Tag",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TagImplication_ToId",
                table: "TagImplication",
                column: "ToId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagImplication");
        }
    }
}
