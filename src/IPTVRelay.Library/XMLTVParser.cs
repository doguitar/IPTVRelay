using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml;
using IPTVRelay.Database.Models;

namespace IPTVRelay.Library
{
    public static class XMLTVParser
    {
        public static async Task<List<XMLTVItem>?> Parse(Uri uri)
        {
            using var response = await UriClient.GetAsync(uri);

            if (!(response?.IsSuccess ?? default)) return null;

            using var stream = await response!.ReadAsStreamAsync();
            var doc = new System.Xml.XmlDocument();
            doc.Load(stream);

            var channels = doc.SelectNodes("/tv/channel");

            var bag = new ConcurrentBag<XMLTVItem>();
            if ((channels?.Count > 0))
            {

                await Task.WhenAll(
                    channels.OfType<XmlNode>().Select(c => Task.Run(() =>
                    {
                        var id = c?.Attributes?.GetNamedItem("id")?.Value;
                        if (!string.IsNullOrEmpty(id))
                        {
                            var item = new XMLTVItem() { ChannelId = id };
                            bag.Add(item);
                            item.Data = c?.ChildNodes?.Cast<XmlNode>()
                                .Select(n => new { n.Name, n.InnerText })
                                .Where(i => !string.IsNullOrWhiteSpace(i.InnerText))
                                .Select(i=> new XMLTVItemData { Key = i.Name, Value = i.InnerText }).ToList();

                            var url = c?.SelectSingleNode("icon")?.Attributes?.GetNamedItem("src")?.Value;
                            if (url != null)
                            {
                                item.Logo = url;
                            }
                        }
                    })).ToArray());

                return bag.ToList();
            }


            return null;
        }
    }
}