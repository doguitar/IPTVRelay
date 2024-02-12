using IPTVRelay.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IPTVRelay.Library
{
    public static class FilterHelper
    {
        public static bool DoFilter(string context, long index, long total, Filter filter)
        {
            if (filter.FilterType == Database.Enums.FilterType.None) return false;
            var filtered = filter.Invert;
            switch (filter.FilterType)
            {
                case Database.Enums.FilterType.Contains:
                    if (context.Contains(filter.FilterContent))
                    {
                        filtered = !filtered;
                    }
                    break;
                case Database.Enums.FilterType.Regex:
                    var re = new Regex(filter.FilterContent, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    if (re.Match(context).Success)
                    {
                        filtered = !filtered;
                    }
                    break;
                case Database.Enums.FilterType.First:
                    if (index == 0)
                    {
                        filtered = !filtered;
                    }
                    break;
                case Database.Enums.FilterType.Last:
                    if (index == total - 1)
                    {
                        filtered = !filtered;
                    }
                    break;
                case Database.Enums.FilterType.Index:
                    if (long.TryParse(filter.FilterContent, out var target) && target == index)
                    {
                        filtered = !filtered;
                    }
                    break;
            }
            return filtered;
        }

        public static bool DoFilter(M3UItem item, long index, long total, Filter filter)
        {
            return DoFilter(item.ToString(), index, total, filter);
        }
    }
}
