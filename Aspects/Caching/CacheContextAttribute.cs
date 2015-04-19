// <copyright file="CacheContextAttribute.cs" company="Infosoft AS">
// Copyright (c) 2014 All Rights Reserved
// </copyright>
// <author>esbbach</author>
// <email></email>
// <date>12/10/2014 11:08:29 AM</date>
// <summary></summary>
namespace Infosoft.Library.Caching
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Indicates that by default all Caching should be done in the given CacheContext unless overridden explicitly. Only used as a decorator together with the <see cref="CacheAspect"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CacheContextAttribute : Attribute
    {
        /// <summary>
        /// Indicates that by default all Caching should be done in the given CacheContext unless overridden explicitly. Only used as a decorator together with the <see cref="CacheAspect"/>
        /// </summary>
        /// <param name="name">The cache context/instance that cache aspects in this class should use.</param>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1642:ConstructorSummaryDocumentationMustBeginWithStandardText", Justification = "The constructor is an attribute constructor and must describe the attributes function and not the constructors function.")]
        public CacheContextAttribute(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// The cache context/instance that cache aspects in this class should use.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The property is a named parameter in the attribute and must describe its function as such and not a property")]
        public string Name { get; set; }
    }
}