// <copyright file="CacheKeyAttribute.cs" company="Infosoft AS">
// Copyright (c) 2012 All Rights Reserved
// </copyright>
// <author>esbbach</author>
// <email></email>
// <date>10/15/2012 6:40:10 PM</date>
// <summary></summary>
namespace Infosoft.Library.Caching
{
    using System;

    /// <summary>
    /// Indicates that a given field or property is part of the Cache key. Only used as a decorator with the <see cref="CachableInputAspect"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class CacheKeyAttribute : Attribute
    {
    }
}