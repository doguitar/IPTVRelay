using IPTVRelay.Blazor.Components.Pages;
using IPTVRelay.Database.Models;
using IPTVRelay.Library;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Collections.Concurrent;
using System.Linq;
using System.Xml;

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
                    WriteToDisk(config, playlist);
                }
                else
                {
                    playlist.Items = await M3UParser.Parse(new Uri($"file:\\{file.FullName.Replace(Path.DirectorySeparatorChar, '/')}"));
                }
                if (applyFilters && playlist.Filters.Count > 0)
                {
                    playlist.Items = await ApplyFiltersAsync(playlist);
                }

                return playlist;
            }
            public static FileInfo WriteToDisk(IConfiguration config, Database.Models.M3U playlist)
            {
                var file = GetFileInfo(config, playlist);
                WriteToDisk(file, playlist);
                return file;
            }
            public static FileInfo WriteToDisk(FileInfo file, Database.Models.M3U playlist)
            {
                return file;
            }
            public static async Task<List<M3UItem>> ApplyFiltersAsync(Database.Models.M3U playlist)
            {
                var items = playlist.Items.ToList();
                foreach (var f in playlist.Filters)
                {
                    var chunks = items.Select((Item, Index) => new { Item, Index }).Chunk(Environment.ProcessorCount);
                    var bag = new ConcurrentBag<M3UItem>();
                    foreach (var chunk in chunks)
                    {
                        await Task.WhenAll(chunk.Select(p => Task.Run(() =>
                        {
                            var filtered = FilterHelper.DoFilter(p.Item, p.Index, items.Count, f);
                            if (filtered) bag.Add(p.Item);
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

                var fileInfo = new FileInfo(Path.Combine(outputDir.FullName, $"playlist.m3u"));

                var playlist = new Database.Models.M3U();


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
                        if (m != null)
                        {
                            if (m.M3UId.HasValue && !playlists.ContainsKey(m.M3UId.Value))
                            {
                                playlists[m.M3UId.Value] = (await Populate(config, new Database.Models.M3U { Id = m.M3UId.Value })).Items;
                            }
                            var playlistItems = playlists[m.M3UId.Value].ToList();

                            foreach (var f in m.Filters)
                            {
                                for (var j = 0; j < playlistItems.Count; j++)
                                {
                                    if (FilterHelper.DoFilter(playlistItems[j], j, playlistItems.Count, f))
                                    {
                                        playlistItems.RemoveAt(j);
                                        j--;
                                    }
                                }
                            }
                            if (playlistItems.Count > 0)
                            {
                                playlist.Items.Add(new M3UItem
                                {
                                    Url = playlistItems.First().Url,
                                    Data = [
                                        new M3UItemData { Key = "tvg-id", Value = m.Id.ToString() },
                                        new M3UItemData { Key = "tvg-name", Value = m.Name },
                                        new M3UItemData { Key = "tvg-logo", Value = m.XMLTVItem?.Logo ?? playlistItems.First().Data.FirstOrDefault(d => d.Key == "tvg-logo")?.Value ?? string.Empty },
                                        new M3UItemData { Key = "tvg-chno", Value = m.Channel.ToString() },
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
            public static async Task<string> Populate(IConfiguration config, Database.Models.XMLTV guide, bool refresh = false, bool applyFilters = true)
            {
                var file = GetFileInfo(config, guide);
                if (!file.Exists) { refresh = true; }
                if (refresh)
                {
                    return await XMLTVParser.Parse(new Uri(guide.Uri));
                }
                else
                {
                    return await XMLTVParser.Parse(new Uri($"file:\\{file.FullName.Replace(Path.DirectorySeparatorChar, '/')}"));
                }
            }
            public static FileInfo WriteToDisk(IConfiguration config, Database.Models.XMLTV guide)
            {
                var file = GetFileInfo(config, guide);
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

                            var channelNode = doc.SelectSingleNode($"/tv/channel[@id=\"{m.XMLTVItem.ChannelId}\"]") as XmlElement;
                            var programmesNodes = doc.SelectNodes($"/tv/programme[@channel=\"{m.XMLTVItem.ChannelId}\"]");


                            channelNode.SetAttribute("id", m.Id.ToString());

                            channelsXml.Add(channelNode.OuterXml);
                            programmingXml.AddRange(programmesNodes.Cast<XmlElement>().Select(n =>
                            {
                                n.SetAttribute("channel", m.Id.ToString());
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
    }
}
