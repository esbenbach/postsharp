// <copyright file="RuntimeCacheItemPolicyAdapter.cs" company="Infosoft AS">
// Copyright (c) 2012 All Rights Reserved
// </copyright>
// <author>esbbach</author>
// <email></email>
// <date>10/23/2012 12:43:06 PM</date>
// <summary></summary>
namespace Infosoft.Library.Caching.Implementations
{
    using System;
    using System.Runtime.Caching;

    /// <summary>
    /// An adapter between the ICacheItemPolicy and the System.Runtime.Cache Policy system
    /// </summary>
    public class RuntimeCacheItemPolicyAdapter : ICacheItemPolicy
    {
        /// <summary>
        /// Gets or sets the absolute cache expiration.
        /// </summary>
        /// <value>
        /// The absolute expiration.
        /// </value>
        public int AbsoluteExpiration { get; set; }

        /// <summary>
        /// Gets or sets the sliding cache expiration.
        /// </summary>
        /// <value>
        /// The sliding expiration.
        /// </value>
        public int SlidingExpiration { get; set; }

        /// <summary>
        /// Gets the actual policy instance.
        /// </summary>
        /// <returns>A cache item policy</returns>
        public CacheItemPolicy GetInstance()
        {
            return new CacheItemPolicy()
            {
                AbsoluteExpiration = this.AbsoluteExpiration != 0 ? DateTimeOffset.Now.AddSeconds(this.AbsoluteExpiration) : ObjectCache.InfiniteAbsoluteExpiration,
                SlidingExpiration = this.SlidingExpiration != 0 ? TimeSpan.FromSeconds(this.SlidingExpiration) : ObjectCache.NoSlidingExpiration
            };
        }
    }
}