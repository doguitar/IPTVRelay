using IPTVRelay.Database.Models;
using System;
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

        public static async Task ParallelForEachAsync<T>(this IEnumerable<T> items, Action action)
        {
            await Task.Run(() =>
                Parallel.ForEach(items, (item, state, index) =>
                {
                    action();
                }));
        }
        public static async Task ParallelForEachAsync<T>(this IEnumerable<T> items, Action<T, long> action)
        {
            await Task.Run(() =>
                Parallel.ForEach(items, (item, state, index) =>
                {
                    action(item, index);
                }));
        }
        public static async Task ParallelForEachAsync<T>(this IEnumerable<T> items, Action<T, ParallelLoopState> action)
        {
            await Task.Run(() =>
                Parallel.ForEach(items, (item, state, index) =>
                {
                    action(item, state);
                }));
        }
        public static async Task ParallelForEachAsync<T>(this IEnumerable<T> items, Action<T> action)
        {
            await Task.Run(() =>
                Parallel.ForEach(items, (item, state, index) =>
                {
                    action(item);
                }));
        }
    }
}
