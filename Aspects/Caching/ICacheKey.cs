
namespace Aspects.Caching
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