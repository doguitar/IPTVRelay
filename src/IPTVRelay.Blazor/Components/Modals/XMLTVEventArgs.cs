using IPTVRelay.Database.Models;
using IPTVRelay.Library.Components;

namespace IPTVRelay.Blazor.Components.Modals
{
    public class XMLTVEventArgs : ModalEventArgs<XMLTV>
    {
        public string Content { get; set; }

        public XMLTVEventArgs(XMLTV model, string content) : base(model)
        {
            Content = content;
        }
    }
}
