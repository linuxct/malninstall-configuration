using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace space.linuxct.malninstall.Configuration.Common.Extensions
{
    public static class DistributedCacheExtensions
    {
        public static void SetObject(this IDistributedCache cache, string key, object value)
        {
            cache.SetString(key, JsonConvert.SerializeObject(value));
        }
        
        public static void SetTimedObject(this IDistributedCache cache, string key, object value, int hours)
        {
            cache.SetString(key, JsonConvert.SerializeObject(value), new DistributedCacheEntryOptions {AbsoluteExpiration = DateTimeOffset.Now.AddHours(hours)});
        }
        
        public static Task SetObjectAsync(this IDistributedCache cache, string key, object value)
        {
            return cache.SetStringAsync(key, JsonConvert.SerializeObject(value));
        }
        
        public static Task SetTimedObjectAsync(this IDistributedCache cache, string key, object value, int hours)
        {
            return cache.SetStringAsync(key, JsonConvert.SerializeObject(value), new DistributedCacheEntryOptions {AbsoluteExpiration = DateTimeOffset.Now.AddHours(hours)});
        }
        
        public static T GetObject<T>(this IDistributedCache cache, string key)
        {
            var value = cache.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }

}