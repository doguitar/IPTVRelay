using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPTVRelay.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddM3UAndXMLTV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ChannelId",
                table: "XMLTVItem",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<long>(
                name: "XMLTVId",
                table: "XMLTVItem",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "M3UId",
                table: "M3UItem",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "M3U",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uri = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_M3U", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "XMLTV",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uri = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XMLTV", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_XMLTVItem_XMLTVId",
                table: "XMLTVItem",
                column: "XMLTVId");

            migrationBuilder.CreateIndex(
                name: "IX_M3UItem_M3UId",
                table: "M3UItem",
                column: "M3UId");

            migrationBuilder.CreateIndex(
                name: "IX_M3U_Id",
                table: "M3U",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XMLTV_Id",
                table: "XMLTV",
                column: "Id",
                unique: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_M3UItem_M3U_M3UId",
                table: "M3UItem");

            migrationBuilder.DropForeignKey(
                name: "FK_XMLTVItem_XMLTV_XMLTVId",
                table: "XMLTVItem");

            migrationBuilder.DropTable(
                name: "M3U");

            migrationBuilder.DropTable(
                name: "XMLTV");

            migrationBuilder.DropIndex(
                name: "IX_XMLTVItem_XMLTVId",
                table: "XMLTVItem");

            migrationBuilder.DropIndex(
                name: "IX_M3UItem_M3UId",
                table: "M3UItem");

            migrationBuilder.DropColumn(
                name: "XMLTVId",
                table: "XMLTVItem");

            migrationBuilder.DropColumn(
                name: "M3UId",
                table: "M3UItem");

            migrationBuilder.AlterColumn<string>(
                name: "ChannelId",
                table: "XMLTVItem",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
