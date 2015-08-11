
namespace Aspects.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Indicate the cache items eviction policy
    /// </summary>
    public interface ICacheItemPolicy
    {
        /// <summary>
        /// Absolute expiration time in seconds
        /// </summary>
        int AbsoluteExpiration { get; set; }

        /// <summary>
        /// Sliding expiration time in seconds
        /// </summary>
        int SlidingExpiration { get; set; }
    }
}
