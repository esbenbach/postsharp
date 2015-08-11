// <copyright file="RuntimeMemoryCacheAdapter.cs" company="Infosoft AS">
// Copyright (c) 2012 All Rights Reserved
// </copyright>
// <author>esbbach</author>
// <email></email>
// <date>10/15/2012 6:16:56 PM</date>
// <summary></summary>
namespace Infosoft.Library.Caching.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Caching;

    /// <summary>
    /// A cache adapter using the System.Runtime.Caching cache as an in-process memory cache
    /// </summary>
    public class RuntimeMemoryCacheAdapter : ICacheAdapter
    {
        /// <summary>
        /// Cache instance
        /// </summary>
        private static MemoryCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeMemoryCacheAdapter" /> class.
        /// </summary>
        public RuntimeMemoryCacheAdapter()
        {
            cache = MemoryCache.Default;
        }

        /// <summary>
        /// Adds an entry with the specified key to the cache in the cache region
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="inputPolicy">The input policy.</param>
        /// <param name="cacheRegion">The cache region.</param>
        /// <returns>
        /// true if added false if something with the key already exists
        /// </returns>
        public bool AddEntry(string key, object entry, ICacheItemPolicy inputPolicy, string cacheRegion)
        {
            // In case the entry is null we "fake" the result since the cache adapter cannot figure out how to cache null values.
            // Note that this might cause strange results if the return value is tested for (which it is not currently)
            if (entry == null)
            {
                return false;
            }

            var policy = (inputPolicy as RuntimeCacheItemPolicyAdapter).GetInstance();
            return cache.Add(this.GetCacheKey(key, cacheRegion), entry, policy, null);
        }

        /// <summary>
        /// Gets the cache item with the specified key from the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cacheRegion">The cache region.</param>
        /// <returns>
        /// The entry if it exists otherwise null
        /// </returns>
        public object GetEntry(string key, string cacheRegion)
        {
            return cache.Get(this.GetCacheKey(key, cacheRegion), null);
        }

        /// <summary>
        /// Determines whether the cache contains and object with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cacheRegion">The cache region.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified key]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(string key, string cacheRegion)
        {
            return cache.Contains(this.GetCacheKey(key, cacheRegion), null);
        }

        /// <summary>
        /// Deletes the object with the specified key from the cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cacheRegion">The cache region.</param>
        public void Delete(string key, string cacheRegion)
        {
            cache.Remove(this.GetCacheKey(key, cacheRegion), null);
        }

        /// <summary>
        /// Deletes all objects within the specified region.
        /// </summary>
        /// <param name="cacheRegion">The cache region.</param>
        public void DeleteRegion(string cacheRegion)
        {
            IEnumerable<string> keys = cache.Where(item => item.Key.StartsWith(this.GetRegionString(cacheRegion))).Select(item => item.Key);
            foreach (string key in keys)
            {
                cache.Remove(key);
            }
        }

        /// <summary>
        /// Gets a region string used to prefix all keys to support a hacky type of regions. Note this can fail horribly if for some reason a key is generated with the same prefix!
        /// </summary>
        /// <param name="cacheRegion">The cache region.</param>
        /// <returns>A region prefix string</returns>
        private string GetRegionString(string cacheRegion)
        {
            return "Region=##" + cacheRegion + "##";
        }

        /// <summary>
        /// Gets the cache key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cacheRegion">The cache region.</param>
        /// <returns>A string with the cache region as part of the key</returns>
        private string GetCacheKey(string key, string cacheRegion)
        {
            string cacheKey = string.Empty;
            if (!string.IsNullOrWhiteSpace(cacheRegion))
            {
                cacheKey = this.GetRegionString(cacheRegion);
            }

            cacheKey += key;

            return cacheKey;
        }
    }
}