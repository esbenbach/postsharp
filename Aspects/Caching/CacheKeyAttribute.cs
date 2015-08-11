
namespace Aspects.Caching
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