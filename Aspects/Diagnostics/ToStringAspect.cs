// <copyright file="ToStringAspect.cs">
// 
// </copyright>
// <author></author>
// <email></email>
// <date></date>
// <summary></summary>
namespace Aspects.Diagnostics
{
    using PostSharp.Aspects;
    using PostSharp.Aspects.Advices;
    using PostSharp.Extensibility;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// An aspect that implements the ToString method on the addorned class using all public properties that are not reference types.
    /// </summary>
    [Serializable]
    [MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Strict)]
    public class ToStringAspect : InstanceLevelAspect
    {
        /// <summary>
        /// The primitive public properties of the type.
        /// </summary>
        private List<PropertyInfo> publicPropertiesForRendering;

        /// <summary>
        /// A list of non-primitive types that have a reasonable default print form, so they are included in the ToString implementation.
        /// </summary>
        private static readonly IList<Type> renderedNonPrimitiveTypes = new List<Type> { typeof(string), typeof(DateTime), typeof(DateTime?) };

        /// <summary>
        /// The actual delegate that is executed when tostring is called.
        /// </summary>
        [NonSerialized]
        private Func<string> toStringDelegate;

        /// <summary>
        /// Method invoked at build time to initialize the instance fields of the current aspect.
        /// In this particular case we build a list of public properties that should be "rendered" in the ToString implementation.
        /// </summary>
        /// <param name="type">Type to which the current aspect is applied</param>
        /// <param name="aspectInfo">Reserved for future usage.</param>
        public override void CompileTimeInitialize(Type type, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(type, aspectInfo);

            this.publicPropertiesForRendering = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => (
                    property.PropertyType.IsPrimitive ||
                    property.PropertyType.IsValueType ||
                    renderedNonPrimitiveTypes.Contains(property.PropertyType) ||
                    (property.PropertyType.GetGenericArguments().Any(t => t.IsValueType || t.IsPrimitive)) ||
                    this.HasOverridenToString(property.PropertyType) ||
                    this.HasToStringAspectAttribute(property.PropertyType)
                    ) && property.CanRead)
                .ToList();
        }

        /// <summary>
        /// Validate compiletime behvaior of the aspect
        /// </summary>
        /// <param name="type">The type the aspect is applied to</param>
        /// <returns>true if validation passes, otherwise false (if false the aspect won't be applied)</returns>
        public override bool CompileTimeValidate(Type type)
        {
            // Makes sure that we only override ToString where it isnt already declared.
            return type.GetMethod("ToString", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public) == null;
        }

        /// <summary>
        /// Initializes the aspect instance. This will instantiate the delegate that will be invoked when ToString is invoked.
        /// </summary>
        public override void RuntimeInitializeInstance()
        {
            this.toStringDelegate = () =>
            {
                StringBuilder builder = new StringBuilder();
                foreach (var property in this.publicPropertiesForRendering)
                {
                    var value = property.GetValue(this.Instance) ?? "Is Null";
                    builder.AppendLine(string.Format("{0} : {1}", property.Name, value.ToString()));
                }

                return builder.ToString();
            };
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <remarks>
        ///  All public, primitives instance properties that are readable will be be included in the string.
        ///  Note that the nullable types with no value will be printed as "Is Null" in the value.
        /// </remarks>
        /// <example>
        /// Exampleoutput:
        /// MyString : "Omg This is cool"\r\n
        /// MyInteger : 42\r\n
        /// </example>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        [IntroduceMember(OverrideAction = MemberOverrideAction.OverrideOrIgnore, IsVirtual = true)]
        public override string ToString()
        {
            return this.toStringDelegate.Invoke();
        }

        /// <summary>
        /// Determines if there is a ToString override on the given type (i.e. it is not object.ToString()) - if there is we can assume it can be "printed" to a string in a sensible way.
        /// </summary>
        /// <param name="propertyType">The type that should be tested for a tostring override</param>
        /// <returns>True if ToString is implemented/overriden false otherwise</returns>
        private bool HasOverridenToString(Type propertyType)
        {
            var methodInfo = propertyType.GetMethod("ToString");
            if (methodInfo.GetBaseDefinition().DeclaringType != methodInfo.DeclaringType)
            {
                return true;
            }

            // If the ToString method is overriden somewhere in the hierarchy that is not on System.Object - then just use that ToString Implementation
            if (methodInfo.GetBaseDefinition().DeclaringType.Name != "Object")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if the type is annotated with this attribute, if it is ToString will be implemented for it once the postsharp weaver has done its work.
        /// </summary>
        /// <param name="propertyType">The type that should be tested for an aspect attribution</param>
        /// <returns>True if there if this aspect is applied to the type, false otherwise</returns>
        /// <remarks>The reasonsing behind doing the code like this is that the weaver might not have gotten arround to implementing to string on the type when it is referenced here so the ToString override test won't nessecarily work.</remarks>
        private bool HasToStringAspectAttribute(Type propertyType)
        {
            return propertyType.GetCustomAttributes<ToStringAspect>(true).Any();
        }
    }
}
