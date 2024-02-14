using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Library
{
    public class FileCache : Cache<string, string?, DateTime>
    {
        public FileCache() :
            base(
                (k) => Task.FromResult(new FileInfo(k).LastWriteTimeUtc),
                async (k) => File.Exists(k) ? (string?)await File.ReadAllTextAsync(k) : null)
        {
        }
    }
    public class Cache<K, V, S> where K : notnull
    {
        Dictionary<K, V> _cache = new Dictionary<K, V>();
        Dictionary<K, S> _state = new Dictionary<K, S>();

        Func<K, Task<S>> _stateGetter;
        Func<K, Task<V>> _valueGetter;

        public Cache(Func<K, Task<S>> stateGetter, Func<K, Task<V>> valueGetter)
        {
            _stateGetter = stateGetter;
            _valueGetter = valueGetter;
        }

        public async Task<V> GetAsync(K key)
        {
            var currentState = await _stateGetter.Invoke(key);
            if (_cache.TryGetValue(key, out var value) && _state.TryGetValue(key, out var state))
            {
                if (state?.Equals(currentState) ?? false)
                {
                    return value;
                }
            }
            value = await _valueGetter.Invoke(key);
            _state[key] = currentState;
            _cache[key] = value;

            return value;
        }

        public bool Remove(K key)
        {
            return _cache.Remove(key) && _state.Remove(key);
        }
    }
}
