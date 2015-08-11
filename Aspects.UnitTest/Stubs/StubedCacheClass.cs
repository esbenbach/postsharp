namespace Aspects.UnitTest.Stubs
{
    using Caching;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A dummy stub class for testing code related to caching
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class StubCachedClass
    {
        /// <summary>
        /// Gets or sets the cachable access layer.
        /// </summary>
        /// <value>
        /// The cachable access layer.
        /// </value>
        public IDummyInterface CachableAccessLayer { get; set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="keyValue">The key value.</param>
        /// <returns>Something from the CachableAccessLayer with key, keyValue</returns>
        [CacheAspect(ContextName = "CacheAspectTest")]
        public string GetValue(string keyValue)
        {
            // Dummy call to data layer
            return this.CachableAccessLayer.GetDummyData(keyValue);
        }

        /// <summary>
        /// Gets the value2.
        /// </summary>
        /// <param name="keyValue">The key value.</param>
        /// <param name="dummyParameter">The dummy parameter.</param>
        /// <returns>the value of dummyParameter</returns>
        [CacheAspect(ContextName = "CacheAspectTest")]
        public string GetValue2(string keyValue, string dummyParameter)
        {
            // Dummy call to data layer
            return this.CachableAccessLayer.GetDummyData(dummyParameter);
        }

        [CacheAspect(ContextName = "CacheAspectTest", Key = "FixedKey")]
        public string GetValueWithFixedKey(string keyValue)
        {
            // Dummy call to data layer
            return this.CachableAccessLayer.GetDummyData(keyValue);
        }

        [CacheInvalidationAspect(ContextName = "CacheAspectTest", Key = "FixedKey")]
        public void InvalidateValueWithFixedKey(string keyValue)
        {
            // Do nothing, we just need the invalidation
        }

        [CacheAspect(ContextName = "CacheAspectTest", Key = "FixedKey", Region = "FixedRegion")]
        public string GetValueWithKeyAndRegion(string keyValue)
        {
            // Dummy call to data layer
            return this.CachableAccessLayer.GetDummyData(keyValue);
        }

        [CacheInvalidationAspect(ContextName = "CacheAspectTest", Key = "FixedKey", Region = "FixedRegion")]
        public void InvalidateValueWithKeyRegion(string keyValue)
        {
            // Do nothing, we just need the invalidation
        }

        [CacheInvalidationAspect(ContextName = "CacheAspectTest", Key = "FixedKey", Region = "FixedRegion", InvalidateRegion = true)]
        public void InvalidateEntireRegion(string keyValue)
        {
            // Do nothing, we just need the invalidation
        }

        /// <summary>
        /// Gets the value using TTL.
        /// </summary>
        /// <param name="keyValue">The key value.</param>
        /// <returns>Something from the CachableAccessLayer with key, keyValue</returns>
        [CacheAspect(AbsoluteItemExpiration = 2, ContextName = "CacheAspectTest")]
        public string GetValueUsingAbsoluteTTL(string keyValue)
        {
            // Dummy call to data layer
            return this.CachableAccessLayer.GetDummyData(keyValue);
        }

        /// <summary>
        /// Gets the value using TTL.
        /// </summary>
        /// <param name="keyValue">The key value.</param>
        /// <returns>Something from the CachableAccessLayer with key, keyValue</returns>
        [CacheAspect(SlidingItemExpiration = 2, ContextName = "CacheAspectTest")]
        public string GetValueUsingSlidingTTL(string keyValue)
        {
            // Dummy call to data layer
            return this.CachableAccessLayer.GetDummyData(keyValue);
        }

        /// <summary>
        /// Gets the value3.
        /// </summary>
        /// <param name="keyValue">The key value.</param>
        /// <returns>Something from the CachableAccessLayer with key, keyValue</returns>
        [CacheAspect(ContextName = "CacheDisabledTest")]
        public string GetValue3(string keyValue)
        {
            // Dummy call to data layer
            return this.CachableAccessLayer.GetDummyData(keyValue);
        }

        /// <summary>
        /// Gets the cached item implementing cache key.
        /// </summary>
        /// <param name="inputItem">The input item.</param>
        /// <returns>A fixed string</returns>
        public string GetCachedItemImplementingCacheKey(ICacheKey inputItem)
        {
            return "CacheKeyString";
        }
    }
}