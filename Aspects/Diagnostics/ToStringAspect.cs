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
        private List<PropertyInfo> primitivePublicProperties;

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

            this.primitivePublicProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => (
                    property.PropertyType.IsPrimitive ||
                    property.PropertyType.IsValueType ||
                    renderedNonPrimitiveTypes.Contains(property.PropertyType) ||
                    (property.PropertyType.GetGenericArguments().Any(t => t.IsValueType || t.IsPrimitive))
                    ) && property.CanRead)
                .ToList();
        }

        /// <summary>
        /// Initializes the aspect instance. This will instantiate the delegate that will be invoked when ToString is invoked.
        /// </summary>
        public override void RuntimeInitializeInstance()
        {
            this.toStringDelegate = () =>
            {
                StringBuilder builder = new StringBuilder();
                foreach (var property in this.primitivePublicProperties)
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
        [IntroduceMember(OverrideAction = MemberOverrideAction.OverrideOrIgnore)]
        public override string ToString()
        {
            return this.toStringDelegate.Invoke();
        }
    }
}
