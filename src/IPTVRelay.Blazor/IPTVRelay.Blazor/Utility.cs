using Hangfire.Dashboard;
using IPTVRelay.Blazor.Components.Pages;
using IPTVRelay.Blazor.Components.Shared;
using IPTVRelay.Database;
using IPTVRelay.Database.Models;
using IPTVRelay.Library;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using static IPTVRelay.Blazor.Utility;

namespace IPTVRelay.Blazor
{
    public class Utility
    {
        public static class M3U
        {
            public static FileInfo GetFileInfo(IConfiguration config, Database.Models.M3U playlist)
            {
                var dataFolder = config.GetValue<string>("DATA_FOLDER");

                var dir = new DirectoryInfo(dataFolder);
                if (!dir.Exists)
                {
                    dir.Create();
                }
                var m3uDir = new DirectoryInfo(Path.Combine(dir.FullName, "m3u"));
                if (!m3uDir.Exists)
                {
                    m3uDir.Create();
                }

                var fileInfo = new FileInfo(Path.Combine(m3uDir.FullName, $"{playlist.Id}.m3u"));

                return fileInfo;
            }
            public static async Task<Database.Models.M3U> Populate(IConfiguration config, Database.Models.M3U playlist, bool refresh = false, bool applyFilters = true)
            {
                var file = GetFileInfo(config, playlist);
                if (!file.Exists) { refresh = true; }

                if (refresh)
                {
                    playlist.Items = await M3UParser.Parse(new Uri(playlist.Uri));
                }
                else
                {
                    playlist.Items = await M3UParser.Parse(new Uri($"file://{file.FullName.Replace(Path.DirectorySeparatorChar, '/')}"));
                }

                if (applyFilters && playlist.Filters.Count > 0)
                {
                    await ApplyFiltersAsync(playlist);
                }
                if (playlist.Id > 0)
                {
                    await WriteToDisk(config, playlist);
                }

                playlist.Count = playlist.Items.Count;

                return playlist;
            }
            public static async Task<FileInfo> WriteToDisk(IConfiguration config, Database.Models.M3U playlist)
            {
                var file = GetFileInfo(config, playlist);
                await WriteToDisk(file, playlist);
                return file;
            }
            public static async Task<FileInfo> WriteToDisk(FileInfo file, Database.Models.M3U playlist)
            {
                var content = await M3UParser.Create(playlist.Items);
                await File.WriteAllTextAsync(file.FullName, content.ToString());
                return file;
            }
            public static async Task ApplyFiltersAsync(Database.Models.M3U playlist)
            {
                playlist.Items = await FilterHelper.DoFilter(playlist.Items, playlist.Filters);
            }
            public static async Task<FileInfo> Generate(IConfiguration config, List<Database.Models.Mapping> mappings)
            {
                var dataFolder = config.GetValue<string>("DATA_FOLDER");

                var dir = new DirectoryInfo(dataFolder);
                if (!dir.Exists)
                {
                    dir.Create();
                }
                var outputDir = new DirectoryInfo(Path.Combine(dir.FullName, "output"));
                if (!outputDir.Exists)
                {
                    outputDir.Create();
                }

                var fileInfo = new FileInfo(Path.Combine(outputDir.FullName, $"playlist.m3u"));

                var playlist = new Database.Models.M3U();


                var playlists = new Dictionary<long, List<M3UItem>>();
                var guides = new Dictionary<long, Dictionary<string, XMLTVItem>>();

                long previous = 0;
                foreach (var g in mappings.GroupBy(m => m.Channel))
                {
                    var list = g.OrderBy(g => g.Name).ToArray();
                    previous = Math.Max(g.Key, previous);
                    for (long i = 0; i < list.LongLength; i++)
                    {
                        var m = list[i];
                        if (m?.M3UId != null)
                        {
                            if (m.M3UId.HasValue && !playlists.ContainsKey(m.M3UId.Value))
                            {
                                playlists[m.M3UId.Value] = (await Populate(config, new Database.Models.M3U { Id = m.M3UId.Value })).Items;
                            }
                            var playlistItems = playlists[m.M3UId.Value].ToList();
                            if (m.XMLTVItem?.XMLTVId == long.MaxValue)
                            {
                                var items = await FilterHelper.DoFilter(playlistItems, m.Filters);
                                var padding = (items.Count() - 1).ToString().Length;
                                for (var j = 0; j < items.Count; j++)
                                {
                                    var m3uItem = items[j];
                                    var channelid = $"{m.Id}.{j}";
                                    var title = DummyChannel.GetTitleOutput(m3uItem.ToString(), m);
                                    if (m.DummyMapping.IncludeBlank || !string.IsNullOrWhiteSpace(title))
                                    {
                                        var chno = j + 1;
                                        var chnoString = chno.ToString();
                                        while (chnoString.Length < padding)
                                        {
                                            chnoString = "0" + chnoString;
                                        }
                                        var chname = $"{m.Name} {chnoString}";
                                        var icon = m3uItem.Data.FirstOrDefault(d => d.Key == "tvg-logo")?.Value ?? string.Empty;
                                        var channelname = string.Empty;
                                        playlist.Items.Add(new M3UItem
                                        {
                                            Url = m3uItem.Url,
                                            Data = [
                                                new M3UItemData { Key = "tvg-id", Value = channelid },
                                                new M3UItemData { Key = "tvg-name", Value = chname },
                                                new M3UItemData { Key = "group-title", Value = m.Category },
                                                new M3UItemData { Key = "tvg-logo", Value = m.XMLTVItem?.Logo ?? m3uItem.Data.FirstOrDefault(d => d.Key == "tvg-logo")?.Value ?? string.Empty },
                                                new M3UItemData { Key = "tvg-chno", Value = (previous++).ToString() },
                                                new M3UItemData { Key = "TrackTitle", Value = chname }
                                            ]
                                        });
                                    }
                                }
                            }
                            else if (m?.XMLTVItem != null)
                            {
                                var playlistItem = (await FilterHelper.DoFilter(playlistItems, m.Filters))?.FirstOrDefault();
                                if (playlistItem != null)
                                {
                                    playlist.Items.Add(new M3UItem
                                    {
                                        Url = playlistItem.Url,
                                        Data = [
                                            new M3UItemData { Key = "tvg-id", Value = m.Id.ToString() },
                                            new M3UItemData { Key = "tvg-name", Value = m.Name },
                                            new M3UItemData { Key = "group-title", Value = m.Category },
                                            new M3UItemData { Key = "tvg-logo", Value = m.XMLTVItem?.Logo ?? playlistItem.Data.FirstOrDefault(d => d.Key == "tvg-logo")?.Value ?? string.Empty },
                                            new M3UItemData { Key = "tvg-chno", Value = (previous++).ToString() },
                                            new M3UItemData { Key = "TrackTitle", Value = m.Name }
                                        ]
                                    });
                                }
                            }
                        }
                    }


                }

                var content = await M3UParser.Create(playlist.Items);

                await File.WriteAllTextAsync(fileInfo.FullName, content.ToString());

                return fileInfo;
            }
        }
        public static class XMLTV
        {
            public static async Task<Database.Models.XMLTV?> Update(IConfiguration config, IPTVRelayContext DB, long id, string? content = null)
            {
                var guide = await DB.XMLTV.Include(g => g.Items).ThenInclude(i => i.Mappings)
                    .FirstOrDefaultAsync(g => g.Id == id);
                if (guide == null) return null;

                if (content == null)
                {
                    content = await Populate(config, guide, refresh: true);
                }
                if (content == null) return null;
                var items = await Library.XMLTVParser.Parse(content);
                if (items == null) return null;

                var existingItems = guide.Items.Where(i => !string.IsNullOrWhiteSpace(i.ChannelId)).GroupBy(i => i.ChannelId!, i => i).ToDictionary(g => g.Key, g => g.ToList());
                var pendingItems = items.GroupBy(i => i.ChannelId, i => i).Where(i => !string.IsNullOrWhiteSpace(i.Key)).Select(g => g.First()).ToDictionary(i => i.ChannelId!, i => i);
                var newItems = pendingItems.Values.Where(i => !existingItems.ContainsKey(i.ChannelId!)).ToList();
                var missingItems = existingItems.Where(i => !pendingItems.ContainsKey(i.Key!)).ToList();

                var duplicateItems = existingItems.Where(g => g.Value.Count > 1)
                    .Select(g => g.Value.OrderByDescending(i => i.Mappings.Count()).ThenByDescending(i => i.Id))
                    .Select(g => new { Primary = g.First(), Duplicates = g.Skip(1).ToList() });

                foreach (var d in duplicateItems)
                {
                    foreach (var duplicate in d.Duplicates)
                    {
                        var mappings = duplicate.Mappings;
                        duplicate.Mappings.Clear();
                        foreach (var m in mappings)
                        {
                            m.XMLTVItemId = d.Primary.Id;
                            d.Primary.Mappings.Add(m);
                        }
                        DB.Remove(duplicate);
                        guide.Items.Remove(duplicate);
                    }
                }

                foreach (var newItem in newItems)
                {
                    newItem.XMLTVId = guide.Id;
                    guide.Items.Add(newItem);
                }

                var file = Utility.XMLTV.GetFileInfo(config, guide);
                File.WriteAllText(file.FullName, content);

                await DB.SaveChangesAsync();

                return guide;
            }

            public static FileInfo GetFileInfo(IConfiguration config, Database.Models.XMLTV guide)
            {
                var dataFolder = config.GetValue<string>("DATA_FOLDER");

                var dir = new DirectoryInfo(dataFolder);
                if (!dir.Exists)
                {
                    dir.Create();
                }
                var xmltvDir = new DirectoryInfo(Path.Combine(dir.FullName, "xmltv"));
                if (!xmltvDir.Exists)
                {
                    xmltvDir.Create();
                }

                var fileInfo = new FileInfo(Path.Combine(xmltvDir.FullName, $"{guide.Id}.xml"));

                return fileInfo;
            }
            public static async Task<string?> Populate(IConfiguration config, Database.Models.XMLTV guide, bool refresh = false, bool applyFilters = true)
            {
                if (guide.Id == long.MaxValue) return null;
                var file = GetFileInfo(config, guide);
                if (!file.Exists) { refresh = true; }
                if (refresh)
                {
                    return await XMLTVParser.Parse(new Uri(guide.Uri));
                }
                else
                {
                    return await XMLTVParser.Parse(new Uri($"file://{file.FullName.Replace(Path.DirectorySeparatorChar, '/')}"));
                }
            }
            public static async Task<FileInfo> WriteToDisk(IConfiguration config, Database.Models.XMLTV guide, string content)
            {
                var file = GetFileInfo(config, guide);
                await File.WriteAllTextAsync(file.FullName, content);
                return file;
            }
            public static async Task<List<XMLTVItem>> ApplyFiltersAsync(Database.Models.XMLTV guide)
            {
                var items = guide.Items.ToList();
                //foreach (var f in guide.Filters)
                {
                    var chunks = items.Select((Item, Index) => new { Item, Index }).Chunk(Environment.ProcessorCount);
                    var bag = new ConcurrentBag<XMLTVItem>();
                    foreach (var chunk in chunks)
                    {
                        await Task.WhenAll(chunk.Select(p => Task.Run(() =>
                        {
                            //var filtered = FilterHelper.DoFilter(p.Item, p.Index, items.Count, f);
                            //if (filtered) bag.Add(p.Item);
                        })));
                    }
                    foreach (var item in bag)
                    {
                        items.Remove(item);
                    }
                }
                return items;
            }
            public static async Task<FileInfo> Generate(IConfiguration config, List<Database.Models.Mapping> mappings)
            {
                var dataFolder = config.GetValue<string>("DATA_FOLDER");

                var dir = new DirectoryInfo(dataFolder);
                if (!dir.Exists)
                {
                    dir.Create();
                }
                var outputDir = new DirectoryInfo(Path.Combine(dir.FullName, "output"));
                if (!outputDir.Exists)
                {
                    outputDir.Create();
                }

                var fileInfo = new FileInfo(Path.Combine(outputDir.FullName, $"guide.xml"));

                var channelsXml = new List<string>();
                var programmingXml = new List<string>();


                var playlists = new Dictionary<long, Database.Models.M3U>();
                var guides = new Dictionary<long, XmlDocument>();

                foreach (var g in mappings.GroupBy(m => m.Channel))
                {
                    var list = g.OrderBy(g => g.Name).ToArray();
                    for (long i = 0; i < list.LongLength; i++)
                    {
                        var m = list[i];
                        if (m != null)
                        {
                            var id = m?.XMLTVItem?.XMLTVId;
                            var doc = new XmlDocument();
                            if (id == null)
                            {
                                Console.WriteLine($"Something Went wrong with {m.Name}: {m.XMLTVItem?.ChannelId}");
                                continue;
                            }
                            List<string>? channelNodes = [];
                            List<string>? programmesNodes = [];
                            if (id == long.MaxValue)
                            {
                                Database.Models.M3U m3u;
                                if (m.M3UId.HasValue && !playlists.ContainsKey(m.M3UId.Value))
                                {
                                    m3u = await M3U.Populate(config, new Database.Models.M3U { Id = m.M3UId.Value });
                                    playlists[id.Value] = m3u;
                                }
                                else
                                {
                                    m3u = playlists[m.M3UId.Value];
                                }

                                var items = await FilterHelper.DoFilter(m3u.Items, m.Filters);
                                var padding = (items.Count() - 1).ToString().Length;

                                for (var j = 0; j < items.Count; j++)
                                {
                                    var m3uItem = items[j];
                                    var channelid = $"{m.Id}.{j}";

                                    var title = DummyChannel.GetTitleOutput(m3uItem.ToString(), m);
                                    var start = DummyChannel.GetTimeOutput(m3uItem.ToString(), m.TimeOffsetSpan, m);

                                    if (m.DummyMapping.IncludeBlank || start.HasValue)
                                    {
                                        var chno = j + 1;
                                        var chnoString = chno.ToString();
                                        while (chnoString.Length < padding)
                                        {
                                            chnoString = "0" + chnoString;
                                        }
                                        var chname = $"{m.Name} {chnoString}";
                                        var icon = m3uItem.Data.FirstOrDefault(d => d.Key == "tvg-logo")?.Value ?? string.Empty;

                                        channelNodes.Add(@$"<channel id=""{channelid}""><display-name>{chname}</display-name><icon src=""{icon}"" height=""270"" width=""360""></icon></channel>");

                                    }
                                    var pastdate = $"{DateTime.Today.AddDays(-1):yyyyMMddHHmm00 +0000}";
                                    var futuredate = $"{DateTime.Today.AddDays(2):yyyyMMddHHmm00 +0000}";
                                    var offair = $"Off Air";
                                    if (!string.IsNullOrWhiteSpace(title) && start.HasValue)
                                    {
                                        var local = new DateTime(start.Value.Year, start.Value.Month, start.Value.Day, start.Value.Hour, start.Value.Minute, 0, DateTimeKind.Local);
                                        var utc = local.ToUniversalTime();

                                        var startdate = $"{utc:yyyyMMddHHmm00 +0000}";
                                        var duration = 30;
                                        _ = int.TryParse(m?.XMLTVItem?.ChannelId?.Replace("Dummy.", string.Empty), out duration);
                                        var enddate = $"{utc.AddMinutes(duration):yyyyMMddHHmm00 +0000}";

                                        programmesNodes.Add(@$"<programme channel=""{channelid}"" start=""{pastdate}"" stop=""{startdate}""><title lang=""en"">{$"{offair} until {local:hh:mm} - {title}"}</title></programme>");
                                        programmesNodes.Add(@$"<programme channel=""{channelid}"" start=""{startdate}"" stop=""{enddate}""><title lang=""en"">{title}</title></programme>");
                                        programmesNodes.Add(@$"<programme channel=""{channelid}"" start=""{enddate}"" stop=""{futuredate}""><title lang=""en"">{offair}</title></programme>");
                                    }
                                    else if (m.DummyMapping.IncludeBlank)
                                    {
                                        programmesNodes.Add(@$"<programme channel=""{channelid}"" start=""{pastdate}"" stop=""{futuredate}""><title lang=""en"">{offair}</title></programme>");
                                    }
                                }
                            }
                            else if (m?.XMLTVItem != null)
                            {
                                if (id.HasValue && !guides.ContainsKey(id.Value))
                                {
                                    var xml = await Populate(config, new Database.Models.XMLTV { Id = id.Value });
                                    doc.LoadXml(xml);
                                    guides[id.Value] = doc;
                                }
                                else
                                {
                                    doc = guides[id.Value];
                                }

                                var element = (doc.SelectSingleNode($"/tv/channel[@id=\"{m.XMLTVItem.ChannelId}\"]") as XmlElement);
                                element?.SetAttribute("id", m.Id.ToString());

                                channelNodes.Add(element?.OuterXml);
                                programmesNodes.AddRange(doc.SelectNodes($"/tv/programme[@channel=\"{m.XMLTVItem.ChannelId}\"]").Cast<XmlElement>().Select(n =>
                                {
                                    n.SetAttribute("channel", m.Id.ToString());
                                    return n.OuterXml;
                                }));
                            }
                            if (channelNodes != null && programmesNodes != null)
                            {

                                channelsXml.AddRange(channelNodes);
                                programmingXml.AddRange(programmesNodes);
                            }


                        }
                    }
                }

                var content = @$"<?xml version=""1.0"" encoding=""UTF-8""?>
  <tv generator-info-name=""IPTVRelay"" source-info-name=""IPTVRelay"">
    {string.Join(Environment.NewLine, channelsXml)}
    {string.Join(Environment.NewLine, programmingXml)}
  </tv>
";

                await File.WriteAllTextAsync(fileInfo.FullName, content.ToString());

                return fileInfo;
            }
        }

        public static class DummyChannel
        {
            static readonly string[] MINUTE = new[] { "m", "mm" };
            static readonly string[] HOUR = new[] { "H", "HH", "h", "hh" };
            static readonly string[] AMPM = new[] { "t", "tt" };
            static readonly string[] DAY = new[] { "d", "dd" };
            static readonly string[] MONTH = new[] { "M", "MM", "MMM", "MMMM" };
            static readonly string[] YEAR = new[] { "y", "yy", "yyyy" };

            public static Match? CheckExpression(string expression, string value)
            {
                try
                {
                    var match = Regex.Match(value, expression, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        return match;
                    }
                }
                catch { }
                return null;
            }
            public static DateTime? GetTimeOutput(string input, TimeSpan offset, Mapping mapping)
            {
                var match = CheckExpression(mapping.DummyMapping.TimeExpression, input);
                if (match?.Success ?? false)
                {
                    var formatParts = new List<string>();
                    var valueParts = new List<string>();

                    var date = new[] { YEAR, MONTH, DAY, }.Reverse();
                    var anyfound = false;
                    foreach (var group in date)
                    {
                        var found = false;
                        foreach (var key in group)
                        {
                            if (match?.Groups?.ContainsKey(key) ?? false)
                            {
                                formatParts.Add(key);
                                valueParts.Add(match!.Groups[key].Value);
                                found = true;
                                anyfound = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            formatParts.Add(group.First());
                            valueParts.Add(DateTime.Now.ToString($"%{group.First()}", CultureInfo.CurrentCulture));
                        }
                    }
                    var time = new[] { HOUR, MINUTE, AMPM };
                    foreach (var group in time)
                    {
                        foreach (var key in group)
                        {
                            if (match?.Groups?.ContainsKey(key) ?? false)
                            {
                                formatParts.Add(key);
                                valueParts.Add(match!.Groups[key].Value);
                                anyfound = true;
                                break;
                            }
                        }
                    }
                    if(!anyfound) return null;
                    var value = string.Join(" ", valueParts);
                    var format = string.Join(" ", formatParts);

                    if (DateTime.TryParseExact(value, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out var datetime))
                    {
                        var actual = datetime.Add(offset);
                        return actual;
                    }
                }
                return null;
            }
            public static string? GetTitleOutput(string input, Mapping mapping)
            {
                try
                {
                    var match = CheckExpression(mapping.DummyMapping.TitleExpression, input);
                    if (match?.Success ?? false)
                    {
                        var output = mapping.DummyMapping.TitleFormat ?? string.Empty;
                        var matches = Regex.Matches(output, @"(?<!{){(?!={)(?<key>[^{}]+)(?<!})}(?!=})", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase).ToList().OrderByDescending(m => m.Index);

                        foreach (var m in matches)
                        {
                            if (m.Success)
                            {
                                var value = m.Value;
                                var key = m.Groups["key"].Value;

                                string? replaceValue = null;
                                if (int.TryParse(key, out var index))
                                {
                                    if (index < match.Groups.Count)
                                    {
                                        replaceValue = match.Groups[index].Value;
                                    }
                                }
                                else
                                {
                                    if (match.Groups.ContainsKey(key))
                                    {
                                        replaceValue = match.Groups[key].Value;
                                    }
                                }
                                if (replaceValue is not null)
                                {
                                    output = output.Remove(m.Index, value.Length);
                                    output = output.Insert(m.Index, replaceValue);
                                }
                            }
                        }

                        return output;
                    }
                }
                catch { }
                return null;
            }
        }

        public static class Jobs
        {
            public class UpdateJob
            {
                IConfiguration Config;
                IPTVRelayContext DB;

                public UpdateJob(IConfiguration config, IPTVRelayContext db)
                {
                    Config = config;
                    DB = db;
                }

                public async Task Update()
                {
                    {
                        var playlists = await DB.M3U.Include(p => p.Filters).AsNoTracking().ToListAsync();

                        await Task.WhenAll(playlists.Where(p => p.ParentId.GetValueOrDefault() == 0).Select(p => PopulateM3URecursive(p, playlists)));

                        await DB.SaveChangesAsync();
                    }
                    DB.ChangeTracker.Clear();
                    GC.Collect();
                    {
                        var guides = await DB.XMLTV.Include(g => g.Items).ThenInclude(i => i.Mappings).AsNoTracking().ToListAsync();
                        foreach (var guide in guides)
                        {
                            await XMLTV.Update(Config, DB, guide.Id);
                        }
                    }
                    DB.ChangeTracker.Clear();
                    GC.Collect();
                    {
                        var mappings = await DB.Mapping
                            .Include(m => m.XMLTVItem)
                            .Include(m => m.M3U)
                            .Include(m => m.Filters)
                            .Include(m => m.DummyMapping)
                            .OrderBy(m => m.Channel).ThenBy(m => m.Name)
                            .AsNoTracking().ToListAsync();

                        await M3U.Generate(Config, mappings);
                        await XMLTV.Generate(Config, mappings);
                    }
                    DB.ChangeTracker.Clear();
                    GC.Collect();
                }
                private async Task PopulateM3URecursive(Database.Models.M3U playlist, List<Database.Models.M3U> playlists)
                {
                    await M3U.Populate(Config, playlist, refresh: true, applyFilters: true);

                    await Task.WhenAll(playlists.Where(p => p.ParentId == playlist.Id).Select(p => PopulateM3URecursive(p, playlists)));
                }

            }
        }
    }
}
