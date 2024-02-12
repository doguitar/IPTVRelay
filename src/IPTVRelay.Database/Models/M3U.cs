using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Database.Models
{
    public class M3U : ModelBase
    {
        public string? Name { get; set; }
        public string? Uri { get; set; }
        public long Count { get; set; }
        public long? ParentId { get; set; }

        public virtual M3U? Parent { get; set; }
        public virtual List<M3UFilter> Filters { get; set; } = [];

        [NotMapped]
        public List<M3UItem> Items { get; set; } = [];
    }
}
