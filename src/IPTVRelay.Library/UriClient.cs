using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Library
{
    internal class UriClient
    {
        public abstract class UriClientResponse : IDisposable
        {
            public abstract bool IsSuccess { get; }

            public abstract void Dispose();
            public abstract Task<Stream> ReadAsStreamAsync();
        }

        internal class HttpUriClientResponse : UriClientResponse
        {
            HttpResponseMessage _response;
            Stream _stream;

            public HttpUriClientResponse(HttpResponseMessage response)
            {
                _response = response;
            }

            public override bool IsSuccess => _response.IsSuccessStatusCode;

            public override void Dispose()
            {
                _stream?.Dispose();
            }

            public override async Task<Stream> ReadAsStreamAsync()
            {
                if (IsSuccess) _stream = await _response.Content.ReadAsStreamAsync();
                return _stream;
            }
        }

        internal class FileUriClientResponse : UriClientResponse
        {
            FileInfo _file;
            Stream _stream;
            public FileUriClientResponse(FileInfo file)
            {
                _file = file;
            }

            public override bool IsSuccess => _file.Exists;

            public override void Dispose()
            {
                _stream?.Dispose();
            }

            public override Task<Stream> ReadAsStreamAsync()
            {
                if (_stream == null && IsSuccess) _stream = _file.OpenRead();
                return Task.FromResult<Stream>(_stream);
            }
        }

        public static async Task<UriClientResponse?> GetAsync(Uri uri)
        {
            switch (uri.Scheme)
            {
                case "http":
                case "https":
                    HttpClient client = new HttpClient();
                    var result = await client.GetAsync(uri);
                    return new HttpUriClientResponse(result);
                case "file":
                    return new FileUriClientResponse(new FileInfo(uri.LocalPath));
            }

            return null;
        }
    }
}
