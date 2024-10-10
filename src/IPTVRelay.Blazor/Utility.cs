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
using System.Xml;

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
                    var start = Math.Max(g.Key, previous);
                    for (long i = 0; i < list.LongLength; i++)
                    {
                        previous = g.Key + i;
                        var m = list[i];
                        if (m?.M3UId != null)
                        {
                            if (m.M3UId.HasValue && !playlists.ContainsKey(m.M3UId.Value))
                            {
                                playlists[m.M3UId.Value] = (await Populate(new M3U { Id = m.M3UId.Value })).Items;
                            }
                            var playlistItems = playlists[m.M3UId.Value].ToList();
                            var playlistItem = (await FilterHelper.DoFilter(playlistItems, m.Filters))?.FirstOrDefault();
                            if (playlistItem != null)
                            {
                                playlist.Items.Add(new M3UItem
                                {
                                    Url = playlistItem.Url,
                                    Data = [
                                        new M3UItemData { Key = "tvg-id", Value = previous.ToString() },
                                        new M3UItemData { Key = "tvg-name", Value = m.Name },
                                        new M3UItemData { Key = "group-title", Value = m.Category },
                                        new M3UItemData { Key = "tvg-logo", Value = m.XMLTVItem?.Logo ?? playlistItem.Data.FirstOrDefault(d => d.Key == "tvg-logo")?.Value ?? string.Empty },
                                        new M3UItemData { Key = "tvg-chno", Value = previous.ToString() },
                                        new M3UItemData { Key = "TrackTitle", Value = m.Name }
                                    ]
                                });
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

            public XmlTvUtility(IPTVConfig config)
            {
                Config = config;
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


                var playlists = new Dictionary<long, List<M3UItem>>();
                var guides = new Dictionary<long, XmlDocument>();

                long previous = 0;
                foreach (var g in mappings.GroupBy(m => m.Channel))
                {
                    var list = g.OrderBy(g => g.Name).ToArray();
                    var start = Math.Max(g.Key, previous);
                    for (long i = 0; i < list.LongLength; i++)
                    {
                        previous = g.Key + i;
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

                            var channelNode = doc.SelectSingleNode($"/tv/channel[@id=\"{m.XMLTVItem.ChannelId}\"]") as XmlElement;
                            var programmesNodes = doc.SelectNodes($"/tv/programme[@channel=\"{m.XMLTVItem.ChannelId}\"]");

                            if (channelNode == null)
                            {
                                Console.WriteLine($"{m.XMLTVItem?.ChannelId} not found in XMLTVId {m.XMLTVItem?.XMLTVId} for mapping {m.Name}:");
                                continue;
                            }

                            channelNode.SetAttribute("id", previous.ToString());

                            channelsXml.Add(channelNode.OuterXml);
                            programmingXml.AddRange(programmesNodes.Cast<XmlElement>().Select(n =>
                            {
                                n.SetAttribute("channel", previous.ToString());
                                return n.OuterXml;
                            }));


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
