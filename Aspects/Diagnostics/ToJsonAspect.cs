// <copyright file="ToJsonAspect.cs">
// </copyright>
// <author></author>
// <email></email>
// <date></date>
// <summary></summary>
namespace Aspects.Diagnostics
{
    using Newtonsoft.Json;
    using PostSharp.Aspects;
    using PostSharp.Aspects.Advices;
    using PostSharp.Extensibility;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// An aspect that when applied to an object will implement a ToString method that serialises as JSON.
    /// </summary>
    [Serializable]
    [MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Multicast)]
    [LinesOfCodeAvoided(1)]
    public class ToJsonAspect : InstanceLevelAspect
    {
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
        /// Method invoked at build time to initialize the instance fields of the current aspect. This method is invoked
        /// before any other build-time method.
        /// </summary>
        /// <param name="type">Type to which the current aspect is applied</param>
        /// <param name="aspectInfo">Reserved for future usage.</param>
        public override void CompileTimeInitialize(Type type, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(type, aspectInfo);
        }

        /// <summary>
        /// Validate compiletime behvaior of the aspect
        /// </summary>
        /// <param name="type">The type the aspect is applied to</param>
        /// <returns>
        /// true if validation passes, otherwise false (if false the aspect won't be applied)
        /// </returns>
        public override bool CompileTimeValidate(Type type)
        {
            // Makes sure that we only override ToString where it isnt already declared.
            return type.GetMethod("ToString", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public) == null;
        }

        /// <summary>
        /// Initializes the aspect instance. This method is invoked when all system elements of the aspect (like member imports)
        /// have completed.
        /// </summary>
        public override void RuntimeInitializeInstance()
        {
            this.toStringDelegate = () =>
            {
                return JsonConvert.SerializeObject(this.Instance, Formatting.Indented);
            };
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance as a JSON object.
        /// </summary>
        /// <remarks>
        ///  Internally this uses the Newtonsoft JSON library to serialize the object. Refer to its documentation for the gory details.
        /// </remarks>
        /// <returns>
        /// A JSON formatted <see cref="System.String" /> that represents this instance.
        /// </returns>
        [IntroduceMember(OverrideAction = MemberOverrideAction.OverrideOrIgnore, IsVirtual = true)]
        public override string ToString()
        {
            return this.toStringDelegate.Invoke();
        }
    }
}