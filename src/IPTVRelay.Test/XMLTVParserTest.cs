using IPTVRelay.Library;

namespace IPTVRelay.Test
{
    public class XMLTVParserTest
    {
        [Fact]
        public async void Parse()
        {
            var file = new FileInfo("data/DIRECTV-San-Diego.xml");
            var guide = new Database.Models.XMLTV();
            await XMLTVParser.Parse(new Uri($"file:///{file.FullName.Replace("\\","/")}"), guide);
            Assert.NotNull(guide.Items);
        }
    }
}