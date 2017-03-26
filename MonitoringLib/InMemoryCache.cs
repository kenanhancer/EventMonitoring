using System.Collections.Generic;
using System.Linq;
using System;

namespace MonitoringLib
{
    public class InMemoryCache : ICache
    {
        Dictionary<string, List<object>> inMemoryCache = new Dictionary<string, List<object>>();

        public void Add(string key, List<object> item)
        {
            lock (inMemoryCache)
            {
                inMemoryCache.Add(key, item);
            }
        }

        public List<object> Get(string key)
        {
            lock (inMemoryCache)
            {
                return inMemoryCache[key];
            }
        }

        public void Remove(string key)
        {
            lock (inMemoryCache)
            {
                inMemoryCache.Remove(key);
            }
        }

        public void RemoveAll()
        {
            lock (inMemoryCache)
            {
                inMemoryCache.Clear();
            }
        }

        public int Count()
        {
            lock (inMemoryCache)
            {
                return inMemoryCache.Count;
            }
        }

        public bool TrySetValue(string key, Func<ICache, List<object>, bool> callback)
        {
            lock (inMemoryCache)
            {
                List<object> val;
                if (!inMemoryCache.TryGetValue(key, out val))
                {
                    val = new List<object>();
                    inMemoryCache.Add(key, val);
                }

                if (callback != null)
                    return callback(this, val);
            }
            return false;
        }
    }
}