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
        }

        [Fact]
        public async void Create()
        {
            var file = new FileInfo("data/tv_channels_f1fd4cadc5_plus.m3u");
            var items = await M3UParser.Parse(new Uri($"file:///{file.FullName.Replace("\\", "/")}"));
            Assert.NotNull(items);
            var sb = await M3UParser.Create(items);
            Assert.NotNull(sb);
            Assert.True(sb.Length > 0);
        }
    }
}