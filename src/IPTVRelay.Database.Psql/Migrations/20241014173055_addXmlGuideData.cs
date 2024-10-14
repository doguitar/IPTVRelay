using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IPTVRelay.Database.Psql.Migrations
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
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "XMLTVGuideData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuideData = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                principalColumn: "Id");
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
