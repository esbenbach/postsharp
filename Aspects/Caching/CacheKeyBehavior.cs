// <copyright file="CacheKeyBehavior.cs" company="Infosoft AS">
// Copyright (c) 2012 All Rights Reserved
// </copyright>
// <author>esbbach</author>
// <email></email>
// <date>10/15/2012 2:01:20 PM</date>
// <summary></summary>
namespace Infosoft.Library.Caching
{
    using System;

    /// <summary>
    /// Flags controlling the behavior of the cache key generation.
    /// </summary>
    [Flags]
    public enum CacheKeyBehavior
    {
        /// <summary>
        /// Default cache key behavior
        /// </summary>
        Default = 0,

        /// <summary>
        /// Ignore method parameters when building cache key (increase risk of invalid hits in cache entry if auto generated key is used)
        /// </summary>
        IgnoreParameters = 1,
    }
}