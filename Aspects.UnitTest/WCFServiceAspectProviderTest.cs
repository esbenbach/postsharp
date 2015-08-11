using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aspects.Services;
using Aspects.Diagnostics;

namespace Aspects.UnitTest
{
    [TestClass]
    public class WCFServiceAspectProviderTest
    {
        [TestMethod]
        public void ProvideAspects_ProvideMethodLogAspect()
        {
            // Arrange
            var currentAssembly = this.GetType().Assembly;
            var providerTestTarget = new WCFServiceLoggingAspectProvider();

            // Act
            var providedAspects = providerTestTarget.ProvideAspects(currentAssembly);
            var methodsWithAppliedAspects = providedAspects.Where(aspect => ((MethodInfo)aspect.TargetElement).Name == "DoStuffMethod");

            // Assert
            Assert.IsNotNull(methodsWithAppliedAspects.Select(item => item.Aspect).OfType<MethodLogAspect>().FirstOrDefault(), "MethodLog Aspect was not applied to the method DoStuffMethod");
        }

        [TestMethod]
        public void ProvideAspects_ProvidesOneAspect()
        {
            // Arrange
            var currentAssembly = this.GetType().Assembly;
            var providerTestTarget = new WCFServiceLoggingAspectProvider();

            // Act
            var providedAspects = providerTestTarget.ProvideAspects(currentAssembly);
            var methodsWithAppliedAspects = providedAspects.Where(aspect => ((MethodInfo)aspect.TargetElement).Name == "DoStuffMethod");

            // Assert
            Assert.IsTrue(methodsWithAppliedAspects.Count() == 1, "One aspect should be provided for method without aspects already provided.");
        }

        [TestMethod]
        public void ProvideAspects_ProvidesOnlyPublicMethods()
        {
            // Arrange
            var currentAssembly = this.GetType().Assembly;
            var providerTestTarget = new WCFServiceLoggingAspectProvider();

            // Act
            var providedAspects = providerTestTarget.ProvideAspects(currentAssembly);
            var privateMethods = providedAspects.Where(aspect => ((MethodInfo)aspect.TargetElement).Name == "DoPrivateStuffMethod");
            var internalMethods = providedAspects.Where(aspect => ((MethodInfo)aspect.TargetElement).Name == "DoInternalStuffMethod");
            var protectedMethods = providedAspects.Where(aspect => ((MethodInfo)aspect.TargetElement).Name == "DoProtectedStuffMethod");

            // Assert
            Assert.IsTrue(privateMethods.Count() == 0, "No aspects should be applied for private method");
            Assert.IsTrue(internalMethods.Count() == 0, "No aspects should be applied for internal method");
            Assert.IsTrue(protectedMethods.Count() == 0, "No aspects should be applied for protected method");
        }

        [ServiceContract]
        private interface IServiceInterfaceTest
        {
            [OperationContract]
            void DoStuffMethod();
        }

        // This inherting interface is here to provoke a runtime error that will be present in all of the tests if the provider is wrong.
        // Writing a test for it is kinda pointless in that regard.
        [ServiceContract]
        private interface IInheritingInterface : IServiceInterfaceTest
        {
            [OperationContract]
            void DoStuffMethod2();
        }

        private class ServiceImplementation : IInheritingInterface
        {
            public void DoStuffMethod()
            {
                throw new NotImplementedException();
            }

            public void DoStuffMethod2()
            {
                throw new NotImplementedException();
            }

            private void DoPrivateStuffMethod()
            {
                throw new NotImplementedException();
            }

            internal void DoInternalStuffMethod()
            {
                throw new NotImplementedException();
            }

            protected void DoProtectedStuffMethod()
            {
                throw new NotImplementedException();
            }
        }
    }
}
