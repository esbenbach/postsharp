// <copyright file="CachableInputAspect.cs" company="Infosoft AS">
// Copyright (c) 2012 All Rights Reserved
// </copyright>
// <author>esbbach</author>
// <email></email>
// <date>10/15/2012 6:40:21 PM</date>
// <summary></summary>
namespace Infosoft.Library.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using PostSharp.Aspects;
    using PostSharp.Aspects.Advices;
    using PostSharp.Aspects.Dependencies;
    using PostSharp.Extensibility;
    using PostSharp.Reflection;

    /// <summary>
    /// An aspect indicating that the class can serve as a key in a cache request. Use the <see creft="CacheKeyAttribute" /> to indicate fields and properties that should be keys.
    /// </summary>
    [Serializable]
    [IntroduceInterface(typeof(ICacheKey), OverrideAction = InterfaceOverrideAction.Ignore)]
    [ProvideAspectRole(StandardRoles.Caching)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Tracing)]
    [MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Strict)]
    public class CachableInputAspect : InstanceLevelAspect, ICacheKey
    {
        /// <summary>
        /// The defining class type
        /// </summary>
        private Type classType;
        
        /// <summary>
        /// Gets or sets the property key list.
        /// </summary>
        /// <value>
        /// The cache key list.
        /// </value>
        private List<PropertyInfo> PropertyKeyList { get; set; }

        /// <summary>
        /// Gets or sets the field key list.
        /// </summary>
        /// <value>
        /// The field key list.
        /// </value>
        private List<FieldInfo> FieldKeyList { get; set; }

        /// <summary>
        /// Method invoked at build time to initialize the instance fields of the current aspect. This method is invoked
        /// before any other build-time method.
        /// </summary>
        /// <param name="type">Type to which the current aspect is applied</param>
        /// <param name="aspectInfo">Reserved for future usage.</param>
        public override void CompileTimeInitialize(Type type, AspectInfo aspectInfo)
        {
            // Define the type at compile time so we don't need to do it runtime.
            this.classType = type;

            this.PropertyKeyList = this.classType.GetProperties().Where(prop => prop.IsDefined(typeof(CacheKeyAttribute), false)).ToList();
            this.FieldKeyList = this.classType.GetFields().Where(field => field.IsDefined(typeof(CacheKeyAttribute), false)).ToList();
        }

        /// <summary>
        /// Method introduced in the target type (unless it is already present);
        /// returns the list of cache keys
        /// </summary>
        /// <returns>A cache built from properties and fields marked as keys</returns>
        [IntroduceMember(Visibility = Visibility.Public, IsVirtual = true, OverrideAction = MemberOverrideAction.Ignore)]
        public string GetCacheKey()
        {
            StringBuilder cacheKey = new StringBuilder(string.Empty);

            if (this.PropertyKeyList != null)
            {
                foreach (PropertyInfo info in this.PropertyKeyList)
                {
                    cacheKey.Append(info.GetValue(this.Instance, null));
                }
            }

            if (this.FieldKeyList != null)
            {
                foreach (FieldInfo info in this.FieldKeyList)
                {
                    cacheKey.Append(info.GetValue(this.Instance));
                }
            }

            return cacheKey.ToString();
        }
    }
}