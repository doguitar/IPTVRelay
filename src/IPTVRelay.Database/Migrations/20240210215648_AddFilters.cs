using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPTVRelay.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "M3UFilter",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    M3UId = table.Column<long>(type: "INTEGER", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FilterType = table.Column<int>(type: "TEXT", nullable: false),
                    FilterContent = table.Column<string>(type: "TEXT", nullable: true),
                    Invert = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_M3UFilter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_M3UFilter_M3U_M3UId",
                        column: x => x.M3UId,
                        principalTable: "M3U",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Mapping",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChannelId = table.Column<long>(type: "INTEGER", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mapping_XMLTV_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "XMLTV",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MappingFilter",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MappingId = table.Column<long>(type: "INTEGER", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FilterType = table.Column<int>(type: "TEXT", nullable: false),
                    FilterContent = table.Column<string>(type: "TEXT", nullable: true),
                    Invert = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappingFilter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MappingFilter_Mapping_MappingId",
                        column: x => x.MappingId,
                        principalTable: "Mapping",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_M3UFilter_M3UId",
                table: "M3UFilter",
                column: "M3UId");

            migrationBuilder.CreateIndex(
                name: "IX_Mapping_ChannelId",
                table: "Mapping",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Mapping_Id",
                table: "Mapping",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MappingFilter_MappingId",
                table: "MappingFilter",
                column: "MappingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "M3UFilter");

            migrationBuilder.DropTable(
                name: "MappingFilter");

            migrationBuilder.DropTable(
                name: "Mapping");
        }
    }
}
