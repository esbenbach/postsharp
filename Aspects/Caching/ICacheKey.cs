// <copyright file="ICacheKey.cs" company="Infosoft AS">
// Copyright (c) 2012 All Rights Reserved
// </copyright>
// <author>esbbach</author>
// <email></email>
// <date>10/15/2012 1:19:28 PM</date>
// <summary></summary>
namespace Infosoft.Library.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Indicates that the implemented member has a special properties to be used for caching.
    /// </summary>
    public interface ICacheKey
    {
        /// <summary>
        /// Gets the cache key.
        /// </summary>
        /// <returns>A string that can be used as a cache key</returns>
        string GetCacheKey();
    }
}