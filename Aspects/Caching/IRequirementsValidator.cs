
namespace Aspects.Caching
{
    using System;

    /// <summary>
    /// A validation of cache requirements
    /// </summary>
    public interface IRequirementsValidator
    {
        /// <summary>
        /// Determines whether the specified method has parameters that can be used for the cache key
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="parametersUsed">The parameters used.</param>
        /// <returns>
        ///   <c>true</c> if [is valid cache keyparameters] [the specified method]; otherwise, <c>false</c>.
        /// </returns>
        bool IsValidCacheKeyparameters(System.Reflection.MethodBase method, System.Reflection.ParameterInfo[] parametersUsed);

        /// <summary>
        /// Determines whether the specified method is a valid cache method (for instance, not a constructor)
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>
        ///   <c>true</c> if [is valid cache method] [the specified method]; otherwise, <c>false</c>.
        /// </returns>
        bool IsValidCacheMethod(System.Reflection.MethodBase method);

        /// <summary>
        /// Determines whether the method has parameters that are valid for caching (return values, and no "out" occurences for instance)
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>
        ///   <c>true</c> if [is valid cache parameters] [the specified method]; otherwise, <c>false</c>.
        /// </returns>
        bool IsValidCacheParameters(System.Reflection.MethodBase method);

        /// <summary>
        /// Determines whether the given expiration times are valid.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="absoluteExpiration">The absolute expiration.</param>
        /// <param name="slidingExpiration">The sliding expiration.</param>
        /// <returns>true if valid expiration parameters otherwise false</returns>
        bool IsValidExpirationTimes(System.Reflection.MethodBase method, int absoluteExpiration, int slidingExpiration);
    }
}
