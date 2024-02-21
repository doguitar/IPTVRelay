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
            public static void Merge(string content, Database.Models.XMLTV guide, out List<XMLTVItem> removed, out List<XMLTVItem> added)
            {
                removed = new List<XMLTVItem>();
                added = new List<XMLTVItem>();

                var items = XMLTVParser.Parse(content).GetAwaiter().GetResult();
                var dict = items.ToDictionary(i => i.ChannelId, i => i);
                for (var i = 0; i < guide.Items.Count; i++)
                {
                    if (!dict.ContainsKey(guide.Items[i].ChannelId))
                    {
                        removed.Add(guide.Items[i]);
                        guide.Items.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        items[i].Data.AddRange(dict[items[i].ChannelId].Data);
                    }
                }
                dict = guide.Items.ToDictionary(i => i.ChannelId, i => i);
                for (var i = 0; i < items.Count; i++)
                {
                    if (!dict.ContainsKey(items[i].ChannelId))
                    {
                        guide.Items.Add(items[i]);
                        added.Add(items[i]);
                    }
                    else
                    {
                        items[i].Data.AddRange(dict[items[i].ChannelId].Data);
                    }
                }
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
                                    var pastdate = $"{DateTime.Today.AddDays(-1):yyyyMMddhhmm00 +0000}";
                                    var futuredate = $"{DateTime.Today.AddDays(2):yyyyMMddhhmm00 +0000}";
                                    var offair = $"Off Air";
                                    if (!string.IsNullOrWhiteSpace(title) && start.HasValue)
                                    {
                                        start = new DateTime(start.Value.Year, start.Value.Month, start.Value.Day, start.Value.Hour, start.Value.Minute, 0, DateTimeKind.Local);
                                        
                                        var startdate = $"{start:yyyyMMddhhmm00 +0000}";   //20240209210000 +0000
                                        int.TryParse(m.XMLTVItem.ChannelId.Replace("Dummy.", string.Empty), out var duration);
                                        var enddate = $"{start.Value.AddMinutes(duration):yyyyMMddhhmm00 +0000}";     //20240209213000 +0000

                                        programmesNodes.Add(@$"<programme channel=""{channelid}"" start=""{pastdate}"" stop=""{startdate}""><title lang=""en"">{$"{offair} until {start.Value:hh:mm}"}</title></programme>");
                                        programmesNodes.Add(@$"<programme channel=""{channelid}"" start=""{startdate}"" stop=""{enddate}""><title lang=""en"">{title}</title></programme>");
                                        programmesNodes.Add(@$"<programme channel=""{channelid}"" start=""{enddate}"" stop=""{futuredate}""><title lang=""en"">{offair}</title></programme>");
                                    }
                                    else if(m.DummyMapping.IncludeBlank)
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
                    var output = match?.Groups[0]?.Value ?? string.Empty;

                    if (TimeOnly.TryParse(output, out var time))
                    {
                        var actual = DateTime.Now.Date.Add(time.ToTimeSpan()).Add(offset);
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
                                    if (match.Groups.ContainsKey(value))
                                    {
                                        replaceValue = match.Groups[value].Value;
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
                        var playlists = await DB.M3U.Include(p => p.Filters).ToListAsync();

                        await Task.WhenAll(playlists.Where(p => p.ParentId.GetValueOrDefault() == 0).Select(p => PopulateM3URecursive(p, playlists)));

                        await DB.SaveChangesAsync();
                    }
                    DB.ChangeTracker.Clear();
                    {
                        var guides = await DB.XMLTV.Include(g => g.Items).ThenInclude(i => i.Mappings).ToListAsync();
                        foreach (var guide in guides)
                        {
                            Database.Models.XMLTV newGuide = new();
                            string content = string.Empty;
                            try
                            {
                                content = await XMLTV.Populate(Config, guide, refresh: true, applyFilters: true);
                                newGuide.Items = await XMLTVParser.Parse(content);
                            }
                            catch { continue; }
                            var removed = guide.Items.Except(newGuide.Items, (x, y) => x.ChannelId == y.ChannelId, x => x.ChannelId.GetHashCode()).ToList();
                            var added = newGuide.Items.Except(guide.Items, (x, y) => x.ChannelId == y.ChannelId, x => x.ChannelId.GetHashCode()).ToList();

                            if (removed.Any() || added.Any())
                            {
                                guide.Items.RemoveAll(i => removed.Any(r => r.ChannelId == i.ChannelId));
                                DB.RemoveRange(removed);
                                guide.Items.AddRange(added);
                                added.ForEach(a => a.XMLTVId = guide.Id);
                                await DB.SaveChangesAsync();
                            }

                            await XMLTV.WriteToDisk(Config, guide, content);
                        }
                    }
                    DB.ChangeTracker.Clear();
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
