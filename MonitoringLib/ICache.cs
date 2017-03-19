using System;
using System.Collections.Generic;

namespace MonitoringLib
{
    public interface ICache
    {
        void Add(string key, List<object> item);
        void Remove(string key);
        void RemoveAll();
        List<object> Get(string key);
        bool TrySetValue(string key, Func<ICache, List<object>, bool> callback);
        int Count();
    }
}