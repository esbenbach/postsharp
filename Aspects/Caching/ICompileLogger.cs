// <copyright file="ICompileLogger.cs" company="Infosoft AS">
// Copyright (c) 2012 All Rights Reserved
// </copyright>
// <author>esbbach</author>
// <email></email>
// <date>10/24/2012 7:14:28 AM</date>
// <summary></summary>
namespace Infosoft.Library.Caching
{
    using System.Reflection;

    /// <summary>
    /// Compile time logging abstraction, useful for unit testing of aspects
    /// </summary>
    public interface ICompileLogger
    {
        /// <summary>
        /// Writes the error.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="args">The args.</param>
        void WriteError(MethodBase method, string errorCode, string errorMessage, params object[] args);
    }
}
