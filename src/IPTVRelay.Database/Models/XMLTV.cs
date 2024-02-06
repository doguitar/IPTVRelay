using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Database.Models
{
    public class XMLTV : ModelBase
    {
        public string Uri { get; set; }
        public virtual List<XMLTVItem> Items { get; set; }
    }
}
