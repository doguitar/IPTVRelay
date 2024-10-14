using IPTVRelay.Database.Models;
using Updogg;

namespace IPTVRelay.Blazor.ViewModels
{
    public class GuideModalVM
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
        public long Count { get; set; }

        public GuideModalVM()
        {
        }

        public GuideModalVM(XMLTV guide)
        {
            guide.ThrowIfArgumentNull();

            Id = guide.Id;
            Name = guide.Name;
            Uri = guide.Uri;
            Count = guide.Items.Count;
        }
    }
}
