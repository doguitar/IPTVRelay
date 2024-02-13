using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml;
using IPTVRelay.Database.Models;
using Microsoft.Extensions.Options;

namespace IPTVRelay.Library
{
    public static class XMLTVParser
    {
        public static async Task<string?> Parse(Uri uri)
        {
            using var response = await UriClient.GetAsync(uri);
            if (!(response?.IsSuccess ?? default)) return null;            
            using var stream = await response!.ReadAsStreamAsync(); 
            StreamReader reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            return content;
        }
        public static async Task<List<XMLTVItem>?> Parse(string content)
        {
            var doc = new XmlDocument();
            doc.LoadXml(content);

            var channels = doc.SelectNodes("/tv/channel");

            var bag = new ConcurrentDictionary<string, XMLTVItem>();
            if ((channels?.Count > 0))
            {

                await Task.WhenAll(
                    channels.OfType<XmlNode>().Select(c => Task.Run(() =>
                    {
                        var id = c?.Attributes?.GetNamedItem("id")?.Value;
                        if (!string.IsNullOrEmpty(id))
                        {
                            var item = new XMLTVItem() { ChannelId = id };
                            var items = c?.ChildNodes?.Cast<XmlNode>()
                                    ?.Select(n => new { n?.Name, n?.InnerText })
                                    ?.Where(i => !string.IsNullOrWhiteSpace(i?.InnerText))
                                    ?.Select(i => new XMLTVItemData { Key = i?.Name, Value = i?.InnerText })?.ToList();

                            if (items?.Count > 0)
                            {
                                item.Data = items;
                            }
                            bag.AddOrUpdate(id, item, (key, x) => { x.Data.AddRange(item.Data); return x; });

                            var url = c?.SelectSingleNode("icon")?.Attributes?.GetNamedItem("src")?.Value;
                            if (url != null)
                            {
                                item.Logo = url;
                            }
                        }
                    })).ToArray());

                return bag.Values.ToList();
            }


            return null;
        }
    }
}