using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml;
using IPTVRelay.Database.Models;
using Microsoft.Extensions.Options;
using Updogg;

namespace IPTVRelay.Library
{
    public static class XMLTVParser
    {
        public static async Task Parse(Uri uri, XMLTV guide)
        {
            using var response = await UriClient.GetAsync(uri);
            if (!(response?.IsSuccess ?? default)) return;
            using var stream = await response!.ReadAsStreamAsync();
            await Parse(stream, guide);
        }
        public static async Task Parse(string content, XMLTV guide)
        {
            await Parse(new MemoryStream(Encoding.UTF8.GetBytes(content)), guide);
        }
        public static async Task Parse(Stream stream, XMLTV guide)
        {
            using var streamReader = new StreamReader(stream);
            using var xmlReader = XmlReader.Create(streamReader, new XmlReaderSettings { Async = true, DtdProcessing = DtdProcessing.Ignore });


            var currentChannel = new XMLTVItem();
            string currentProgramme = string.Empty;

            var channelProgrammes = new Dictionary<string, List<string>>();
            var channels = new Dictionary<string, XMLTVItem>();
            var nodes = new Stack<string>();

            while (await xmlReader.ReadAsync())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Attribute:
                        {
                            var path = string.Join("/", nodes.Append(xmlReader.Name));
                            switch (path)
                            {
                                case "tv/programme/channel":
                                    {
                                        var id = xmlReader.Value;
                                    }
                                    break;
                                case "tv/channel/id":
                                    {
                                        var id = xmlReader.Value;
                                    }
                                    break;
                            }
                        }
                        break;
                    case XmlNodeType.Text:
                        {
                            var path = string.Join("/", nodes);
                            switch (path)
                            {
                            }
                        }
                        break;
                    case XmlNodeType.Element:
                        nodes.Push(xmlReader.Name);
                        switch (xmlReader.Name)
                        {
                            case "channel":
                                {
                                    XmlDocument doc = new XmlDocument();
                                    doc.LoadXml(await xmlReader.ReadOuterXmlAsync());

                                    var c = doc.FirstChild;

                                    var id = c?.Attributes?.GetNamedItem("id")?.Value;
                                    var name = c?.SelectSingleNode("display-name")?.Value;
                                    var logo = c?.SelectSingleNode("icon")?.Attributes?.GetNamedItem("id")?.Value;

                                    if (!string.IsNullOrWhiteSpace(id))
                                    {
                                        if (channels.TryGetValue(id, out var existing))
                                        {
                                            existing.Logo ??= logo;
                                            existing.DisplayName ??= name;
                                        }
                                        else
                                        {
                                            channels.Add(id, new XMLTVItem { ChannelId = id, Logo = logo, DisplayName = name });
                                        }
                                    }
                                }
                                break;
                            case "programme":
                                {
                                    XmlDocument doc = new XmlDocument();
                                    var programme = await xmlReader.ReadOuterXmlAsync();
                                    doc.LoadXml(programme);

                                    var c = doc.FirstChild;

                                    var id = c?.Attributes?.GetNamedItem("channel")?.Value;
                                    if (!string.IsNullOrWhiteSpace(id))
                                    {
                                        if (!channelProgrammes.TryGetValue(id, out var programmes))
                                        {
                                            programmes = [];
                                            channelProgrammes.Add(id, programmes);
                                        }
                                        programmes.Add(programme);
                                    }
                                }
                                break;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        nodes.Pop();
                        break;
                }
            }

            foreach (var kv in channels)
            {
                if (!(guide.Items.FirstOrDefault(i => i.ChannelId == kv.Key) is { } existing))
                {
                    guide.Items.Add(kv.Value);
                    existing = kv.Value;
                }
                else
                {
                    existing.DisplayName = kv.Value.DisplayName ?? existing.DisplayName;
                    existing.Logo = kv.Value.Logo ?? existing.Logo;
                }

                if (channelProgrammes.TryGetValue(kv.Key, out var programmes))
                {
                    existing.GuideData ??= new XMLTVGuideData();
                    existing.GuideData.GuideData = string.Join(Environment.NewLine, programmes);
                }
            }
        }
    }
}