// <copyright file="CacheAspect.cs" company="Infosoft AS">
// Copyright (c) 2012 All Rights Reserved
// </copyright>
// <author>esbbach</author>
// <email>esbbach@infosoft.no</email>
// <date>10/15/2012 12:28:31 PM</date>
// <summary>Provides caching for a decorated method</summary>
namespace Infosoft.Library.Caching
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using Infosoft.Library.Caching.Implementations;
    using PostSharp.Aspects;
    using PostSharp.Aspects.Dependencies;
    using PostSharp.Extensibility;

    /// <summary>
    /// An aspect that indicates that caching is done on the applied method
    /// </summary>
    [Serializable]
    [ProvideAspectRole(StandardRoles.Caching)]
    [MulticastAttributeUsage()]
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class CacheAspect : CacheAspectBase
    {
        /// <summary>
        /// Thread syncronization point
        /// </summary>
        [NonSerialized]
        private object syncRoot;

        /// <summary>
        /// Caching policy
        /// </summary>
        [NonSerialized]
        private ICacheItemPolicy policyAdapter;

        /// <summary>
        /// Validation of cached method
        /// </summary>
        [NonSerialized]
        private IRequirementsValidator validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheAspect" /> class.
        /// </summary>
        public CacheAspect()
        {
            this.validator = new RequirementsValidator();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheAspect" /> class. Constructor is only here to allow for unit testing.
        /// </summary>
        /// <param name="validator">The validator.</param>
        public CacheAspect(IRequirementsValidator validator)
        {
            this.validator = validator;
        }

        /// <summary>
        /// Override the default absolute item expiration with the given offset (in seconds)
        /// </summary>
        /// <value>
        /// The cache expiration.
        /// </value>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The property is a named parameter in the attribute and must describe its function as such and not a property")]
        public int AbsoluteItemExpiration { get; set; }

        /// <summary>
        /// Override the default sliding item expiration with the given offset (in seconds)
        /// </summary>
        /// <value>
        /// The cache expiration.
        /// </value>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The property is a named parameter in the attribute and must describe its function as such and not a property")]
        public int SlidingItemExpiration { get; set; }

        /// <summary>
        /// Validate aspect usage at compile time. If the aspect has been used for a not-cacheable return type an error will be thrown.
        /// </summary>
        /// <remarks>
        /// Please note that it is unclear what constitutes an not-cacheable return type. So the validation is limited.
        /// </remarks>
        /// <param name="method">The method.</param>
        /// <returns>True if the validation is okay, false otherwise</returns>
        public override bool CompileTimeValidate(MethodBase method)
        {
            // General method validation
            if (!this.validator.IsValidCacheMethod(method))
            {
                return false;
            }

            // Parameter validation
            if (!this.validator.IsValidCacheParameters(method))
            {
                return false;
            }

            if (!this.validator.IsValidCacheKeyparameters(method, this.keyBuilder.ParametersUsed))
            {
                return false;
            }

            if(!this.validator.IsValidExpirationTimes(method, this.AbsoluteItemExpiration, this.SlidingItemExpiration))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Initializes the caching aspect with needed data
        /// </summary>
        /// <param name="method">Method to which the current aspect is applied.</param>
        public override void RuntimeInitialize(MethodBase method)
        {
            base.RuntimeInitialize(method);
            
            this.syncRoot = new object();

            // Current aspect implementation assumes that the adapter references ONE SHARED STATIC cache instance in its implementation - not reliable
            // However for remote cache instances such as the AppFabricCache this is probably the correct idea/mindset.
            this.cacheAdapter = new RuntimeMemoryCacheAdapter(); // TODO: This should probably be injected/servicelocated somehow so we can switch cache adapter without rebuilding.
            this.policyAdapter = new RuntimeCacheItemPolicyAdapter();
            //this.cacheAdapter = AppFabricCacheAdapter.Instance;

            // Absolute + Sliding is mutually exclusive don't do them both
            if (this.AbsoluteItemExpiration != 0)
            {
                this.policyAdapter.AbsoluteExpiration = this.AbsoluteItemExpiration;
            } 
            else if(this.SlidingItemExpiration != 0)
            {
                this.policyAdapter.SlidingExpiration = this.SlidingItemExpiration;
            }
        }

        /// <summary>
        /// Method invoked <i>instead</i> of the method to which the aspect has been applied.
        /// </summary>
        /// <param name="args">Advice arguments.</param>
        public override void OnInvoke(MethodInterceptionArgs args)
        {
            // Initialise settings if not already initialised
            if (this.settings == null)
            { 
                this.settings = this.GetConfiguredCacheSettings();
            }

            // If the cache is disabled just execute the original method and return asap.
            if (!this.settings.CacheEnabled)
            {
                args.ReturnValue = args.Invoke(args.Arguments);
                return;
            }

            // Build a cache key from the object instance and the arguments
            string key = this.keyBuilder.BuildCacheKey(args.Arguments.ToArray());

            // TODO: If invalidate cache is implemented we should consider a TryGet method that locks around contains otherwise there can be an issue with a "contains" returns true, another thread invalidates the cache and get returns null.
            if (this.cacheAdapter.Contains(key, this.Region))
            {
                this.LogToDebugIfEnabled("Found key in cache: {0}", key);
                args.ReturnValue = this.cacheAdapter.GetEntry(key, this.Region);
            }
            else
            {
                lock (this.syncRoot)
                {
                    if (!this.cacheAdapter.Contains(key, this.Region))
                    {
                        this.LogToDebugIfEnabled("Key {0} not in cache executing method {1}", key, this.methodName);

                        var returnValue = args.Invoke(args.Arguments);
                        args.ReturnValue = returnValue;
                        this.cacheAdapter.AddEntry(key, returnValue, this.policyAdapter, this.Region);
                    }
                    else
                    {
                        this.LogToDebugIfEnabled("Found key in cache: {0}", key);
                        args.ReturnValue = this.cacheAdapter.GetEntry(key, this.Region);
                    }
                }
            }
        }
    }
}