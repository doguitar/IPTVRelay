using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Database.Models
{
    public class XMLTVItem : ModelBase
    {
        public string? ChannelId { get; set; }
        public string? Logo { get; set; }
        public virtual List<XMLTVItemData> Data { get; set; } = new List<XMLTVItemData>();

        public long XMLTVId { get; set; }
    }

}
