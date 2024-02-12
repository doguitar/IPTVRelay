using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Database.Models
{
    public class Mapping : ModelBase
    {
        public string Name { get; set; }
        public long Channel { get; set; } = 1;

        public long? XMLTVItemId { get; set; }
        public long? M3UId { get; set; }

        public virtual XMLTVItem? XMLTVItem { get; set; }
        public virtual M3U? M3U { get; set; }

        public virtual List<MappingFilter> Filters { get; set; } = [];
    }
}
