// <copyright file="CacheAspectBase.cs" company="Infosoft AS">
// Copyright (c) 2012 All Rights Reserved
// </copyright>
// <author>esbbach</author>
// <email></email>
// <date>12/10/2014 1:46:52 PM</date>
// <summary></summary>
namespace Infosoft.Library.Caching
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using log4net;
    using PostSharp.Aspects;
    using PostSharp.Aspects.Dependencies;
    using PostSharp.Extensibility;

    /// <summary>
    /// Base cache class
    /// </summary>
    [Serializable]
    public class CacheAspectBase : MethodInterceptionAspect
    {
        /// <summary>
        /// The actual caching provider
        /// </summary>
        [NonSerialized]
        protected ICacheAdapter cacheAdapter;

        /// <summary>
        /// Logger instance always initialised to "Cache"
        /// </summary>
        [NonSerialized]
        protected ILog logger;

        /// <summary>
        /// The cache settings for this instance
        /// </summary>
        [NonSerialized]
        protected CacheSettings settings;

        /// <summary>
        /// Helper class to build cache entry keys
        /// </summary>
        protected CacheKeyBuilder keyBuilder;

        /// <summary>
        /// Named of the method the aspect has been applied to
        /// </summary>
        protected string methodName;

        /// <summary>
        /// Name of the class for the observed method
        /// </summary>
        protected string className;

        /// <summary>
        /// The cache context/instance that the cache should have. If set this overrides anything set on a class level.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The property is a named parameter in the attribute and must describe its function as such and not a property")]
        public string ContextName { get; set; }

        /// <summary>
        /// Specifies the key prefix of the cached entry. If not provided it will be automatically generated.
        /// </summary>
        /// <value>
        /// The desired prefix
        /// </value>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The property is a named parameter in the attribute and must describe its function as such and not a property")]
        public string Key { get; set; }

        /// <summary>
        /// (Optional)Specifies the name of the region for the cached entry. This might not be valid for all cache implementations.
        /// </summary>
        /// <value>
        /// The desired region name
        /// </value>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The property is a named parameter in the attribute and must describe its function as such and not a property")]
        public string Region { get; set; }

        /// <summary>
        /// A csv string, specifying input parameters from the method which should be used to build the cache key.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The property is a named parameter in the attribute and must describe its function as such and not a property")]
        public string KeyParameters { get; set; }

        /// <summary>
        /// Controls the behavior of the key building
        /// </summary>
        /// <value>
        /// The key building behavior.
        /// </value>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The property is a named parameter in the attribute and must describe its function as such and not a property")]
        public CacheKeyBehavior KeyBuildingBehavior { get; set; }

        /// <summary>
        /// Method invoked at build time to initialize the instance fields of the current aspect. This method is invoked
        /// before any other build-time method.
        /// </summary>
        /// <param name="method">Method to which the current aspect is applied</param>
        /// <param name="aspectInfo">Reserved for future usage.</param>
        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(method, aspectInfo);

            this.methodName = method.Name;
            this.className = method.DeclaringType.FullName;

            // Determine the ContextName. This will later be used runtime to initialize the cachesettings specific for this context.
            if (string.IsNullOrWhiteSpace(this.ContextName))
            {
                var contextNameAttribute = method.DeclaringType.GetCustomAttribute<CacheContextAttribute>();
                if (contextNameAttribute != null)
                {
                    this.ContextName = contextNameAttribute.Name;
                }
            }

            this.keyBuilder = new CacheKeyBuilder(method, this.Key, this.KeyParameters, this.KeyBuildingBehavior);
        }

        /// <summary>
        /// Initialise stuff at runtime
        /// </summary>
        /// <param name="method">Method being cached</param>
        public override void RuntimeInitialize(MethodBase method)
        {
            base.RuntimeInitialize(method);
            this.logger = LogManager.GetLogger(CacheSettings.CacheLoggerName);
        }

        /// <summary>
        /// Logs to the debug format if cache logging is enabled
        /// </summary>
        /// <param name="formatString">The format string.</param>
        /// <param name="formatArgs">The format args.</param>
        protected void LogToDebugIfEnabled(string formatString, params object[] formatArgs)
        {
            if (!this.settings.DisableCacheLogging)
            {
                this.logger.DebugFormat(formatString, formatArgs);
            }
        }

        /// <summary>
        /// Gets the configured cache settings from the CacheSettingsRepository baed on the ContextName.
        /// </summary>
        /// <returns>A cache settings instance.</returns>
        protected CacheSettings GetConfiguredCacheSettings()
        {
            CacheSettings result = null;
            if (!string.IsNullOrWhiteSpace(this.ContextName))
            {
                result = CacheSettingsRepository.Instance.Get(this.ContextName);
            }

            if (result == null)
            {
                result = CacheSettingsRepository.Instance.GetDefault();
            }

            return result;
        }
    }
}