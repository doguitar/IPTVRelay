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
        public string? DisplayName { get; set; }
        public string? Logo { get; set; }

        [NotMapped]
        public List<XMLTVItemData> Data { get; set; } = [];


        public long? XMLTVGuideDataId { get; set; }

        public virtual XMLTVGuideData? GuideData { get; set; }
        public virtual List<Mapping> Mappings { get; set; } = [];
        public virtual XMLTV? XMLTV { get; set; }
        public long XMLTVId { get; set; }
    }

}
