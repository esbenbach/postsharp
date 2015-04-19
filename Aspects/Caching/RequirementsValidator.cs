// <copyright file="RequirementsValidator.cs" company="Infosoft AS">
// Copyright (c) 2012 All Rights Reserved
// </copyright>
// <author>esbbach</author>
// <email></email>
// <date>10/24/2012 7:14:28 AM</date>
// <summary></summary>
namespace Infosoft.Library.Caching
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using PostSharp.Extensibility;

    /// <summary>
    /// Class for validating cachable requirements of a method
    /// </summary>
    public class RequirementsValidator : IRequirementsValidator
    {
        /// <summary>
        /// A list of return types that won't be allowed caching for
        /// </summary>
        private static readonly IList<Type> DisallowedTypes = new List<Type> { typeof(Stream), typeof(IEnumerator), typeof(IQueryable) };

        /// <summary>
        /// Logging instance for writing error messages at compile time
        /// </summary>
        private ICompileLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsValidator" /> class.
        /// </summary>
        /// <param name="compileLogger">The compile logger.</param>
        public RequirementsValidator(ICompileLogger compileLogger)
        {
            this.logger = compileLogger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsValidator" /> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public RequirementsValidator()
        {
            this.logger = new PostsharpMessageLogger();
        }

        /// <summary>
        /// Determines whether the specified method is a valid cache method.
        /// If a method is a constructor or returns a type that cannot be cached it will be false.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>
        ///   <c>true</c> if the specified method is a valid cache method; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidCacheMethod(MethodBase method)
        {
            if (method is ConstructorInfo)
            {
                this.logger.WriteError(method, "CX0001", "Cannot cache constructors. ({0}.{1})", method.DeclaringType.FullName, method.Name);
                return false;
            }

            var methodInfo = method as MethodInfo;
            if (methodInfo != null)
            {
                var returnType = methodInfo.ReturnType;
                if (IsDisallowedCacheReturnType(returnType))
                {
                    this.logger.WriteError(method, "CX0002", "Methods with return type {0} are not allowed to be cached in ({1}.{2})", returnType.Name, method.DeclaringType.FullName, method.Name);
                    return false;
                }

                if (string.Equals(returnType.Name, "Void", StringComparison.OrdinalIgnoreCase))
                {
                    this.logger.WriteError(method, "CX0003", "Cannot cache void methods ({0}.{1})", method.DeclaringType.FullName, method.Name);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified parameters are valid for use as cache keys.
        /// </summary>
        /// <param name="method">The method the validation operates for (used to indicate location to Postsharp)</param>
        /// <param name="parametersUsed">An array of parameter information, where each represents a parameter that should be used as a cache lookup key.</param>
        /// <returns>
        ///   <c>true</c> if all parameters are valid as cache keys; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidCacheKeyparameters(MethodBase method, ParameterInfo[] parametersUsed)
        {
            // Cache key validation
            foreach (ParameterInfo info in parametersUsed)
            {
                Type paramType = info.ParameterType;
                if (paramType != null)
                {
                    if (!paramType.IsValueType && paramType != typeof(string) && !typeof(ICacheKey).IsAssignableFrom(paramType))
                    {
                        this.logger.WriteError(method, "CX0201", "Reference types must implement ICacheKey to be used as key parameters for the cache ({0}.{1})", method.DeclaringType.FullName, method.Name);
                        return false;
                    }

                    if (!paramType.IsSerializable && !typeof(ICacheKey).IsAssignableFrom(paramType))
                    {
                        this.logger.WriteError(method, "CX0202", "Can only use serializable value types and types implementing ICacheKey as key parameters for the cache ({0}.{1})", method.DeclaringType.FullName, method.Name);
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the parameters of the specified method adheres to the requirements for caching. For instance out parameters cannot be cached
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>
        ///   <c>true</c> if [is valid cache parameters] [the specified method]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidCacheParameters(MethodBase method)
        {
            var parameters = method.GetParameters();
            foreach (ParameterInfo info in parameters)
            {
                if (info.IsOut)
                {
                    this.logger.WriteError(method, "CX0101", "Cannot cache methods with 'out' parameter values ({0}.{1})", method.DeclaringType.FullName, method.Name);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the given expiration times are valid.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="absoluteExpiration">The absolute expiration.</param>
        /// <param name="slidingExpiration">The sliding expiration.</param>
        /// <returns>true if valid expiration parameters otherwise false</returns>
        public bool IsValidExpirationTimes(MethodBase method, int absoluteExpiration, int slidingExpiration)
        {
            if(absoluteExpiration < 0 || slidingExpiration < 0)
            {
                this.logger.WriteError(method, "CX0301", "Cannot have negative cache expiration values ({0}.{1})", method.DeclaringType.FullName, method.Name);
                return false;
            }

            if(absoluteExpiration != 0 && slidingExpiration != 0)
            {
                this.logger.WriteError(method, "CX0302", "Only one of the exipiratio options (Aboslute/Sliding) may be set. ({0}.{1})", method.DeclaringType.FullName, method.Name);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified return type is one of the types in the list of disallowed cache return types.
        /// </summary>
        /// <param name="returnType">Type of the return.</param>
        /// <returns>
        ///   <c>true</c> if [is disallowed cache return type] [the specified return type]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsDisallowedCacheReturnType(Type returnType)
        {
            return DisallowedTypes.Any(t => t == returnType);
        }
    }
}