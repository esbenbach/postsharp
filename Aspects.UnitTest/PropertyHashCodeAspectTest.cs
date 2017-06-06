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
            var stub = new StubWithProperties() { MyFirstProperty = "AString", MySecondProperty = 123, MyDateProperty = new DateTime(2010, 10, 12), MyThirdProperty = true };
            Assert.AreEqual(1033763843, stub.GetHashCode());
        }
    }
}