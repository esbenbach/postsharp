
namespace Aspects.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Aspects.Caching;
    using Aspects.UnitTest.Stubs;

    /// <summary>
    /// Test class for cache requirements validation
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RequirementsValidatorTest
    {
        /// <summary>
        /// Test that constructors won't be acknowledged as methods to cache the result of
        /// </summary>
        [TestMethod]
        public void IsValidCacheMethod_ConstructorMethod_ReturnsFalse()
        {
            // Arrange
            var mock = new Mock<ICompileLogger>();
            var cachedType = typeof(StubCachedClass);
            var methodToValidate = cachedType.GetConstructor(new Type[] { });
            var validator = new RequirementsValidator(mock.Object);

            // Act
            var actualResult = validator.IsValidCacheMethod(methodToValidate);

            // Assert
            mock.Verify(action => action.WriteError(methodToValidate, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object[]>()), "No error logged for invalid constructor");
            Assert.IsFalse(actualResult, "Constructors cannot be used for caching so should return an error");
        }

        /// <summary>
        /// Test that void methods won't be acknowledged as methods to cache the result of
        /// </summary>
        [TestMethod]
        public void IsValidCacheMethod_VoidMethod_ReturnsFalse()
        {
            // Arrange
            var mock = new Mock<ICompileLogger>();
            var methodToValidate = this.GetType().GetMethod("ReturnVoid", BindingFlags.NonPublic | BindingFlags.Instance);
            var validator = new RequirementsValidator(mock.Object);

            // Act
            var actualResult = validator.IsValidCacheMethod(methodToValidate);

            // Assert
            mock.Verify(action => action.WriteError(methodToValidate, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object[]>()), "No error logged for invalid queryable method");
            Assert.IsFalse(actualResult, "Methods with queryable output cannot be used for caching");
        }

        /// <summary>
        /// Test that methods with a queryable return type won't be acknowledged as methods to cache the result of
        /// </summary>
        [TestMethod]
        public void IsValidCacheMethod_QueryableReturnType_ReturnsFalse()
        {
            // Arrange
            var mock = new Mock<ICompileLogger>();
            var methodToValidate = this.GetType().GetMethod("ReturnQueryable", BindingFlags.NonPublic | BindingFlags.Instance);
            var validator = new RequirementsValidator(mock.Object);

            // Act
            var actualResult = validator.IsValidCacheMethod(methodToValidate);

            // Assert
            mock.Verify(action => action.WriteError(methodToValidate, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object[]>()), "No error logged for invalid void method");
            Assert.IsFalse(actualResult, "Methods with void output cannot be used for caching");
        }

        /// <summary>
        /// Test that methods with a string return type will be acknowledged as methods to cache the result of
        /// </summary>
        [TestMethod]
        public void IsValidCacheMethod_ValidStringMethod_ReturnsTrue()
        {
            // Arrange
            var mock = new Mock<ICompileLogger>();
            var methodToValidate = this.GetType().GetMethod("StringType");
            var validator = new RequirementsValidator(mock.Object);

            // Act
            var actualResult = validator.IsValidCacheMethod(methodToValidate);

            // Assert
            Assert.IsTrue(actualResult, "Methods with string input and output are valid methods for caching");
        }

        /// <summary>
        /// Test that methods with an input parameter of type OUT won't be acknowledged as methods to cache the result of
        /// </summary>
        [TestMethod]
        public void IsValidCacheParameters_OutParameterInput_ReturnsFalse()
        {
            // Arrange
            var mock = new Mock<ICompileLogger>();
            var methodToValidate = this.GetType().GetMethod("OutParameter", BindingFlags.NonPublic | BindingFlags.Instance);
            var validator = new RequirementsValidator(mock.Object);

            // Act
            var actualResult = validator.IsValidCacheParameters(methodToValidate);

            // Assert
            mock.Verify(action => action.WriteError(methodToValidate, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object[]>()), "No error logged for invalid 'out' method");
            Assert.IsFalse(actualResult, "Methods with 'out' paramaters cannot be used for caching");
        }

        /// <summary>
        /// Test that input parameter that is a reference type won't be acknowledged as key building parameter
        /// </summary>
        [TestMethod]
        public void IsValidCacheKeyparameters_ReferenceTypeParameterInput_ReturnsFalse()
        {
            // Arrange
            var mock = new Mock<ICompileLogger>();
            var methodToValidate = this.GetType().GetMethod("ReferenceType", BindingFlags.NonPublic | BindingFlags.Instance);
            var validator = new RequirementsValidator(mock.Object);

            // Act
            var actualResult = validator.IsValidCacheKeyparameters(methodToValidate, methodToValidate.GetParameters());

            // Assert
            mock.Verify(action => action.WriteError(methodToValidate, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object[]>()), "No error logged for invalid key parameter method");
            Assert.IsFalse(actualResult, "Reference type paramaters cannot be used as keys for caching");
        }

        /// <summary>
        /// Test that input parameter of type string will be acknowledged as key building parameter
        /// </summary>
        [TestMethod]
        public void IsValidCacheKeyparameters_StringTypeParameterInput_ReturnsTrue()
        {
            // Arrange
            var mock = new Mock<ICompileLogger>();
            var methodToValidate = this.GetType().GetMethod("StringType", BindingFlags.NonPublic | BindingFlags.Instance);
            var validator = new RequirementsValidator(mock.Object);

            // Act
            var actualResult = validator.IsValidCacheKeyparameters(methodToValidate, methodToValidate.GetParameters());

            // Assert
            Assert.IsTrue(actualResult, "String type paramaters can be used as keys for caching");
        }

        /// <summary>
        /// Test that input parameter of a struct type won't be acknowledged as a key building parameter
        /// </summary>
        [TestMethod]
        public void IsValidCacheKeyparameters_StructParameter_ReturnsFalse()
        {
            // Arrange
            var mock = new Mock<ICompileLogger>();
            var methodToValidate = this.GetType().GetMethod("ReferenceTypeWithStruct", BindingFlags.NonPublic | BindingFlags.Instance);
            var validator = new RequirementsValidator(mock.Object);

            // Act
            var actualResult = validator.IsValidCacheKeyparameters(methodToValidate, methodToValidate.GetParameters());

            // Assert
            Assert.IsFalse(actualResult, "Struct paramaters cannot be used as keys for caching if they do not implement ICacheKey");
        }

        /// <summary>
        /// Test that input parameter of a struct type which implement ICacheKey will be acknowledged as a key building parameter
        /// </summary>
        [TestMethod]
        public void IsValidCacheKeyparameters_StructICacheKeyImplementation_ReturnsTrue()
        {
            // Arrange
            var mock = new Mock<ICompileLogger>();
            var methodToValidate = this.GetType().GetMethod("ReferenceTypeWithCachableStruct", BindingFlags.NonPublic | BindingFlags.Instance);
            var validator = new RequirementsValidator(mock.Object);

            // Act
            var actualResult = validator.IsValidCacheKeyparameters(methodToValidate, methodToValidate.GetParameters());

            // Assert
            Assert.IsTrue(actualResult, "Struct paramaters implementing ICacheKey should be allowed");
        }

        #region StubImplementations

        /// <summary>
        /// Dummy method returning void, should not be a valid method for caching
        /// </summary>
        private void ReturnVoid()
        {
        }

        /// <summary>
        /// Dummy method using "out" parameter, should not be a valid method for caching
        /// </summary>
        /// <param name="returnValue">The return value.</param>
        private void OutParameter(out string returnValue)
        {
            returnValue = "Somestring";
        }

        /// <summary>
        /// Method using struct not valid for key generation
        /// </summary>
        /// <param name="stub">The stub.</param>
        /// <returns>Empty string</returns>
        private string ReferenceTypeWithStruct(NotValidKeyStruct stub)
        {
            return string.Empty;
        }

        /// <summary>
        /// Method using struct valid for key generation
        /// </summary>
        /// <param name="stub">The stub.</param>
        /// <returns>Empty string</returns>
        private string ReferenceTypeWithCachableStruct(ValidKeyStruct stub)
        {
            return string.Empty;
        }

        /// <summary>
        /// Method using stub reference type as input, not valid for key generation
        /// </summary>
        /// <param name="stub">The stub.</param>
        /// <returns>Empty string</returns>
        private string ReferenceType(StubCachedClass stub)
        {
            return string.Empty;
        }

        /// <summary>
        /// Method using string as input and output, should be valid for caching and input should be valid as key
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns>input string</returns>
        private string StringType(string inputString)
        {
            return inputString;
        }

        /// <summary>
        /// Returns a queryable type, not a valid method for caching.
        /// </summary>
        /// <returns>An iqueryable</returns>
        private IQueryable ReturnQueryable()
        {
            return new List<object>().AsQueryable();
        }

        /// <summary>
        /// An empty struct, should not be valid as cache key parameter
        /// </summary>
        private struct NotValidKeyStruct
        {
        }

        /// <summary>
        /// An empty struct, implements ICacheKey and should be valid as cache key parameter.
        /// </summary>
        private struct ValidKeyStruct : ICacheKey
        {
            /// <summary>
            /// Gets the cache key.
            /// </summary>
            /// <returns>
            /// A string that can be used as a cache key
            /// </returns>
            public string GetCacheKey()
            {
                return "key";
            }
        }

        #endregion
    }
}
