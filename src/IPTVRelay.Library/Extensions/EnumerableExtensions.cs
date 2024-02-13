﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Library
{
    public static partial class Extensions
    {
            public static IEnumerable<T> Except<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> equalsFunc, Func<T, int> hashCodeFunc)
            {
                var comparer = new InlineEqualityComparer<T>(equalsFunc, hashCodeFunc);
                return first.Except(second, comparer);
            }
    }
}
