namespace Aspects.UnitTest
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using PostSharp.Aspects;
    using Caching;
    using Stubs;

    /// <summary>
    /// Test the implementation of the CacheAspect attribute on a class
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CacheAspectTest
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
        /// Test that geting data once hits the data acess layer (dummy interface)
        /// </summary>
        [TestMethod]
        public void CacheTest_GetDataOnce_GetsDataFromAccessLayer()
        {
            // Arrange
            var dummy = new StubCachedClass();
            this.mockDataAccess.Setup(access => access.GetDummyData(It.IsAny<string>())).Returns((string s) => s);
            dummy.CachableAccessLayer = this.cachedDataAccess;

            // Act
            dummy.GetValue("ValueToGet");

            // Assert
            this.mockDataAccess.Verify(access => access.GetDummyData("ValueToGet"), Times.Once(), "Data access layer should be hit");
        }

        /// <summary>
        /// Test that geting data thrice hits the data acess layer (dummy interface) once only.
        /// </summary>
        [TestMethod]
        public void CacheTest_GetDataThrice_GetDataFromCache()
        {
            // Arrange
            var dummy = new StubCachedClass();
            this.mockDataAccess.Setup(access => access.GetDummyData(It.IsAny<string>())).Returns((string s) => s);
            dummy.CachableAccessLayer = this.cachedDataAccess;

            // Act
            dummy.GetValue("Test_GetDataThrice_GetDataFromCache");
            dummy.GetValue("Test_GetDataThrice_GetDataFromCache");
            dummy.GetValue("Test_GetDataThrice_GetDataFromCache");

            // Assert
            this.mockDataAccess.Verify(access => access.GetDummyData("Test_GetDataThrice_GetDataFromCache"), Times.Once(), "Data access layer should be hit only once");
        }

        /// <summary>
        /// Test that geting different data hits the data access layer each time
        /// </summary>
        [TestMethod]
        public void CacheTest_GetDifferentData_CacheNotUsed()
        {
            // Arrange
            var dummy = new StubCachedClass();
            this.mockDataAccess.Setup(access => access.GetDummyData(It.IsAny<string>())).Returns((string s) => s);
            dummy.CachableAccessLayer = this.cachedDataAccess;

            // Act
            dummy.GetValue("CacheTest_GetDifferentData_CacheNotUsed_1");
            dummy.GetValue("CacheTest_GetDifferentData_CacheNotUsed_2");
            dummy.GetValue("CacheTest_GetDifferentData_CacheNotUsed_3");

            // Assert
            this.mockDataAccess.Verify(access => access.GetDummyData(It.IsAny<string>()), Times.Exactly(3), "Data access layer should be hit three times");
        }

        /// <summary>
        /// Test that geting data thrice hits the data acess layer thrice if the cache has been disabled
        /// </summary>
        [TestMethod]
        public void CacheDisabledTest_GetDataThrice_GetDataFromDataLayer()
        {
            // Arrange
            CacheSettingsRepository.Instance.Add("CacheDisabledTest", new CacheSettings() { CacheEnabled = false });
            var dummy = new StubCachedClass();
            this.mockDataAccess.Setup(access => access.GetDummyData(It.IsAny<string>())).Returns((string s) => s);
            dummy.CachableAccessLayer = this.cachedDataAccess;

            // Act
            dummy.GetValue3("Test_GetDataThrice_GetDataFromDataLayer");
            dummy.GetValue3("Test_GetDataThrice_GetDataFromDataLayer");
            dummy.GetValue3("Test_GetDataThrice_GetDataFromDataLayer");

            // Assert
            this.mockDataAccess.Verify(access => access.GetDummyData("Test_GetDataThrice_GetDataFromDataLayer"), Times.Exactly(3), "Data access layer should be hit 3 times since the cache is disabled");
        }

        /// <summary>
        /// Test that caching the same data in different regions is indeed cached twice.
        /// </summary>
        [TestMethod]
        public void CacheRegions_SameDataDifferentRegion_CacheSeparately()
        {
            // Arrange
            var dummy = new StubCachedClass();
            this.mockDataAccess.Setup(access => access.GetDummyData(It.IsAny<string>())).Returns((string s) => s);
            dummy.CachableAccessLayer = this.cachedDataAccess;

            // Act
            dummy.GetValue("DifferentRegionsTest");
            dummy.GetValueWithKeyAndRegion("DifferentRegionsTest");

            // Act
            this.mockDataAccess.Verify(access => access.GetDummyData("DifferentRegionsTest"), Times.Exactly(2), "Data access layer should be hit 2 times since the caching is done in different regions");
        }


        /// <summary>
        /// Test that caching the same data in same region is accessed once
        /// </summary>
        [TestMethod]
        public void CacheRegions_DataInSameRegion_CachedOnce()
        {
            // Arrange
            var dummy = new StubCachedClass();
            this.mockDataAccess.Setup(access => access.GetDummyData(It.IsAny<string>())).Returns((string s) => s);
            dummy.CachableAccessLayer = this.cachedDataAccess;

            // Act
            dummy.GetValueWithKeyAndRegion("SameRegionTest");
            dummy.GetValueWithKeyAndRegion("SameRegionTest");

            // Act
            this.mockDataAccess.Verify(access => access.GetDummyData("SameRegionTest"), Times.Once(), "Data access layer should be hit exactly once since the caching is done in the same region");
        }

        [TestMethod]
        public void CacheExpirationTime_AbsoluteTTL_CacheHitInBeforeExpireTime()
        {
            // Arrange
            var dummy = new StubCachedClass();
            this.mockDataAccess.Setup(access => access.GetDummyData(It.IsAny<string>())).Returns((string s) => s);
            dummy.CachableAccessLayer = this.cachedDataAccess;

            // Act
            dummy.GetValueUsingAbsoluteTTL("AbsoluteTTLTest");
            dummy.GetValueUsingAbsoluteTTL("AbsoluteTTLTest");

            // Act
            this.mockDataAccess.Verify(access => access.GetDummyData("AbsoluteTTLTest"), Times.Once, "Data access layer should be hit exactly once within TTL timeframe");
        }

        /// <summary>
        /// Test that the cache is expired when waiting longer than ttl for second lookup
        /// </summary>
        [TestMethod]
        [TestCategory("Slow")]
        public void CacheExpirationTime_AbsoluteTTL_CacheMissAfterExpireTime()
        {
            // Arrange
            var dummy = new StubCachedClass();
            this.mockDataAccess.Setup(access => access.GetDummyData(It.IsAny<string>())).Returns((string s) => s);
            dummy.CachableAccessLayer = this.cachedDataAccess;

            // Act
            dummy.GetValueUsingAbsoluteTTL("AbsoluteTTLMissTest");
            Thread.Sleep(2000);
            dummy.GetValueUsingAbsoluteTTL("AbsoluteTTLMissTest");

            // Act
            this.mockDataAccess.Verify(access => access.GetDummyData("AbsoluteTTLMissTest"), Times.Exactly(2), "Data access layer should be hit exactly twice because second call is outside evictiontime");
        }

        /// <summary>
        /// Test that caching using sliding TTL will indeed cache second request.
        /// </summary>
        [TestMethod]
        public void CacheExpirationTime_SlidingTTL_CacheHit()
        {
            // Arrange
            var dummy = new StubCachedClass();
            this.mockDataAccess.Setup(access => access.GetDummyData(It.IsAny<string>())).Returns((string s) => s);
            dummy.CachableAccessLayer = this.cachedDataAccess;

            // Act
            dummy.GetValueUsingSlidingTTL("SlidingTTLTest");
            dummy.GetValueUsingSlidingTTL("SlidingTTLTest");

            // Act
            this.mockDataAccess.Verify(access => access.GetDummyData("SlidingTTLTest"), Times.Once, "Data access layer should be hit exactly once because second call is within evictiontime");
        }

        /// <summary>
        /// Test that caching using sliding TTL will expire after TTL time
        /// </summary>
        [TestMethod]
        [TestCategory("Slow")]
        public void CacheExpirationTime_SlidingTTL_CacheMiss()
        {
            // Arrange
            var dummy = new StubCachedClass();
            this.mockDataAccess.Setup(access => access.GetDummyData(It.IsAny<string>())).Returns((string s) => s);
            dummy.CachableAccessLayer = this.cachedDataAccess;

            // Act
            dummy.GetValueUsingSlidingTTL("SlidingTTLMissTest");
            Thread.Sleep(2000);
            dummy.GetValueUsingSlidingTTL("SlidingTTLMissTest");

            // Act
            this.mockDataAccess.Verify(access => access.GetDummyData("SlidingTTLMissTest"), Times.Exactly(2), "Data access layer should be hit exactly twice because second call is outside evictiontime");
        }

        /// <summary>
        /// Test that caching using sliding TTL will renew TTL if requested before expire time
        /// </summary>
        [TestMethod]
        [TestCategory("Slow")]
        public void CacheExpirationTime_SlidingTTL_CacheTTLRenewed()
        {
            // Arrange
            var dummy = new StubCachedClass();
            this.mockDataAccess.Setup(access => access.GetDummyData(It.IsAny<string>())).Returns((string s) => s);
            dummy.CachableAccessLayer = this.cachedDataAccess;

            // Act
            dummy.GetValueUsingSlidingTTL("SlidingTTLRenewTest");
            Thread.Sleep(1000);
            dummy.GetValueUsingSlidingTTL("SlidingTTLRenewTest");
            Thread.Sleep(1000);
            dummy.GetValueUsingSlidingTTL("SlidingTTLRenewTest");
            Thread.Sleep(1000);
            dummy.GetValueUsingSlidingTTL("SlidingTTLRenewTest");

            // Act
            this.mockDataAccess.Verify(access => access.GetDummyData("SlidingTTLRenewTest"), Times.Once, "Data access layer should be hit once because each request is within the sliding window expiretime");
        }

        /// <summary>
        /// Test that compile time validate returns false if the method sub-validations return false
        /// </summary>
        [TestMethod]
        public void CompileTimeValidate_InvalidCacheMethod_ReturnsFalse()
        {
            // Arrange
            var mock = new Mock<IRequirementsValidator>();
            mock.Setup(instance => instance.IsValidCacheMethod(It.IsAny<MethodBase>())).Returns(false);
            var cachedType = typeof(StubCachedClass);
            var methodToCache = cachedType.GetMethod("GetValue");
            var aspectInstance = new CacheAspect(mock.Object);

            // Act
            var actualResult = aspectInstance.CompileTimeValidate(methodToCache);

            // Assert
            mock.VerifyAll();
            Assert.IsFalse(actualResult);
        }

        /// <summary>
        /// Test that compile time validate returns false if the method sub-validations return false
        /// </summary>
        [TestMethod]
        public void CompileTimeValidate_InvalidExpirationTime_ReturnsFalse()
        {
            // Arrange
            var mock = new Mock<IRequirementsValidator>();
            mock.Setup(instance => instance.IsValidCacheMethod(It.IsAny<MethodBase>())).Returns(true);
            mock.Setup(instance => instance.IsValidCacheParameters(It.IsAny<MethodBase>())).Returns(true);
            mock.Setup(instance => instance.IsValidCacheKeyparameters(It.IsAny<MethodBase>(), It.IsAny<ParameterInfo[]>())).Returns(true);
            mock.Setup(instance => instance.IsValidExpirationTimes(It.IsAny<MethodBase>(), It.IsAny<int>(), It.IsAny<int>())).Returns(false);
            var cachedType = typeof(StubCachedClass);
            var methodToCache = cachedType.GetMethod("GetValue");
            var aspectInstance = new CacheAspect(mock.Object);
            aspectInstance.CompileTimeInitialize(methodToCache, new AspectInfo());

            // Act
            var actualResult = aspectInstance.CompileTimeValidate(methodToCache);

            // Assert
            mock.VerifyAll();
            Assert.IsFalse(actualResult);
        }

        /// <summary>
        /// Test that compile time validate returns false if the parameters sub-validations return false
        /// </summary>
        [TestMethod]
        public void CompileTimeValidate_InvalidCacheParameters_ReturnsFalse()
        {
            // Arrange
            var mock = new Mock<IRequirementsValidator>();
            mock.Setup(instance => instance.IsValidCacheMethod(It.IsAny<MethodBase>())).Returns(true);
            mock.Setup(instance => instance.IsValidCacheParameters(It.IsAny<MethodBase>())).Returns(false);
            var cachedType = typeof(StubCachedClass);
            var methodToCache = cachedType.GetMethod("GetValue");
            var aspectInstance = new CacheAspect(mock.Object);

            // Act
            var actualResult = aspectInstance.CompileTimeValidate(methodToCache);

            // Assert
            mock.VerifyAll();
            Assert.IsFalse(actualResult);
        }

        /// <summary>
        /// Test that compile time validate returns false if the cache-key sub-validations return false
        /// </summary>
        [TestMethod]
        public void CompileTimeValidate_InvalidCacheKeyParameters_ReturnsFalse()
        {
            // Arrange
            var mock = new Mock<IRequirementsValidator>();
            mock.Setup(instance => instance.IsValidCacheMethod(It.IsAny<MethodBase>())).Returns(true);
            mock.Setup(instance => instance.IsValidCacheParameters(It.IsAny<MethodBase>())).Returns(true);
            mock.Setup(instance => instance.IsValidCacheKeyparameters(It.IsAny<MethodBase>(), It.IsAny<ParameterInfo[]>())).Returns(false);

            var cachedType = typeof(StubCachedClass);
            var methodToCache = cachedType.GetMethod("GetValue");
            var aspectInstance = new CacheAspect(mock.Object);

            // Act
            aspectInstance.CompileTimeInitialize(methodToCache, new AspectInfo());
            var actualResult = aspectInstance.CompileTimeValidate(methodToCache);

            // Assert
            mock.VerifyAll();
            Assert.IsFalse(actualResult);
        }

        /// <summary>
        /// Test that compile time validate returns true if all sub-validations return true
        /// </summary>
        [TestMethod]
        public void CompileTimeValidate_ValidMethod_ReturnsTrue()
        {
            // Arrange
            var mock = new Mock<IRequirementsValidator>();
            mock.Setup(instance => instance.IsValidCacheMethod(It.IsAny<MethodBase>())).Returns(true);
            mock.Setup(instance => instance.IsValidCacheParameters(It.IsAny<MethodBase>())).Returns(true);
            mock.Setup(instance => instance.IsValidCacheKeyparameters(It.IsAny<MethodBase>(), It.IsAny<ParameterInfo[]>())).Returns(true);
            mock.Setup(instance => instance.IsValidExpirationTimes(It.IsAny<MethodBase>(), It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            var cachedType = typeof(StubCachedClass);
            var methodToCache = cachedType.GetMethod("GetValue");
            var aspectInstance = new CacheAspect(mock.Object);

            // Act
            aspectInstance.CompileTimeInitialize(methodToCache, new AspectInfo());
            var actualResult = aspectInstance.CompileTimeValidate(methodToCache);

            // Assert
            mock.VerifyAll();
            Assert.IsTrue(actualResult);
        }
    }
}
