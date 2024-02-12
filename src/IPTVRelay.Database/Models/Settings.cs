using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Database.Models
{
    public class Settings : ModelBase
    {
        public List<Setting> List { get; set; } = new List<Setting>();
    }
    public class Setting : ModelBase
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public long SettingsId { get; set; }
    }
}
