using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Database.Models
{
    public class XMLTVItem : ModelBase
    {
        public string? ChannelId { get; set; }
        public string? Logo { get; set; }

        [NotMapped]
        public List<XMLTVItemData> Data { get; set; } = new List<XMLTVItemData>();

        public virtual XMLTV? XMLTV { get; set; }
        public long XMLTVId { get; set; }
    }

}
