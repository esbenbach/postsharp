// <copyright file="CacheKeyBuilder.cs" company="Infosoft AS">
// Copyright (c) 2012 All Rights Reserved
// </copyright>
// <author>esbbach</author>
// <email></email>
// <date>10/15/2012 12:58:00 PM</date>
// <summary></summary>
namespace Infosoft.Library.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using PostSharp.Aspects;

    /// <summary>
    /// Class for building a cache key from the method and arguments
    /// </summary>
    [Serializable]
    public class CacheKeyBuilder
    {
        /// <summary>
        /// Separator string used to separate each item in a key.
        /// </summary>
        private const string CacheSeparator = ".";

        /// <summary>
        /// A list of argument indicies from the method that should be used for the cache key. So for instance if the list contains 1, 2, 5 the first, second and fifth parameter will be used when building the key.
        /// </summary>
        private List<int> methodCacheKeyArgumentIndicies;

        /// <summary>
        /// format string for the parameters part of the key
        /// </summary>
        private string parameterFormatString;

        /// <summary>
        /// Cache key prefix
        /// </summary>
        private string keyPrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheKeyBuilder" /> class.
        /// </summary>
        /// <param name="method">The method to build the key for</param>
        /// <param name="keyPrefix">Optional key prefix</param>
        /// <param name="parameters">An optional csv list of parameter names used to build the key</param>
        /// <param name="behavior">The behavior of the key builder</param>
        public CacheKeyBuilder(MethodBase method, string keyPrefix = null, string parameters = null, CacheKeyBehavior behavior = CacheKeyBehavior.Default)
        {
            this.keyPrefix = GeneratePrefixKey(method, keyPrefix);
            this.Behavior = behavior;

            // Now build a format string that can be used runtime to generate the actual cache key.
            // The string will be build in a form of "ParameterName = {0}", where {0} should be replaced with an index from the list of included indices
            // If parameters are explicitly ignored we won't bother generating this string
            this.methodCacheKeyArgumentIndicies = new List<int>();
            if (!this.Behavior.HasFlag(CacheKeyBehavior.IgnoreParameters))
            {
                this.parameterFormatString = this.BuildMethodParameterFormatString(method, parameters);
            }
        }

        /// <summary>
        /// Gets or sets the behavior.
        /// </summary>
        /// <value>
        /// The behavior.
        /// </value>
        public CacheKeyBehavior Behavior { get; set; }

        /// <summary>
        /// Gets the parameters used when the cache key was built
        /// </summary>
        /// <value>
        /// The parameters used.
        /// </value>
        internal ParameterInfo[] ParametersUsed { get; private set; }

        /// <summary>
        /// Builds the cache key using the runtime arguments and the formatstring that was build during the instance construction (compile time)
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>A string usable as a cache key</returns>
        public string BuildCacheKey(object[] arguments)
        {
            ////TODO: Consider adding an option for excluding parameters whose value matches the "default" value of that value type, add a cache setting to avoid it!

            // If there are no argument indicies or the parameters has been explicitly ignored, just use the key prefix and return
            if (this.methodCacheKeyArgumentIndicies.Count == 0 || this.Behavior.HasFlag(CacheKeyBehavior.IgnoreParameters))
            {
                return this.keyPrefix;
            }

            // Build a list of values, and assume that any validation of the parameters have been done compile time.
            List<object> argumentValues = new List<object>();
            for (int i = 0; i < this.methodCacheKeyArgumentIndicies.Count; i++)
            {
                var currentArgument = arguments[this.methodCacheKeyArgumentIndicies[i]];
                if (currentArgument == null)
                {
                    // If the value is null add the empty string to avoid null reference exceptions. Easier than removing the parameter entirely (which is just as right)
                    argumentValues.Add(string.Empty);
                }
                else if (currentArgument is ICacheKey)
                {
                    argumentValues.Add((currentArgument as ICacheKey).GetCacheKey());
                }
                else
                {
                    argumentValues.Add(currentArgument.ToString());
                }
            }

            string formatString = string.Format("{0}{1}", this.keyPrefix, this.parameterFormatString);
            return string.Format(string.Format(formatString, argumentValues.ToArray()));
        }

        /// <summary>
        /// Generates the cache key prefix.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="key">The specified prefix.</param>
        /// <returns>base/prefix key</returns>
        private static string GeneratePrefixKey(MethodBase method, string key)
        {
            StringBuilder formatString = new StringBuilder();

            // If no key was given initialise default
            if (string.IsNullOrWhiteSpace(key))
            {
                formatString.AppendFormat("{0}{1}{2}", method.DeclaringType.FullName, CacheSeparator, method.Name);
            }
            else
            {
                formatString.Append(key);
            }

            return formatString.ToString();
        }

        /// <summary>
        /// Builds the method parameter format string that can be used runtime to generate the actual cache key.
        ///  The string will be build in a form of "ParameterName = {0}", where {0} should be replaced with an index from the list of included indices
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="parameters">Optional parameters to include in the string</param>
        /// <returns>A parameter format string</returns>
        private string BuildMethodParameterFormatString(MethodBase method, string parameters = null)
        {
            // Explicitly ignored!
            if (this.Behavior.HasFlag(CacheKeyBehavior.IgnoreParameters))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(parameters))
            {
                // Build a key using ALL parameters
                this.ParametersUsed = method.GetParameters();
                this.methodCacheKeyArgumentIndicies = this.GetArgumentIndexList(this.ParametersUsed);
                return this.BuildFormatString(this.ParametersUsed);
            }
            else
            {
                // If parameters are defined try to build a key from only the defined parameters
                this.ParametersUsed = this.GetInfoFromSpecifiedParameters(method, parameters);
                this.methodCacheKeyArgumentIndicies = this.GetArgumentIndexList(this.ParametersUsed);
                return this.BuildFormatString(this.ParametersUsed);
            }
        }

        /// <summary>
        /// Gets the argument index list.
        /// </summary>
        /// <param name="parameterInformation">The infoarray.</param>
        /// <returns>A list of zero based positions of the arguments relative to the method they came from</returns>
        private List<int> GetArgumentIndexList(ParameterInfo[] parameterInformation)
        {
            return parameterInformation.Select(item => item.Position).ToList();
        }

        /// <summary>
        /// Builds the format string from specified parameters.
        /// </summary>
        /// <param name="method">The method to get parameter info from</param>
        /// <param name="parameters">The specified parameters names in a csv form</param>
        /// <returns>An array of parameter information</returns>
        private ParameterInfo[] GetInfoFromSpecifiedParameters(MethodBase method, string parameters)
        {
            ParameterInfo[] actualParameters = method.GetParameters();

            // Assume parameters are added as a comma seperated list and find the actual parameters from the method.
            // Split incomming parameters, remove emptry entries and trim them so no trailing spaces exists
            var requestedParameters = parameters.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim());

            var query = from parameter in actualParameters where requestedParameters.Contains(parameter.Name, StringComparer.Ordinal) select parameter;
            return query.ToArray();
        }

        /// <summary>
        /// Builds a format string based on the given parameters information.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>A cache formatstring</returns>
        private string BuildFormatString(ParameterInfo[] parameters)
        {
            StringBuilder keyFormatString = new StringBuilder();

            for (int counter = 0; counter < parameters.Length; counter++)
            {
                keyFormatString.AppendFormat(" {0} = '{{{1}}}'", parameters[counter].Name, counter);
            }
            
            return keyFormatString.ToString();
        }
    }
}