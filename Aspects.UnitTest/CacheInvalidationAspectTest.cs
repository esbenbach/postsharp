using Aspects.Caching;
using Aspects.UnitTest.Stubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace Aspect.UnitTest
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CacheInvalidationAspectTest
    {
        /// <summary>
        /// Dummy interface needed to mock data access (something to cache)
        /// </summary>
        private IDummyInterface cachedDataAccess;

        /// <summary>
        /// Mocked interface
        /// </summary>
        private Mock<IDummyInterface> mockDataAccess;

        /// <summary>
        /// Init prior to running each test method
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            CacheSettingsRepository.Instance.Add("CacheAspectTest", new CacheSettings() { CacheEnabled = true });

            this.mockDataAccess = new Mock<IDummyInterface>(MockBehavior.Strict);
            this.cachedDataAccess = this.mockDataAccess.Object;
        }

        /// <summary>
        /// Cleanup after each test method
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            this.mockDataAccess = null;
            this.cachedDataAccess = null;
        }

        /// <summary>
        /// Test that invalidating the cache and requesting a previously cached object afterwards will indeed reevaluate the result.
        /// </summary>
        [TestMethod]
        public void InvalidCache_Invalidate_ShouldEmptyCache()
        {
            var dummy = new StubCachedClass();
            this.mockDataAccess.Setup(access => access.GetDummyData(It.IsAny<string>())).Returns((string s) => s);
            dummy.CachableAccessLayer = this.cachedDataAccess;

            // Act
            dummy.GetValueWithFixedKey("DataKey"); // Cache
            dummy.InvalidateValueWithFixedKey("DataKey"); // Invalidate
            dummy.GetValueWithFixedKey("DataKey"); // Cache should not hit

            // Assert
            this.mockDataAccess.Verify(access => access.GetDummyData("DataKey"), Times.Exactly(2), "Data access layer should be hit twice because the key has been invalidated");
        }


        /// <summary>
        /// Test that invalidating the cache with a region and requesting a previously cached object afterwards will indeed reevaluate the result.
        /// </summary>
        [TestMethod]
        public void InvalidCache_InvalidateRegionWithKey_ShouldEmptyCache()
        {
            var dummy = new StubCachedClass();
            this.mockDataAccess.Setup(access => access.GetDummyData(It.IsAny<string>())).Returns((string s) => s);
            dummy.CachableAccessLayer = this.cachedDataAccess;

            // Act
            dummy.GetValueWithKeyAndRegion("InvalidateDataKey"); // Cache
            dummy.InvalidateValueWithKeyRegion("InvalidateDataKey"); // Invalidate
            dummy.GetValueWithKeyAndRegion("InvalidateDataKey"); // Cache should not hit

            // Assert
            this.mockDataAccess.Verify(access => access.GetDummyData("InvalidateDataKey"), Times.Exactly(2), "Data access layer should be hit twice because the key has been invalidated");
        }

        /// <summary>
        /// Test that invalidating an entire region and requesting a previously cached object in that region will indeed reevaluate the result.
        /// </summary>
        [TestMethod]
        public void InvalidCache_InvalidateEntireRegion_ShouldEmptyCache()
        {
            var dummy = new StubCachedClass();
            this.mockDataAccess.Setup(access => access.GetDummyData(It.IsAny<string>())).Returns((string s) => s);
            dummy.CachableAccessLayer = this.cachedDataAccess;

            // Act
            dummy.GetValueWithKeyAndRegion("AKeyToCache"); // Cache
            dummy.InvalidateEntireRegion("TotallyIrrelevantKeyValueHopefully"); // Invalidate
            dummy.GetValueWithKeyAndRegion("AKeyToCache"); // Cache should not hit cache

            // Assert
            this.mockDataAccess.Verify(access => access.GetDummyData("AKeyToCache"), Times.Exactly(2), "Data access layer should be hit twice because the region has been invalidated");
        }

        /// <summary>
        /// Test that invalidating an entire region and requesting a previously cached object in a different region will still hit the cache.
        /// </summary>
        [TestMethod]
        public void InvalidCache_InvalidateDifferentRegion_ShouldNotRegion()
        {
            var dummy = new StubCachedClass();
            this.mockDataAccess.Setup(access => access.GetDummyData(It.IsAny<string>())).Returns((string s) => s);
            dummy.CachableAccessLayer = this.cachedDataAccess;

            // Act
            dummy.GetValue("ARegionKeyToCache"); // Cache
            dummy.InvalidateEntireRegion("TotallyIrrelevantKeyValueHopefully"); // Invalidate
            dummy.GetValue("ARegionKeyToCache"); // Cache should be hit

            // Assert
            this.mockDataAccess.Verify(access => access.GetDummyData("ARegionKeyToCache"), Times.Once, "Data access layer should be hit once because the region that was invalidated was a different one!");
        }
    }
}
