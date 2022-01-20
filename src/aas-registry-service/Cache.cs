using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace AAS.API.Registry
{
    public class Cache
    {
        private readonly IDistributedCache _cache;

        public Cache(IDistributedCache aCache)
        {
            _cache = aCache;
        }

        public async Task<T> Get<T>(string key)  where T : class
        {
            var cachedResponse = await _cache.GetStringAsync(key);
            return cachedResponse == null ? null : JsonConvert.DeserializeObject<T>(cachedResponse, 
                new JsonSerializerSettings() { 
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
        }

        public async Task Set<T>(string key, T value, DistributedCacheEntryOptions options) where T : class
        {
            var response = JsonConvert.SerializeObject(value,new JsonSerializerSettings() {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            await _cache.SetStringAsync(key, response, options);
        }

        public async Task Clear(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
