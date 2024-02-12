using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPTVRelay.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "XMLTVItemsData");

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
                name: "IX_MappingFilter_Id",
                table: "MappingFilter",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_M3UFilter_Id",
                table: "M3UFilter",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_XMLTVItem_Id",
                table: "XMLTVItem");

            migrationBuilder.DropIndex(
                name: "IX_XMLTVItem_XMLTVId",
                table: "XMLTVItem");

            migrationBuilder.DropIndex(
                name: "IX_MappingFilter_Id",
                table: "MappingFilter");

            migrationBuilder.DropIndex(
                name: "IX_M3UFilter_Id",
                table: "M3UFilter");

            migrationBuilder.CreateTable(
                name: "XMLTVItemsData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    XMLTVItemId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XMLTVItemsData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_XMLTVItemsData_XMLTVItem_XMLTVItemId",
                        column: x => x.XMLTVItemId,
                        principalTable: "XMLTVItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_XMLTVItemsData_XMLTVItemId",
                table: "XMLTVItemsData",
                column: "XMLTVItemId");
        }
    }
}
