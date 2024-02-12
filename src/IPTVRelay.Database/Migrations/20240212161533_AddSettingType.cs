using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPTVRelay.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Setting",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Setting");
        }
    }
}
