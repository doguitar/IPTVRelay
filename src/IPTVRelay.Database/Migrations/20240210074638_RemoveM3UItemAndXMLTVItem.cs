using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPTVRelay.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveM3UItemAndXMLTVItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_M3UItem_M3U_M3UId",
                table: "M3UItem");

            migrationBuilder.DropForeignKey(
                name: "FK_XMLTVItem_XMLTV_XMLTVId",
                table: "XMLTVItem");

            migrationBuilder.DropIndex(
                name: "IX_XMLTVItemsData_Id",
                table: "XMLTVItemsData");

            migrationBuilder.DropIndex(
                name: "IX_XMLTVItem_Id",
                table: "XMLTVItem");

            migrationBuilder.DropIndex(
                name: "IX_XMLTVItem_XMLTVId",
                table: "XMLTVItem");

            migrationBuilder.DropIndex(
                name: "IX_M3UItemData_Id",
                table: "M3UItemData");

            migrationBuilder.DropIndex(
                name: "IX_M3UItem_Id",
                table: "M3UItem");

            migrationBuilder.DropIndex(
                name: "IX_M3UItem_M3UId",
                table: "M3UItem");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_XMLTVItemsData_Id",
                table: "XMLTVItemsData",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XMLTVItem_Id",
                table: "XMLTVItem",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XMLTVItem_XMLTVId",
                table: "XMLTVItem",
                column: "XMLTVId");

            migrationBuilder.CreateIndex(
                name: "IX_M3UItemData_Id",
                table: "M3UItemData",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_M3UItem_Id",
                table: "M3UItem",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_M3UItem_M3UId",
                table: "M3UItem",
                column: "M3UId");

            migrationBuilder.AddForeignKey(
                name: "FK_M3UItem_M3U_M3UId",
                table: "M3UItem",
                column: "M3UId",
                principalTable: "M3U",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_XMLTVItem_XMLTV_XMLTVId",
                table: "XMLTVItem",
                column: "XMLTVId",
                principalTable: "XMLTV",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
