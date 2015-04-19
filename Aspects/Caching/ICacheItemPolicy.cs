// <copyright file="ICacheItemPolicy.cs" company="Infosoft AS">
// Copyright (c) 2012 All Rights Reserved
// </copyright>
// <author>esbbach</author>
// <email></email>
// <date>23/10/2012 10:45:01 AM</date>
// <summary></summary>
namespace Infosoft.Library.Caching
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
