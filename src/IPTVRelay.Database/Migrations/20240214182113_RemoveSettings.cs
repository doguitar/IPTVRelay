using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPTVRelay.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Setting_Settings_SettingsId",
                table: "Setting");

            migrationBuilder.DropTable(
                name: "M3UItemData");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "M3UItem");

            migrationBuilder.DropIndex(
                name: "IX_Setting_SettingsId",
                table: "Setting");

            migrationBuilder.DropColumn(
                name: "SettingsId",
                table: "Setting");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SettingsId",
                table: "Setting",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "M3UItem",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    M3UId = table.Column<long>(type: "INTEGER", nullable: false),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: true)
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
                name: "M3UItemData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    M3UItemId = table.Column<long>(type: "INTEGER", nullable: false),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_Setting_SettingsId",
                table: "Setting",
                column: "SettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_M3UItemData_M3UItemId",
                table: "M3UItemData",
                column: "M3UItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_Id",
                table: "Settings",
                column: "Id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Setting_Settings_SettingsId",
                table: "Setting",
                column: "SettingsId",
                principalTable: "Settings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
