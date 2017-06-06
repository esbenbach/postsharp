namespace Aspects.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aspects.UnitTest.Stubs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///
    /// </summary>
    [TestClass]
    public class PropertyHashCodeAspectTest
    {
        [TestMethod]
        public void GetHashCode_ReturnsCorrectHashCode()
        {
            var stub = new StubWithProperties() { MyFirstProperty = "AString", MySecondProperty = 123 };

            var expectedHash = stub.MyFirstProperty.GetHashCode() ^ stub.MySecondProperty.GetHashCode();

            Assert.AreEqual(expectedHash, stub.GetHashCode());
        }

        [TestMethod]
        public void GetHashCode_OrderIsIrrelevant()
        {
            var stub = new StubWithProperties() { MyFirstProperty = "AString", MySecondProperty = 123 };

            var expectedHash =  stub.MySecondProperty.GetHashCode() ^ stub.MyFirstProperty.GetHashCode();

            Assert.AreEqual(expectedHash, stub.GetHashCode());
        }
    }
}