using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPTVRelay.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMoveDateTimeOffset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MappingId",
                table: "Mapping");

            migrationBuilder.DropColumn(
                name: "TimeOffset",
                table: "DummyMapping");

            migrationBuilder.AddColumn<int>(
                name: "TimeOffset",
                table: "Mapping",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeOffset",
                table: "Mapping");

            migrationBuilder.AddColumn<long>(
                name: "MappingId",
                table: "Mapping",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeOffset",
                table: "DummyMapping",
                type: "INTEGER",
                nullable: true);
        }
    }
}
