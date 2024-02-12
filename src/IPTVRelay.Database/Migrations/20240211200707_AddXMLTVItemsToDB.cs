using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPTVRelay.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddXMLTVItemsToDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_XMLTVItem_XMLTV_XMLTVId",
                table: "XMLTVItem",
                column: "XMLTVId",
                principalTable: "XMLTV",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_XMLTVItem_XMLTV_XMLTVId",
                table: "XMLTVItem");
        }
    }
}
