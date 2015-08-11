// <copyright file="WCFServiceAspectProvider.cs"></copyright>
// <author></author>
// <email></email>
// <date></date>
// <summary></summary>
namespace Aspects.Services
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using PostSharp.Aspects;
    using PostSharp.Extensibility;
    using Diagnostics;
    using System.ServiceModel;

    /// <summary>
    /// Provides aspects for WCF Service Methods currently the MethodLogAspect.
    /// </summary>
    [Serializable]
    [MulticastAttributeUsage(AllowExternalAssemblies = true)]
    public class WCFServiceLoggingAspectProvider : AssemblyLevelAspect, IAspectProvider
    {
        /// <summary>
        /// The aspect repository service from postsharp
        /// </summary>
        private static readonly IAspectRepositoryService AspectRepositoryService;

        /// <summary>
        /// Initializes static members of the <see cref="WCFServiceLoggingAspectProvider"/> class, used only to initialise the repository needed during build.
        /// </summary>
        static WCFServiceLoggingAspectProvider()
        {
            if (PostSharpEnvironment.IsPostSharpRunning)
            {
                AspectRepositoryService = PostSharpEnvironment.CurrentProject.GetService<IAspectRepositoryService>();
            }
        }

        /// <summary>
        /// Provides the aspect MethodLogAspect to all types in the assembly that implements an interface with the WCF ServiceContract attribute.
        /// </summary>
        /// <param name="targetElement">Code element (<see cref="T:System.Reflection.Assembly" />, <see cref="T:System.Type" />,
        /// <see cref="T:System.Reflection.FieldInfo" />, <see cref="T:System.Reflection.MethodBase" />, <see cref="T:System.Reflection.PropertyInfo" />, <see cref="T:System.Reflection.EventInfo" />,
        /// <see cref="T:System.Reflection.ParameterInfo" />, or <see cref="T:PostSharp.Reflection.LocationInfo" />) to which the current aspect has been applied.</param>
        /// <returns>
        /// A set of aspect instances.
        /// </returns>
        public IEnumerable<AspectInstance> ProvideAspects(object targetElement)
        {
            var allMethodsImplementingInterface = GetServiceMethods((Assembly)targetElement);
            var methodAspectInstances = FilterAlreadyAppliedAspect<MethodLogAspect>(FilterAlreadyAppliedAttribute<MethodLogAspect>(allMethodsImplementingInterface)).Select(method => new AspectInstance(method, CreateMethodLog(method)));

            return methodAspectInstances;
        }

        /// <summary>
        /// Creates a new MethodLogAspect and initialise it.
        /// </summary>
        /// <param name="method">The method the aspect should be applied to</param>
        /// <returns>An initialized MethodLogAspect instance for the method</returns>
        private static MethodLogAspect CreateMethodLog(MethodInfo method)
        {
            var aspect = new MethodLogAspect();
            aspect.CompileTimeValidate(method);
            aspect.CompileTimeInitialize(method, new AspectInfo());
            return aspect;
        }

        /// <summary>
        /// Gets all WCF service methods in the assembly
        /// </summary>
        /// <param name="targetAssembly">The target assembly.</param>
        /// <returns>An enumerable of service methods</returns>
        private static IEnumerable<MethodInfo> GetServiceMethods(Assembly targetAssembly)
        {
            // Note: This basically assumes that all methods in a servicecontract is indeed an operationcontract. If that is not the case, well too bad!
            var typeInterfaceMap = from Type classType in targetAssembly.GetTypes()
                                   let interfaces = classType.GetInterfaces().Where(t => t.GetCustomAttributes<ServiceContractAttribute>().Any() && !t.GetCustomAttributes<GeneratedCodeAttribute>().Any())
                                   from Type interfaceType in interfaces
                                   where !classType.IsInterface
                                   select new { ClassType = classType, InterfaceType = interfaceType };

            return typeInterfaceMap.SelectMany(map => map.ClassType.GetInterfaceMap(map.InterfaceType).TargetMethods);
        }

        /// <summary>
        /// Filters already applied attributes from the list of methods
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="methods">The methods to filter for</param>
        /// <returns>An enmerable of service methods</returns>
        private static IEnumerable<MethodInfo> FilterAlreadyAppliedAttribute<TAttribute>(IEnumerable<MethodInfo> methods) where TAttribute : System.Attribute
        {
            return methods.Where(m => !m.GetCustomAttributes<TAttribute>().Any());
        }

        /// <summary>
        /// Filters already applied aspects from the list of methods
        /// </summary>
        /// <typeparam name="TAspect">The type of the aspect.</typeparam>
        /// <param name="methods">The methods.</param>
        /// <returns>MethodInfos where the given aspect is NOT present</returns>
        /// <remarks>
        /// This will only work during postsharp compile execution as the AspectRepositoryService is null otherwise. 
        /// It is possible to inject the service via the constructor so we could test this behavior, but in order to keep the compile time as low as possible
        /// we won't bother (it would just serve to remove a dependency during build time so we could test the public part of the class)
        /// </remarks>
        private static IEnumerable<MethodInfo> FilterAlreadyAppliedAspect<TAspect>(IEnumerable<MethodInfo> methods) where TAspect : PostSharp.Aspects.Aspect
        {
            if (AspectRepositoryService != null)
            {
                return methods.Where(method => !AspectRepositoryService.HasAspect(method, typeof(TAspect)));
            }
            else
            {
                return methods;
            }
        }
    }
}