
namespace Aspects.Caching
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