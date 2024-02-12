using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Library
{
    public static partial class Extensions
    {
        public static string GetDescription<T>(this T instance) where T : Enum
        {
            //instance.GetType().Attributes.
            return string.Empty;
        }
    }
}
