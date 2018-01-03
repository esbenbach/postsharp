
namespace Aspects.Caching
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    //using Aspects.Caching.Implementations;
    using log4net;
    using PostSharp.Aspects;
    using PostSharp.Aspects.Dependencies;
    using PostSharp.Extensibility;

    /// <summary>
    /// Invalidate an instance in the cache.
    /// </summary>
    [Serializable]
    [ProvideAspectRole(StandardRoles.Caching)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Tracing)]
    [ExcludeFromCodeCoverage]
    [MulticastAttributeUsage(MulticastTargets.Method, TargetMemberAttributes = MulticastAttributes.Instance)]
    public class CacheInvalidationAspect : CacheAspectBase
    {
        /// <summary>
        /// Specifies if the entire region should be invalidated, if so the key is entirely ignored.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [invalidate region]; otherwise, <c>false</c>.
        /// </value>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The property is a named parameter in the attribute and must describe its function as such and not a property")]
        public bool InvalidateRegion { get; set; }

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
        /// Initializes the caching aspect with needed data
        /// </summary>
        /// <param name="method">Method to which the current aspect is applied.</param>
        public override void RuntimeInitialize(MethodBase method)
        {
            base.RuntimeInitialize(method);

            // TODO: Aspect implementation currently assumes that the adapter references ONE SHARED STATIC cache instance in its implementation - not reliable
            //this.cacheAdapter = new RuntimeMemoryCacheAdapter(); // TODO: Figure out how to init this properly
        }

        /// <summary>
        /// Method invoked <i>instead</i> of the method to which the aspect has been applied.
        /// </summary>
        /// <param name="args">Advice arguments.</param>
        public sealed override void OnInvoke(MethodInterceptionArgs args)
        {
            // Execute the original method first and let it do its job.
            args.ReturnValue = args.Invoke(args.Arguments);

            // Initialise settings if not already initialised
            if (this.settings == null)
            {
                this.settings = this.GetConfiguredCacheSettings();
            }

            // If the cache is disabled just return.
            if (!this.settings.CacheEnabled)
            {
                return;
            }

            // TODO: Fix locking between adding and deleting cache entries.
            // TODO: Consider making a TryDelete method that does the contains part in a syncronized fashion
            if (this.InvalidateRegion)
            {
                this.LogToDebugIfEnabled("Trying to remove all cache entries in region {0}", this.Region);
                this.cacheAdapter.DeleteRegion(this.Region);
            }
            else
            {
                string key = this.keyBuilder.BuildCacheKey(args.Arguments.ToArray());
                this.LogToDebugIfEnabled("Trying to remove cache entry for key {0}", key);

                if (this.cacheAdapter.Contains(key, this.Region))
                {
                    this.cacheAdapter.Delete(key, this.Region);
                }
            }
        }
    }
}