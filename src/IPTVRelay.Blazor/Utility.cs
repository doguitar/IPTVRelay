using Hangfire.Dashboard;
using IPTVRelay.Blazor.Components.Pages;
using IPTVRelay.Blazor.Components.Shared;
using IPTVRelay.Blazor.Configuration;
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
using Updogg;
using static IPTVRelay.Blazor.Utility;

namespace IPTVRelay.Blazor
{
    public class Utility
    {
        public class M3uUtility
        {
            public IPTVConfig Config { get; }

            public M3uUtility(IPTVConfig config)
            {
                Config = config;
            }

            public FileInfo GetFileInfo(M3U playlist)
            {
                var dataFolder = Config.DataFolder;

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
            public async Task<M3U> Populate(M3U playlist, bool refresh = false, bool applyFilters = true)
            {
                var file = GetFileInfo(playlist);
                if (!file.Exists) { refresh = true; }

                if (refresh)
                {
                    if (playlist.ParentId.HasValue)
                    {
                        var parentFile = GetFileInfo(new M3U { Id = playlist.ParentId.Value });
                        playlist.Uri = new Uri($"file://{parentFile.FullName.Replace(Path.DirectorySeparatorChar, '/')}").ToString();
                    }
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
                    await WriteToDisk(playlist);
                }

                playlist.Count = playlist.Items.Count;

                return playlist;
            }
            public async Task<FileInfo> WriteToDisk(M3U playlist)
            {
                var file = GetFileInfo(playlist);
                await WriteToDisk(file, playlist);
                return file;
            }
            public async Task<FileInfo> WriteToDisk(FileInfo file, M3U playlist)
            {
                var content = await M3UParser.Create(playlist.Items);
                await File.WriteAllTextAsync(file.FullName, content.ToString());
                return file;
            }
            public async Task ApplyFiltersAsync(M3U playlist)
            {
                playlist.Items = await FilterHelper.DoFilter(playlist.Items, playlist.Filters);
            }
            public async Task<FileInfo> Generate(List<Mapping> mappings)
            {
                var dataFolder = Config.DataFolder;

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

                var playlist = new M3U();


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
                                playlists[m.M3UId.Value] = (await Populate(new M3U { Id = m.M3UId.Value })).Items;
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
        public class XmlTvUtility
        {
            public IPTVConfig Config { get; }
            public M3uUtility M3UUtility { get; }

            public XmlTvUtility(IPTVConfig config, M3uUtility m3UUtility)
            {
                Config = config;
                M3UUtility = m3UUtility;
            }

            public void Merge(string content, XMLTV guide, out List<XMLTVItem> removed, out List<XMLTVItem> added)
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

            public FileInfo GetFileInfo(XMLTV guide)
            {
                var dataFolder = Config.DataFolder;

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
            public async Task<string> Populate(XMLTV guide, bool refresh = false, bool applyFilters = true)
            {
                if (guide.Id == long.MaxValue) return null;
                var file = GetFileInfo(guide);
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
            public async Task<FileInfo> WriteToDisk(XMLTV guide, string content)
            {
                var file = GetFileInfo(guide);
                await File.WriteAllTextAsync(file.FullName, content);
                return file;
            }
            public async Task<List<XMLTVItem>> ApplyFiltersAsync(XMLTV guide)
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
            public async Task<FileInfo> Generate(List<Mapping> mappings)
            {
                var dataFolder = Config.DataFolder;

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


                var playlists = new Dictionary<long, M3U>();
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
                            if (m.DummyMappingId > 0)
                            {
                                M3U m3u;
                                if (m.M3UId.HasValue && !playlists.ContainsKey(m.M3UId.Value))
                                {
                                    m3u = await M3UUtility.Populate(new M3U { Id = m.M3UId.Value });
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
                                    var xml = await Populate(new XMLTV { Id = id.Value });
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
                    var match = Regex.Match(value, expression, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
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
                    var groups = match?.Groups.Values.Skip(1)
                        .Where(g => !g.Name.IsNullOrWhiteSpace())
                        .OrderBy(g => g.Index);

                    var formatString = string.Join(string.Empty, groups
                        .Select(g => g.Name));

                    var output = string.Format(formatString, groups.Select(g => g.Value));

                    if (groups.TryFirst(g => g.Name.StartsWith("h"), out var g) && int.TryParse(g?.Value, out var hour))
                    {
                        if (groups.TryFirst(g => g.Name.StartsWith("m"), out g) && int.TryParse(g?.Value, out var minute))
                        {
                            if (groups.TryFirst(g => g.Name.StartsWith("t"), out g) && g.Value.StartsWith("p"))
                            {
                                hour += 12;
                            }
                            output = $"{hour:00}:{minute:00}";
                        }
                    }

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

        public class Jobs
        {
            public IPTVConfig Config { get; }
            public XmlTvUtility XmlTvUtility { get; }
            public M3uUtility M3uUtility { get; }

            public Jobs(IPTVConfig config, XmlTvUtility xmlTvUtility, M3uUtility m3uUtility)
            {
                Config = config;
                XmlTvUtility = xmlTvUtility;
                M3uUtility = m3uUtility;
            }
            public class UpdateJob
            {
                IPTVConfig Config { get; }
                XmlTvUtility XmlTvUtility { get; }
                M3uUtility M3uUtility { get; }
                IPTVRelayContext DB { get; }

                public UpdateJob(IPTVConfig config, XmlTvUtility xmlTvUtility, M3uUtility m3uUtility, IPTVRelayContext db)
                {
                    Config = config;
                    XmlTvUtility = xmlTvUtility;
                    M3uUtility = m3uUtility;
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
                            XMLTV newGuide = new();
                            string content = string.Empty;
                            try
                            {
                                content = await XmlTvUtility.Populate(guide, refresh: true, applyFilters: true);
                                newGuide.Items = await XMLTVParser.Parse(content);
                            }
                            catch { continue; }
                            var removed = guide.Items.ExceptBy(newGuide.Items.Select(i => i.ChannelId), i => i.ChannelId).ToList();
                            var added = newGuide.Items.ExceptBy(guide.Items.Select(i => i.ChannelId), x => x.ChannelId).ToList();

                            if (removed.Any() || added.Any())
                            {
                                guide.Items.RemoveAll(i => removed.Any(r => r.ChannelId == i.ChannelId));
                                DB.RemoveRange(removed);
                                guide.Items.AddRange(added);
                                added.ForEach(a => a.XMLTVId = guide.Id);
                                await DB.SaveChangesAsync();
                            }

                            await XmlTvUtility.WriteToDisk(guide, content);
                        }
                    }
                    DB.ChangeTracker.Clear();
                    {
                        var mappings = await DB.Mapping
                            .Include(m => m.XMLTVItem)
                            .ThenInclude(x => x.XMLTV)
                            .Include(m => m.M3U)
                            .Include(m => m.Filters)
                            .Include(m => m.DummyMapping)
                            .OrderBy(m => m.Channel).ThenBy(m => m.Name)
                            .AsNoTracking().ToListAsync();

                        await M3uUtility.Generate(mappings);
                        await XmlTvUtility.Generate(mappings);
                    }
                    DB.ChangeTracker.Clear();

                }
                private async Task PopulateM3URecursive(M3U playlist, List<M3U> playlists)
                {
                    await M3uUtility.Populate(playlist, refresh: true, applyFilters: true);

                    await Task.WhenAll(playlists.Where(p => p.ParentId == playlist.Id).Select(p => PopulateM3URecursive(p, playlists)));
                }

            }
        }
    }
}

