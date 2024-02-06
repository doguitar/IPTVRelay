using IPTVRelay.Library;

namespace IPTVRelay.Test
{
    public class M3UParserTest
    {
        [Fact]
        public async void Parse()
        {
            var file = new FileInfo("data/tv_channels_f1fd4cadc5_plus.m3u");
            var items = await M3UParser.Parse(new Uri($"file:///{file.FullName.Replace("\\", "/")}"));
            Assert.NotNull(items);
            //var grouped = items.Where(i => i.Data.TryGetValue("tvg-id", out var id) && !string.IsNullOrWhiteSpace(id)).GroupBy(i => i.Data["tvg-id"]).OrderBy(g => g.Key);
            //var us = grouped.Where(g => g.Key.EndsWith(".us")).OrderBy(g => g.Key);
            //var duplicates = grouped.Where(g => g.Count() > 1);
        }
    }
}