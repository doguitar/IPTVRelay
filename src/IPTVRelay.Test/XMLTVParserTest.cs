using IPTVRelay.Library;

namespace IPTVRelay.Test
{
    public class XMLTVParserTest
    {
        [Fact]
        public async void Parse()
        {
            var file = new FileInfo("data/DIRECTV-San-Diego.xml");
            var items = await XMLTVParser.Parse(new Uri($"file:///{file.FullName.Replace("\\","/")}"));
            Assert.NotNull(items);
        }
    }
}