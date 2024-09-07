using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IPTVRelay.Database.Psql.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "M3U",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Uri = table.Column<string>(type: "text", nullable: true),
                    Count = table.Column<long>(type: "bigint", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_M3U", x => x.Id);
                    table.ForeignKey(
                        name: "FK_M3U_M3U_ParentId",
                        column: x => x.ParentId,
                        principalTable: "M3U",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Setting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "XMLTV",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Uri = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XMLTV", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "M3UFilter",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    M3UId = table.Column<long>(type: "bigint", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FilterType = table.Column<string>(type: "TEXT", nullable: false),
                    FilterContent = table.Column<string>(type: "text", nullable: true),
                    Invert = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "XMLTVItem",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    ChannelId = table.Column<string>(type: "text", nullable: true),
                    Logo = table.Column<string>(type: "text", nullable: true),
                    XMLTVId = table.Column<long>(type: "bigint", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XMLTVItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_XMLTVItem_XMLTV_XMLTVId",
                        column: x => x.XMLTVId,
                        principalTable: "XMLTV",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mapping",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    Channel = table.Column<long>(type: "bigint", nullable: false),
                    XMLTVItemId = table.Column<long>(type: "bigint", nullable: true),
                    M3UId = table.Column<long>(type: "bigint", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mapping_M3U_M3UId",
                        column: x => x.M3UId,
                        principalTable: "M3U",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Mapping_XMLTVItem_XMLTVItemId",
                        column: x => x.XMLTVItemId,
                        principalTable: "XMLTVItem",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MappingFilter",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    MappingId = table.Column<long>(type: "bigint", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FilterType = table.Column<string>(type: "TEXT", nullable: false),
                    FilterContent = table.Column<string>(type: "text", nullable: true),
                    Invert = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "IX_M3U_Id",
                table: "M3U",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_M3U_ParentId",
                table: "M3U",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_M3UFilter_Id",
                table: "M3UFilter",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_M3UFilter_M3UId",
                table: "M3UFilter",
                column: "M3UId");

            migrationBuilder.CreateIndex(
                name: "IX_Mapping_Id",
                table: "Mapping",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mapping_M3UId",
                table: "Mapping",
                column: "M3UId");

            migrationBuilder.CreateIndex(
                name: "IX_Mapping_XMLTVItemId",
                table: "Mapping",
                column: "XMLTVItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingFilter_Id",
                table: "MappingFilter",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MappingFilter_MappingId",
                table: "MappingFilter",
                column: "MappingId");

            migrationBuilder.CreateIndex(
                name: "IX_Setting_Id",
                table: "Setting",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XMLTV_Id",
                table: "XMLTV",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XMLTVItem_Id",
                table: "XMLTVItem",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XMLTVItem_XMLTVId",
                table: "XMLTVItem",
                column: "XMLTVId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "M3UFilter");

            migrationBuilder.DropTable(
                name: "MappingFilter");

            migrationBuilder.DropTable(
                name: "Setting");

            migrationBuilder.DropTable(
                name: "Mapping");

            migrationBuilder.DropTable(
                name: "M3U");

            migrationBuilder.DropTable(
                name: "XMLTVItem");

            migrationBuilder.DropTable(
                name: "XMLTV");
        }
    }
}
