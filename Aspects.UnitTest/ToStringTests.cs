using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aspects.Diagnostics;
using System.Globalization;

namespace Aspects.UnitTest
{
    [TestClass]
    public class ToStringTests
    {
        [TestMethod]
        public void StringProperty_WithValue_RenderedAsKeyValuePair()
        {
            var resultString = new ToStringStub() { ImAString = "AStringOfSorts" }.ToString();

            Assert.AreEqual("ImAString : AStringOfSorts\r\n", resultString);
        }

        [TestMethod]
        public void NonPrimitiveProperty_HasToStringImplementation_RenderedAsKeyValuePair()
        {
            var resultString = new ToStringStubWithToStringProperty() { HasToString = new ToStringStubWithToStringImplementation() }.ToString();

            Assert.AreEqual("HasToString : This is an overriden string\r\n", resultString);
        }

        [TestMethod]
        public void NonPrimitiveProperty_HasToStringAspect_RenderedAsKeyValuePair()
        {
            var resultString = new ToStringStubWithAspectProperty() { HasAspect = new ToStringStubWithToStringAspect() }.ToString();

            Assert.AreEqual("HasAspect : SomethingToRender : An Aspect ToString Implementation\r\n\r\n", resultString);
        }

        [TestMethod]
        public void StringProperty_NoValue_RenderAsNullValued()
        {
            var resultString = new ToStringStub().ToString();

            Assert.AreEqual("ImAString : Is Null\r\n", resultString);
        }

        [TestMethod]
        public void DateTimeProperty_HasValue_Rendered()
        {
            var resultString = new ToStringStubWithDate() { Date = new DateTime(2000, 1, 1) }.ToString();

            var expected = string.Format(CultureInfo.CurrentCulture, "Date : {0}\r\n", new DateTime(2000, 1, 1));

            Assert.AreEqual(expected, resultString);
        }

        [TestMethod]
        public void NullableDateTimeProperty_HasValue_Rendered()
        {
            var resultString = new ToStringStubWithNullableDate() { NullableDate = new DateTime(2000, 1, 1) }.ToString();

            var expected = string.Format(CultureInfo.CurrentCulture, "NullableDate : {0}\r\n", new DateTime(2000, 1, 1));

            Assert.AreEqual(expected, resultString);
        }

        [TestMethod]
        public void NullableIntProperty_HasValue_Rendered()
        {
            var resultString = new ToStringStubWithNullableInt() { NullableInt = 10 }.ToString();

            Assert.AreEqual("NullableInt : 10\r\n", resultString);
        }

        [ToStringAspect]
        private class ToStringStub
        {
            public string ImAString { get; set; }
        }

        [ToStringAspect]
        private class ToStringStubWithDate
        {
            public DateTime Date { get; set; }
        }

        [ToStringAspect]
        private class ToStringStubWithNullableDate
        {
            public DateTime? NullableDate { get; set; }
        }

        [ToStringAspect]
        private class ToStringStubWithNullableInt
        {
            public int? NullableInt { get; set; }
        }

        [ToStringAspect]
        private class ToStringStubWithToStringProperty
        {
            public ToStringStubWithToStringImplementation HasToString { get; set; }
        }

        [ToStringAspect]
        private class ToStringStubWithAspectProperty
        {
            public ToStringStubWithToStringAspect HasAspect { get; set; }
        }

        private class ToStringStubWithToStringImplementation
        {
            public override string ToString()
            {
                return "This is an overriden string";
            }
        }

        [ToStringAspect]
        private class ToStringStubWithToStringAspect
        {
            public string SomethingToRender { get { return "An Aspect ToString Implementation"; } }
        }
    }
}
