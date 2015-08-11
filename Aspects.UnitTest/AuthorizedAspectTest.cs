using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aspects.Security;
using System.Security.Principal;
using System.Threading;

namespace Aspects.UnitTest
{
    [TestClass]
    public class AuthorizedAspectTest
    {
        /// <summary>
        /// Initializes the test by setting up a default identity and principal.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            // Construct a new identity + principal
            GenericIdentity identity = new GenericIdentity("TestUser");
            GenericPrincipal principal = new GenericPrincipal(identity, new[] { "TestRole", "AnotherTestRoleJustToHaveMultipleRoles" });
            Thread.CurrentPrincipal = principal;
        }

        /// <summary>
        /// Removes the current principal to not screw up other tests
        /// </summary>
        [ClassCleanup]
        public static void Cleanup()
        {
            Thread.CurrentPrincipal = null;
        }

        /// <summary>
        /// Execute an authorized method where the current principal has the role.
        /// </summary>
        [TestMethod]
        public void Authorized_HasRole_ReturnsTrue()
        {
            // Arrange
            var target = new StubAuthorizedClass();

            // Act
            bool result = target.WithAuthorizationInTestRole();

            // Assert 
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Authorized_MultipleRolesMet_ReturnsTrue()
        {
            // Arrange
            var target = new StubAuthorizedClass();

            // Act
            bool result = target.WithAuthorizationMultipleRoles();

            // Assert 
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Execute an authorized method where the current principal does not have the role
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(UnauthorizedAccessException))]
        public void Authorized_MissingRole_ThrowsException()
        {
            // Arrange
            var target = new StubAuthorizedClass();

            // Assert & Assert
            target.WithAuthorizationInTestRole2();
        }

        [TestMethod]
        public void AuhtorizedAspect_Constructor_SetsRole()
        {
            var aspect = new AuthorizedAspect("AwesomeRole");
            Assert.AreEqual<string>("AwesomeRole", aspect.RequiredRole, "Role should be set when passing a string to the aspect constructor");
        }

        /// <summary>
        /// An internal stub class used to apply the aspect to something!
        /// </summary>
        private class StubAuthorizedClass
        {
            [AuthorizedAspect("TestRole")]
            public bool WithAuthorizationInTestRole()
            {
                return true;
            }

            [AuthorizedAspect("TestRole2")]
            public bool WithAuthorizationInTestRole2()
            {
                return true;
            }

            [AuthorizedAspect("TestRole")]
            [AuthorizedAspect("AnotherTestRoleJustToHaveMultipleRoles")]
            public bool WithAuthorizationMultipleRoles()
            {
                return true;
            }
        }
    }
}
