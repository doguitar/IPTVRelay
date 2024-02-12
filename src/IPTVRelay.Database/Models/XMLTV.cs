using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Database.Models
{
    public class XMLTV : ModelBase
    {
        public string? Name { get; set; }
        public string? Uri { get; set; }

        public virtual List<XMLTVItem> Items { get; set; } = [];
    }
}
