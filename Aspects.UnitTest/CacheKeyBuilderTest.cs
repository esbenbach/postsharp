namespace Aspects.UnitTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Aspects.Caching;
    using Moq;
    using Stubs;

    [TestClass]
    public class CacheKeyBuilderTest
    {
        [TestMethod]
        public void CacheKeyBuilder_BuildKeyNoOverride_BuildKeyFromTypeName()
        {
            // Arrange
            var cachedType = typeof(StubCachedClass);
            var cachedMethod = cachedType.GetMethod("GetValue");
            var keyBuilder = new CacheKeyBuilder(cachedMethod);

            // Act
            var actualResult = keyBuilder.BuildCacheKey(new object[] { "awesome" });

            // Assert
            string expectedResult = "Aspects.UnitTest.Stubs.StubCachedClass.GetValue keyValue = 'awesome'";
            Assert.AreEqual(expectedResult, actualResult, "Building a cache key using no overrides should generate a valid key using all parameters and values");
        }

        [TestMethod]
        public void CacheKeyBuild_BuildKeyNullValue_BuildKeyNullValueIgnored()
        {
            // Arrange
            var cachedType = typeof(StubCachedClass);
            var cachedMethod = cachedType.GetMethod("GetValue");
            var keyBuilder = new CacheKeyBuilder(cachedMethod);

            // Act
            var actualResult = keyBuilder.BuildCacheKey(new object[] { null });

            // Assert
            string expectedResult = "Aspects.UnitTest.Stubs.StubCachedClass.GetValue keyValue = ''";
            Assert.AreEqual(expectedResult, actualResult, "Building a cache key with a parameter as null value should generate a valid key with the keyvalue replaced by the empty string");
        }

        [TestMethod]
        public void CacheKeyBuilder_BuildKeyPrefixOverride_BuildKeyUsingPrefixAndParameters()
        {
            // Arrange
            var cachedType = typeof(StubCachedClass);
            var cachedMethod = cachedType.GetMethod("GetValue");
            var keyBuilder = new CacheKeyBuilder(cachedMethod, "TestingPrefix");

            // Act
            var actualResult = keyBuilder.BuildCacheKey(new object[] { "awesome" });

            // Assert
            string expectedResult = "TestingPrefix keyValue = 'awesome'";
            Assert.AreEqual(expectedResult, actualResult, "Building a cache key using a key prefix override should generate a valid key using all parameters and values with the specified prefix");
        }

        [TestMethod]
        public void CacheKeyBuilder_BuildKeyPrefixParameter_BuildKeyUsingPrefixAndSelectedParameters()
        {
            // Arrange
            var cachedType = typeof(StubCachedClass);
            var cachedMethod = cachedType.GetMethod("GetValue2");
            var keyBuilder = new CacheKeyBuilder(cachedMethod, "TestingPrefix", "dummyParameter");

            // Act
            var actualResult = keyBuilder.BuildCacheKey(new object[] { "awesome", "notSoAwesome" });

            // Assert
            string expectedResult = "TestingPrefix dummyParameter = 'notSoAwesome'";
            Assert.AreEqual(expectedResult, actualResult, "Overriding prefix and parameter was should result in a key with the prefix and the value of the given parameters");
        }

        [TestMethod]
        public void CacheKeyBuilder_BuildKeyPrefixMultipleParameter_BuildKeyUsingPrefixAndParameters()
        {
            // Arrange
            var cachedType = typeof(StubCachedClass);
            var cachedMethod = cachedType.GetMethod("GetValue2");
            var keyBuilder = new CacheKeyBuilder(cachedMethod, "TestingPrefix", "dummyParameter,keyValue");

            // Act
            var actualResult = keyBuilder.BuildCacheKey(new object[] { "awesome", "notSoAwesome" });

            // Assert
            string expectedResult = "TestingPrefix keyValue = 'awesome' dummyParameter = 'notSoAwesome'";
            Assert.AreEqual(expectedResult, actualResult, "Overriding prefix and parameter was should result in a key with the prefix and the value of the given parameters");
        }

        [TestMethod]
        public void CacheKeyBuilder_BuildMultipleParameter_BuildKeyUsingParametersAndTypePrefix()
        {
            // Arrange
            var cachedType = typeof(StubCachedClass);
            var cachedMethod = cachedType.GetMethod("GetValue2");
            var keyBuilder = new CacheKeyBuilder(cachedMethod, parameters: "dummyParameter,keyValue");

            // Act
            var actualResult = keyBuilder.BuildCacheKey(new object[] { "awesome", "notSoAwesome" });

            // Assert
            string expectedResult = "Aspects.UnitTest.Stubs.StubCachedClass.GetValue2 keyValue = 'awesome' dummyParameter = 'notSoAwesome'";
            Assert.AreEqual(expectedResult, actualResult, "Overriding parameter (not prefix) was should result in a key with the type as prefix and the value of the given parameters");
        }

        [TestMethod]
        public void CacheKeyBuilder_BuildUsingICacheKey_KeyContainsMethodOutput()
        {
            // Arrange
            Mock<ICacheKey> mockCache = new Mock<ICacheKey>();
            mockCache.Setup(cache => cache.GetCacheKey()).Returns("MyCacheKey");
            ICacheKey cacheKey = mockCache.Object;
            var cachedType = typeof(StubCachedClass);
            var cachedMethod = cachedType.GetMethod("GetCachedItemImplementingCacheKey");
            string expectedResult = "Aspects.UnitTest.Stubs.StubCachedClass.GetCachedItemImplementingCacheKey inputItem = 'MyCacheKey'";

            // Act
            var keyBuilder = new CacheKeyBuilder(cachedMethod);
            var actualResult = keyBuilder.BuildCacheKey(new object[] { cacheKey });

            // Assert
            Assert.AreEqual(expectedResult, actualResult, "Returned key does not contain result of GetCacheKey method");
        }

        [TestMethod]
        public void CacheKeyBuilder_BuildUsingICacheKey_BuilderCallsImplementation()
        {
            // Arrange
            Mock<ICacheKey> mockCache = new Mock<ICacheKey>();
            mockCache.Setup(cache => cache.GetCacheKey()).Returns("MyCacheKey");
            ICacheKey cacheKey = mockCache.Object;
            var cachedType = typeof(StubCachedClass);
            var cachedMethod = cachedType.GetMethod("GetCachedItemImplementingCacheKey");

            // Act
            var keyBuilder = new CacheKeyBuilder(cachedMethod);
            var actualResult = keyBuilder.BuildCacheKey(new object[] { cacheKey });

            // Assert
            mockCache.Verify(mock => mock.GetCacheKey(), Times.Once(), "Cache key implementation must be called exactly once");
        }
    }
}
