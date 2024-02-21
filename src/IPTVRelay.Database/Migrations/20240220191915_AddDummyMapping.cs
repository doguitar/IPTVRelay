using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPTVRelay.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDummyMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DummyMappingId",
                table: "Mapping",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MappingId",
                table: "Mapping",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DummyMapping",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimeExpression = table.Column<string>(type: "TEXT", nullable: true),
                    TitleExpression = table.Column<string>(type: "TEXT", nullable: true),
                    TitleFormat = table.Column<string>(type: "TEXT", nullable: true),
                    TimeOffset = table.Column<int>(type: "INTEGER", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DummyMapping", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mapping_DummyMappingId",
                table: "Mapping",
                column: "DummyMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_DummyMapping_Id",
                table: "DummyMapping",
                column: "Id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Mapping_DummyMapping_DummyMappingId",
                table: "Mapping",
                column: "DummyMappingId",
                principalTable: "DummyMapping",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mapping_DummyMapping_DummyMappingId",
                table: "Mapping");

            migrationBuilder.DropTable(
                name: "DummyMapping");

            migrationBuilder.DropIndex(
                name: "IX_Mapping_DummyMappingId",
                table: "Mapping");

            migrationBuilder.DropColumn(
                name: "DummyMappingId",
                table: "Mapping");

            migrationBuilder.DropColumn(
                name: "MappingId",
                table: "Mapping");
        }
    }
}
