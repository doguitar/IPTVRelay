using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPTVRelay.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mapping_XMLTV_ChannelId",
                table: "Mapping");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "Mapping",
                newName: "XMLTVItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Mapping_ChannelId",
                table: "Mapping",
                newName: "IX_Mapping_XMLTVItemId");

            migrationBuilder.AddColumn<ulong>(
                name: "Channel",
                table: "Mapping",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<long>(
                name: "M3UId",
                table: "Mapping",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Mapping",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Mapping_M3UId",
                table: "Mapping",
                column: "M3UId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mapping_M3U_M3UId",
                table: "Mapping",
                column: "M3UId",
                principalTable: "M3U",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Mapping_XMLTVItem_XMLTVItemId",
                table: "Mapping",
                column: "XMLTVItemId",
                principalTable: "XMLTVItem",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mapping_M3U_M3UId",
                table: "Mapping");

            migrationBuilder.DropForeignKey(
                name: "FK_Mapping_XMLTVItem_XMLTVItemId",
                table: "Mapping");

            migrationBuilder.DropIndex(
                name: "IX_Mapping_M3UId",
                table: "Mapping");

            migrationBuilder.DropColumn(
                name: "Channel",
                table: "Mapping");

            migrationBuilder.DropColumn(
                name: "M3UId",
                table: "Mapping");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Mapping");

            migrationBuilder.RenameColumn(
                name: "XMLTVItemId",
                table: "Mapping",
                newName: "ChannelId");

            migrationBuilder.RenameIndex(
                name: "IX_Mapping_XMLTVItemId",
                table: "Mapping",
                newName: "IX_Mapping_ChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mapping_XMLTV_ChannelId",
                table: "Mapping",
                column: "ChannelId",
                principalTable: "XMLTV",
                principalColumn: "Id");
        }
    }
}
