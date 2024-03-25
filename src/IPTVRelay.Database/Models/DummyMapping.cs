using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Database.Models
{
    public class DummyMapping : ModelBase
    {
        public string? TimeExpression { get; set; } = @"(?<=\D)\d{2}:\d{2}(?=\D)";
        public string? TitleExpression { get; set; }
        public string? TitleFormat { get; set; } = "{0}";
        public bool IncludeBlank { get; set; } = true;

    }
}
