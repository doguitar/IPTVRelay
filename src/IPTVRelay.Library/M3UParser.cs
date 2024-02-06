using System.Text;
using IPTVRelay.Database.Models;

namespace IPTVRelay.Library
{
    public static class M3UParser
    {

        public static async Task<List<M3UItem>?> Parse(Uri uri)
        {
            using var response = await UriClient.GetAsync(uri);

            if (!(response?.IsSuccess ?? default)) return null;


            var items = new List<M3UItem>();
            using var stream = await response!.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                var item = new M3UItem();
                while (!reader.EndOfStream)
                {
                    if (ParseLine(uri, await reader.ReadLineAsync(), ref item))
                    {
                        items.Add(item);
                        break;
                    }
                }
            }
            return items;
        }

        private static bool ParseLine(Uri baseUri, string? line, ref M3UItem item)
        {
            if (line == null || item == null) return false;
            var queue = new Queue<char>(line);
            switch (queue.Peek())
            {
                case '#':
                    if (ParseComment(queue, out var data))
                    {
                        item.Data = data.Select(kv=>new M3UItemData { Key = kv.Key, Value = kv.Value }).ToList();
                    }
                    break;
                default:
                    if (ParseUrl(baseUri, queue, out var url))
                    {
                        item.Url = url; return true;
                    }

                    break;
            }
            return false;
        }

        private static bool ParseUrl(Uri baseUri, Queue<char> line, out string? url)
        {
            url = null;
            if (Uri.TryCreate(baseUri, new string(line.ToArray()), out var uri))
            {
                var builder = new UriBuilder(uri);
                builder.Query = baseUri.Query;
                url = builder.ToString();
                return true;
            }
            return false;
        }

        private static bool ParseComment(Queue<char> line, out Dictionary<string, string>? data)
        {
            data = null;
            var info = ReadUntil(line, c => c == ':');
            if (info == "#EXTINF")
            {
                data = new Dictionary<string, string>();
                _ = ReadUntil(line, c => c == ' ');
                _ = ReadUntil(line, c => c != ' ');
                while (line.Count > 0)
                {
                    var key = ReadUntil(line, c => c == '=');
                    line.TryDequeue(out _);
                    if(key.StartsWith(',') && line.Count == 0)
                    {
                        data.Add("TrackTitle", key.Substring(1));
                    }
                    else if (line.TryPeek(out var peeked) && peeked == '"')
                    {
                        line.TryDequeue(out _);
                        var value = ReadUntil(line, c => c == '"', '\\');
                        line.TryDequeue(out _);
                        data.Add(key, value);
                    }
                    else
                    {
                        var value = ReadUntil(line, c => c == ' ');
                        data.Add(key, value);

                    }
                    _ = ReadUntil(line, c => c != ' ');
                }
                return true;

            }
            return false;
        }

        private static string ReadUntil(Queue<char> line, Func<char, bool> test, char? escape = null)
        {
            var result = new StringBuilder();
            var escaped = false; ;
            while (line.TryPeek(out var next))
            {
                if (escaped || !(test?.Invoke(next) ?? default))
                {
                    if (next == escape)
                    {
                        escaped = true;
                    }
                    else
                    {
                        result.Append(next);
                        escaped = false;
                    }
                    _ = line.Dequeue();
                }
                else
                {
                    break;
                }
            }
            return result.ToString();
        }
    }
}
