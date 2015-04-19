// <copyright file="AppFabricCacheAdapter.cs" company="Infosoft AS">
// Copyright (c) 2012 All Rights Reserved
// </copyright>
// <author>esbbach</author>
// <email></email>
// <date>7/30/2013 10:26:36 AM</date>
// <summary></summary>
namespace Infosoft.Library.Caching.Implementations
{
    using System.Linq;
    using Microsoft.ApplicationServer.Caching;

    /// <summary>
    /// An adapter for interfacing with the AppFabric cache system.
    /// </summary>
    public class AppFabricCacheAdapter : Singleton<AppFabricCacheAdapter>, ICacheAdapter
    {
        /// <summary>
        /// The cache instance
        /// </summary>
        private static DataCache cacheInstance;
        
        /// <summary>
        /// Prevents a default instance of the <see cref="AppFabricCacheAdapter"/> class from being created.
        /// </summary>
        private AppFabricCacheAdapter()
        {
            DataCacheFactory cacheFactory = new DataCacheFactory();
            cacheInstance = cacheFactory.GetDefaultCache();
        }

        /// <summary>
        /// Adds an entry with the specified key to the cache in the cache region
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="cacheItemPolicy">The cache item policy.</param>
        /// <param name="cacheRegion">The cache region.</param>
        /// <returns>
        /// true if added false if something with the key already exists
        /// </returns>
        public bool AddEntry(string key, object entry, ICacheItemPolicy cacheItemPolicy, string cacheRegion = null)
        {
            if (string.IsNullOrWhiteSpace(cacheRegion))
            {
                cacheInstance.Add(key, entry);
            }
            else
            {
                if (!cacheInstance.GetSystemRegions().Contains(cacheRegion))
                {
                    cacheInstance.CreateRegion(cacheRegion);
                }

                cacheInstance.Add(key, entry, cacheRegion);
            }
            
            return true;
        }

        /// <summary>
        /// Gets the cache item with the specified key from the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cacheRegion">The cache region.</param>
        /// <returns>
        /// The entry if it exists otherwise null
        /// </returns>
        public object GetEntry(string key, string cacheRegion = null)
        {
            if (string.IsNullOrWhiteSpace(cacheRegion))
            {
                return cacheInstance.Get(key);
            }

            return cacheInstance.Get(key, cacheRegion);
        }

        /// <summary>
        /// Determines whether the cache contains and object with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cacheRegion">The cache region.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified key]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(string key, string cacheRegion = null)
        {
            if (string.IsNullOrWhiteSpace(cacheRegion))
            {
                return cacheInstance.Get(key) != null;
            }

            return cacheInstance.Get(key, cacheRegion) != null;
        }

        /// <summary>
        /// Deletes the object with the specified key from the cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cacheRegion">The cache region.</param>
        public void Delete(string key, string cacheRegion = null)
        {
            if (string.IsNullOrWhiteSpace(cacheRegion))
            {
                cacheInstance.Remove(key);
            }
            else
            {
                cacheInstance.Remove(key, cacheRegion);
            }
        }

        /// <summary>
        /// Deletes all objects within the specified region.
        /// </summary>
        /// <param name="cacheRegion">The cache region.</param>
        public void DeleteRegion(string cacheRegion)
        {
            cacheInstance.RemoveRegion(cacheRegion);
        }
    }
}