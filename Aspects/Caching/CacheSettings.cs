
namespace Aspects.Caching
{
    /// <summary>
    /// Class for implementing global cache related settings
    /// </summary>
    public class CacheSettings
    {
        /// <summary>
        /// The default cache settings
        /// </summary>
        public static CacheSettings DefaultCacheSettings = new CacheSettings() { CacheEnabled = true, DisableCacheLogging = false };

        /// <summary>
        /// The cache logger name
        /// </summary>
        internal const string CacheLoggerName = "Cache";

        /// <summary>
        /// Default cache status
        /// </summary>
        private bool cacheEnabled = true;

        /// <summary>
        /// Default logging status (enabled)
        /// </summary>
        private bool disableCacheLogging = false;

        /// <summary>
        /// Gets or sets a value indicating whether the cache is enabled. Default value is true
        /// </summary>
        /// <value>
        ///   <c>true</c> if cache enabled; otherwise, <c>false</c>.
        /// </value>
        public bool CacheEnabled
        {
            get { return this.cacheEnabled; }
            set { this.cacheEnabled = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to disable cache logging, if set to true no logging will be done.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cache logging is disabled otherwise, <c>false</c>.
        /// </value>
        public bool DisableCacheLogging
        {
            get { return this.disableCacheLogging; }
            set { this.disableCacheLogging = value; }
        }
    }
}