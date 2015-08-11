namespace Aspects.UnitTest
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Stubs;

    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RuntimeMemoryCacheAdapterTest
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
            this.mockDataAccess = new Mock<IDummyInterface>(MockBehavior.Strict);
            this.cachedDataAccess = this.mockDataAccess.Object;
        }

        /// <summary>
        /// Test that a null return value is not cached.
        /// </summary>
        [TestMethod]
        public void CacheTest_NullOutput_NotCached()
        {
            // Arrange
            var dummy = new StubCachedClass();
            this.mockDataAccess.Setup(access => access.GetDummyData(It.IsAny<string>())).Returns((string s) => null);
            dummy.CachableAccessLayer = this.cachedDataAccess;

            // Act
            dummy.GetValue("NullReturnTypeOnThis");
            dummy.GetValue("NullReturnTypeOnThis");

            // Assert
            this.mockDataAccess.Verify(access => access.GetDummyData(It.IsAny<string>()), Times.Exactly(2), "Data access layer should be hit two times because the first should return null and therefore not be cached");
        }
    }
}
