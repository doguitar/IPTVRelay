using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPTVRelay.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "M3UItem",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Url = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_M3UItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "XMLTVItem",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChannelId = table.Column<string>(type: "TEXT", nullable: false),
                    Logo = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XMLTVItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "M3UItemData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    M3UItemId = table.Column<long>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_M3UItemData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_M3UItemData_M3UItem_M3UItemId",
                        column: x => x.M3UItemId,
                        principalTable: "M3UItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Setting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    SettingsId = table.Column<long>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Setting_Settings_SettingsId",
                        column: x => x.SettingsId,
                        principalTable: "Settings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "XMLTVItemsData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    XMLTVItemId = table.Column<long>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                name: "IX_M3UItem_Id",
                table: "M3UItem",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_M3UItemData_Id",
                table: "M3UItemData",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_M3UItemData_M3UItemId",
                table: "M3UItemData",
                column: "M3UItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Setting_Id",
                table: "Setting",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Setting_SettingsId",
                table: "Setting",
                column: "SettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_Id",
                table: "Settings",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XMLTVItem_Id",
                table: "XMLTVItem",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XMLTVItemsData_Id",
                table: "XMLTVItemsData",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XMLTVItemsData_XMLTVItemId",
                table: "XMLTVItemsData",
                column: "XMLTVItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "M3UItemData");

            migrationBuilder.DropTable(
                name: "Setting");

            migrationBuilder.DropTable(
                name: "XMLTVItemsData");

            migrationBuilder.DropTable(
                name: "M3UItem");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "XMLTVItem");
        }
    }
}
