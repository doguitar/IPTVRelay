using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPTVRelay.Database.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class addXmlGuideData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "XMLTVGuideDataId",
                table: "XMLTVItem",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "XMLTVGuideData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GuideData = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XMLTVGuideData", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_XMLTVItem_XMLTVGuideDataId",
                table: "XMLTVItem",
                column: "XMLTVGuideDataId");

            migrationBuilder.AddForeignKey(
                name: "FK_XMLTVItem_XMLTVGuideData_XMLTVGuideDataId",
                table: "XMLTVItem",
                column: "XMLTVGuideDataId",
                principalTable: "XMLTVGuideData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_XMLTVItem_XMLTVGuideData_XMLTVGuideDataId",
                table: "XMLTVItem");

            migrationBuilder.DropTable(
                name: "XMLTVGuideData");

            migrationBuilder.DropIndex(
                name: "IX_XMLTVItem_XMLTVGuideDataId",
                table: "XMLTVItem");

            migrationBuilder.DropColumn(
                name: "XMLTVGuideDataId",
                table: "XMLTVItem");
        }
    }
}
