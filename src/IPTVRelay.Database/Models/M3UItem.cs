using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

        [NotMapped]
        private string? Context;

        public override string ToString()
        {
            if(Context == null)
            {
                var items = Data.Select(kv => $"{kv.Key} : {kv.Value}").ToArray();
                items = items.Prepend($"{nameof(Url)} : {Url}").ToArray();
                Context = string.Join(",", items);
            }
            return Context;
        }
    }
}
