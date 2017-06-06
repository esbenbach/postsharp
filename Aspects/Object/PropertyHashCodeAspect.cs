namespace Aspects.Object
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using PostSharp.Aspects;
    using PostSharp.Aspects.Advices;
    using PostSharp.Extensibility;

    /// <summary>
    /// An aspect that auto implements the GetHashCode method as an XORed chain of each property GetHashCode.
    /// </summary>
    [Serializable]
    [MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Multicast)]
    [LinesOfCodeAvoided(1)]
    public class PropertyHashCodeAspect : InstanceLevelAspect
    {
        /// <summary>The properties that should be used when calculating the hashcode</summary>
        private List<PropertyInfo> properties;

        private readonly uint startingHash;

        private readonly uint constantFactor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyHashCodeAspect"/> class.
        /// </summary>
        /// <param name="hashStartingValue">(Optional) The hash starting value.</param>
        /// <param name="constantFactor">(Optional) The constant factor.</param>
        public PropertyHashCodeAspect(uint hashStartingValue = 2166136261, uint constantFactor = 402653189)
        {
            this.startingHash = hashStartingValue;
            this.constantFactor = constantFactor;
        }

        /// <summary>
        /// At compiletime, validate that the GetHashCode method has not already been overridden in the applied class
        /// </summary>
        /// <param name="type">The type the aspect is applied to</param>
        /// <returns>
        /// true if validation passes, otherwise false (if false the aspect won't be applied)
        /// </returns>
        public override bool CompileTimeValidate(Type type)
        {
            // Makes sure that we only override GetHashCode where it isnt already declared.
            return type.GetMethod(nameof(this.GetHashCode), BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public) == null;
        }

        /// <summary>
        /// Method invoked at build time to initialize the instance fields of the current aspect.
        /// In this particular case we build a list of all properties and fields that should be included when generating the hashcode.
        /// </summary>
        /// <param name="type">Type to which the current aspect is applied</param>
        /// <param name="aspectInfo">Reserved for future usage.</param>
        public override void CompileTimeInitialize(Type type, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(type, aspectInfo);

            this.properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(property => property.CanRead)
                .ToList();
        }

        /// <summary>
        /// Returns an <see cref="System.Int32" /> that represents the hashcode of this object
        /// </summary>
        /// <remarks>
        ///  Just loops through all properties and hashes the properties based on the suggestion from Jon Skeet on https://stackoverflow.com/a/263416
        /// </remarks>
        /// <returns>
        /// A hashcode
        /// </returns>
        [IntroduceMember(OverrideAction = MemberOverrideAction.OverrideOrIgnore, IsVirtual = true)]
        public override int GetHashCode()
        {
            // Okay ideally, this would instead be code generated at compile that does the "this.property.GetHashCode()" chaining
            // But I dont have any idea on how to let Roslyn do that, or the CodeDom or some such thing, so this will have to do.

            unchecked // Overflow is fine, just wrap
            {
                var hashCode = (int)this.startingHash;

                foreach (var item in this.properties)
                {
                    var itemHash = item.GetValue(this.Instance)?.GetHashCode();
                    if (itemHash != null)
                    {
                        hashCode = (hashCode * (int)this.constantFactor) + itemHash.Value;
                    }
                }

                return hashCode;
            }
        }
    }
}