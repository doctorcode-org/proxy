using System;
using System.Web;
using System.Web.Caching;

namespace DoctorProxy.Data
{
    public static class CacheManager
    {
        public static void AddDataToCache(string key, object data, CacheDependency dependency)
        {
            if (HttpContext.Current != null && data != null)
            {
                var Cache = HttpContext.Current.Cache;
                if (Cache != null)
                {
                    Cache.Insert(key,
                        data,
                        dependency,
                        DateTime.MaxValue,
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.High,
                        null);
                }
            }
        }

        public static T GetDataFromCache<T>(string key)
        {
            if (HttpContext.Current != null)
            {
                var Cache = HttpContext.Current.Cache;
                if (Cache != null)
                    return (T)Cache[key];
            }

            return default(T);
        }

        public static void RemoveDataFromCache(string key)
        {
            if (HttpContext.Current != null)
            {
                var Cache = HttpContext.Current.Cache;
                if (Cache != null)
                    Cache.Remove(key);
            }
        }
    }
}
