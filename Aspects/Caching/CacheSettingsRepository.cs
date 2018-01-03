
namespace Aspects.Caching
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A cache settings container used to defined cache settings such as logging and state (enabled/disabled) for each cache context.
    /// </summary>
    public sealed class CacheSettingsRepository
    {
        /// <summary>
        /// Lazy initialised instance (Lazy is thread safe so we can avoid locks).
        /// </summary>
        private static readonly Lazy<CacheSettingsRepository> RepositoryInstance = new Lazy<CacheSettingsRepository>(() => new CacheSettingsRepository());

        /// <summary>
        /// The configured settings
        /// </summary>
        private ConcurrentDictionary<string, CacheSettings> configuredSettings = new ConcurrentDictionary<string, CacheSettings>();

        /// <summary>
        /// Prevents a default instance of the <see cref="CacheSettingsRepository"/> class from being created.
        /// </summary>
        private CacheSettingsRepository()
        {
        }

        /// <summary>
        /// Gets the instance of the CacheSettingsRepository
        /// </summary>
        public static CacheSettingsRepository Instance
        {
            get
            {
                return RepositoryInstance.Value;
            }
        }

        /// <summary>
        /// Adds settings for the specified context name
        /// </summary>
        /// <param name="contextName">Name of the context.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>True if the settings were added, false if there already was an item with the given key</returns>
        public bool Add(string contextName, CacheSettings settings)
        {
            return this.configuredSettings.TryAdd(contextName, settings);
        }

        /// <summary>
        /// Gets cache settings for the specified contextname.
        /// </summary>
        /// <param name="contextName">The contextname.</param>
        /// <returns>A cache settings object for the given context</returns>
        public CacheSettings Get(string contextName)
        {
            this.configuredSettings.TryGetValue(contextName, out CacheSettings setting);
            return setting;
        }

        /// <summary>
        /// Gets the default cache settings.
        /// </summary>
        /// <returns>A default cache settings instance where caching and logging is enabled</returns>
        public CacheSettings GetDefault()
        {
            return CacheSettings.DefaultCacheSettings;
        }
    }
}