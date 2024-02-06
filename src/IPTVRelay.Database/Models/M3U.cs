using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Database.Models
{
    public class M3U : ModelBase
    {
        public string? Uri { get; set; }

        public virtual List<M3UItem> Items { get; set; } = new List<M3UItem>();
    }
}
