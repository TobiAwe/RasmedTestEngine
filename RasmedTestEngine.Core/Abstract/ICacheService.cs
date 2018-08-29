using System;

namespace RasmedTestEngine.Core.Abstract
{
      interface ICacheService
    {
        T GetOrSet<T>(string cacheKey, Func<T> getItemCallback) where T : class;
    }
}
