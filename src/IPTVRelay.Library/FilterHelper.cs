using IPTVRelay.Database.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IPTVRelay.Library
{
	public static class FilterHelper
	{
		public static async Task<List<T>> DoFilter<T, F>(List<T> items, List<F> filter) where F : Filter
		{
			foreach (var f in filter)
			{
				items = await DoFilter(items, f);
			}
			return items;
		}
		public static async Task<List<T>> DoFilter<T>(List<T> items, Filter filter)
		{
			if (filter.FilterType == Database.Enums.FilterType.Sort)
			{
				IEnumerable<T> sorted;
				if (string.IsNullOrWhiteSpace(filter.FilterContent))
				{
					sorted = items.OrderBy(i => i.ToString());
				}
				else
				{
					sorted = items.OrderBy(i => i.ToString().Contains(filter.FilterContent, StringComparison.InvariantCultureIgnoreCase));
				}
				if (filter.Invert)
				{
					sorted = sorted.Reverse();
				}
				return sorted.ToList();
			}
			else
			{
				var bag = new ConcurrentBag<(T, long)>();
				await items.ParallelForEachAsync((item, index) =>
				{
					if (!DoFilter(item, index, items.Count, filter))
					{
						bag.Add((item, index));
					}
				});
				return bag.OrderBy(i => i.Item2).Select(i => i.Item1).ToList();
			}
		}

		private static bool DoFilter<T>(T item, long index, long total, Filter filter)
		{
			if (item == null || filter.FilterType == Database.Enums.FilterType.None) return false;
			var context = item.ToString();
			var filtered = filter.Invert;
			switch (filter.FilterType)
			{
				case Database.Enums.FilterType.Contains:
					if (context == null || string.IsNullOrEmpty(filter.FilterContent)) return false;
					if (context.Contains(filter.FilterContent, StringComparison.InvariantCultureIgnoreCase))
					{
						filtered = !filtered;
					}
					break;
				case Database.Enums.FilterType.Regex:
					if (context == null || string.IsNullOrWhiteSpace(filter.FilterContent)) return false;
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

		private static bool DoFilter(string context, long index, long total, Filter filter)
		{
			if (filter.FilterType == Database.Enums.FilterType.None) return false;
			var filtered = filter.Invert;
			switch (filter.FilterType)
			{
				case Database.Enums.FilterType.Contains:
					if (context.ToLowerInvariant().Contains(filter.FilterContent.ToLowerInvariant()))
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
	}
}
