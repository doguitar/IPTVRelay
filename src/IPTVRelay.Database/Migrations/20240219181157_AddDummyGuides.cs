using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPTVRelay.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDummyGuides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(nameof(Models.XMLTV),
                [nameof(Models.XMLTV.Id), nameof(Models.XMLTV.Name), nameof(Models.XMLTV.Created), nameof(Models.XMLTV.Modified), nameof(Models.XMLTV.Uri)],
                [long.MaxValue, "Dummy", DateTime.MinValue, DateTime.MinValue, string.Empty]);

            foreach (var period in Enumerable.Range(1, 48).Select(i => i * 30))
            {
                migrationBuilder.InsertData(nameof(Models.XMLTVItem),
                    [nameof(Models.XMLTVItem.XMLTVId), nameof(Models.XMLTVItem.ChannelId), nameof(Models.XMLTVItem.Created), nameof(Models.XMLTVItem.Modified)],
                    [long.MaxValue, $"Dummy.{period:0000}", DateTime.MinValue, DateTime.MinValue + TimeSpan.FromMinutes(period)]);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(nameof(Models.XMLTVItem), nameof(Models.XMLTVItem.XMLTVId), long.MaxValue);
            migrationBuilder.DeleteData(nameof(Models.XMLTV), nameof(Models.XMLTV.Id), long.MaxValue);
        }
    }
}
