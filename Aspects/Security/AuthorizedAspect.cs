// <copyright file="AuthorizedAspect.cs">
// 
// </copyright>
// <author></author>
// <email></email>
// <date></date>
// <summary></summary>
namespace Aspects.Security
{
    using PostSharp.Aspects;
    using PostSharp.Aspects.Dependencies;
    using PostSharp.Extensibility;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    /// An aspect that can be use to authorize usage against the current principal.
    /// </summary>
    [Serializable]
    [ProvideAspectRole(StandardRoles.Security)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Threading)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Caching)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Tracing)]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true)]
    [MulticastAttributeUsage(AllowMultiple = true)]
    [AspectTypeDependency(AspectDependencyAction.Commute, typeof(AuthorizedAspect))]
    public sealed class AuthorizedAspect : OnMethodBoundaryAspect
    {
        /// <summary>
        /// The role required to execute the adorned method.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The property is a named parameter in the attribute and must describe its function as such and not a property")]
        public string RequiredRole { get; set; }

        /// <summary>
        /// An aspect attribute that will verify if the current threads principal has the required role. If not an UnauthorizedAccessException is thrown when invoking the method.
        /// </summary>
        /// <param name="requiredRole">The role required to execute the adorned method</param>
        /// <exception cref="UnauthorizedAccessException">Thrown if the required role is not matched</exception>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1642:ConstructorSummaryDocumentationMustBeginWithStandardText", Justification = "The constructor is an attribute/aspect constructor and should describe the attributes function and not the constructors function.")]
        public AuthorizedAspect(string requiredRole)
        {
            this.RequiredRole = requiredRole;
        }

        /// <summary>
        /// Validates the aspect use on compile time. An error message will be written to the build log if the validation fails.
        /// </summary>
        /// <param name="method">The method the aspect is applied to</param>
        /// <returns>true if validation passes, otherwise false.</returns>
        public override bool CompileTimeValidate(System.Reflection.MethodBase method)
        {
            if (!string.IsNullOrWhiteSpace(this.RequiredRole))
            {
                return base.CompileTimeValidate(method);
            }

            Message.Write(method, SeverityType.Error, "CX1001", "A role must be provided for the authorizedaspect to make sense, provide a role requirement or remove the aspect");

            return false;
        }

        /// <summary>
        /// Veries if the current principal has the required role, if not an UnauthorizedAccessException is thrown.
        /// </summary>
        /// <param name="args">Event arguments specifying which method
        /// is being executed, which are its arguments, and how should the execution continue
        /// after the execution of <see cref="M:PostSharp.Aspects.IOnMethodBoundaryAspect.OnEntry(PostSharp.Aspects.MethodExecutionArgs)" />.</param>
        /// <exception cref="UnauthorizedAccessException">Thrown if the required role is not matched</exception>
        public override void OnEntry(MethodExecutionArgs args)
        {
            if (!Thread.CurrentPrincipal.IsInRole(this.RequiredRole))
            {
                var exception = new UnauthorizedAccessException(string.Format("The user is not authorized to use this function. It requires the role: {0}", this.RequiredRole));
                var data = new Dictionary<string, string>();
                data.Add("User", Thread.CurrentPrincipal.Identity.Name);
                data.Add("Authenticated", Thread.CurrentPrincipal.Identity.IsAuthenticated.ToString());
                data.Add("AuthenticationType", Thread.CurrentPrincipal.Identity.AuthenticationType);
                data.Add("RequiredRole", this.RequiredRole);
                throw exception;
            }
        }
    }
}
