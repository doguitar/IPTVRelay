using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Database.Models
{
    public class M3UItem : ModelBase
    {
        public string? Url { get; set; }
        public virtual List<M3UItemData> Data { get; set; } = new List<M3UItemData>();

        public long M3UId { get; set; }
    }
}
