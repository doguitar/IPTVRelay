using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPTVRelay.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddM3UParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Count",
                table: "M3U",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ParentId",
                table: "M3U",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_M3U_ParentId",
                table: "M3U",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_M3U_M3U_ParentId",
                table: "M3U",
                column: "ParentId",
                principalTable: "M3U",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_M3U_M3U_ParentId",
                table: "M3U");

            migrationBuilder.DropIndex(
                name: "IX_M3U_ParentId",
                table: "M3U");

            migrationBuilder.DropColumn(
                name: "Count",
                table: "M3U");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "M3U");
        }
    }
}
