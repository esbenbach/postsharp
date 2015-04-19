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
        /// Policy instance
        /// </summary>
        private CacheItemPolicy instance;

        /// <summary>
        /// The absolute expiration time measured in seconds
        /// </summary>
        private int absoluteExpirationSeconds;

        /// <summary>
        /// Sliding expiration time measured in seconds
        /// </summary>
        private int slidingExpirationSeconds;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeCacheItemPolicyAdapter" /> class.
        /// </summary>
        public RuntimeCacheItemPolicyAdapter()
        {
            this.instance = new CacheItemPolicy();
        }

        /// <summary>
        /// Gets or sets the absolute cache expiration.
        /// </summary>
        /// <value>
        /// The absolute expiration.
        /// </value>
        public int AbsoluteExpiration
        {
            get
            {
                return this.absoluteExpirationSeconds;
            }

            set
            {
                this.absoluteExpirationSeconds = value;
                this.UpdateAbsoluteExpiration();
            }
        }

        /// <summary>
        /// Gets or sets the sliding cache expiration.
        /// </summary>
        /// <value>
        /// The sliding expiration.
        /// </value>
        public int SlidingExpiration
        {
            get
            {
                return this.slidingExpirationSeconds;
            }

            set
            {
                this.slidingExpirationSeconds = value;
                this.UpdateSlidingExpiration();
            }
        }

        /// <summary>
        /// Gets the actual policy instance.
        /// </summary>
        /// <returns>A cache item policy</returns>
        public CacheItemPolicy GetInstance()
        {
            return this.instance;
        }

        /// <summary>
        /// Updates the sliding expiration for the instance
        /// </summary>
        private void UpdateSlidingExpiration()
        {
            if (this.SlidingExpiration != 0 && this.SlidingExpiration != int.MinValue)
            {
                this.instance.SlidingExpiration = TimeSpan.FromSeconds(this.SlidingExpiration);
            }
        }

        /// <summary>
        /// Updates the absolute expiration for the instance
        /// </summary>
        private void UpdateAbsoluteExpiration()
        {
            if (this.AbsoluteExpiration != 0 && this.AbsoluteExpiration != int.MinValue)
            {
                this.instance.AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(this.AbsoluteExpiration);
            }
        }
    }
}