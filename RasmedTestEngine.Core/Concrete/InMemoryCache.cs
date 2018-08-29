using System;
using System.Runtime.Caching;
using RasmedTestEngine.Core.Abstract;

namespace RasmedTestEngine.Core.Concrete
{
    public  class InMemoryCache : ICacheService
    {
        public T GetOrSet<T>(string cacheKey, Func<T> getItemCallback) where T : class
        {
            var item = MemoryCache.Default.Get(cacheKey) as T;
            if (item == null)
            {
                item = getItemCallback();
                MemoryCache.Default.Add(cacheKey, item, DateTime.Now.AddMinutes(1));
            }
            return item;
        }
    }
}
