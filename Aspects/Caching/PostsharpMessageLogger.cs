﻿namespace Aspects.Caching
{
    using System.Reflection;
    using PostSharp.Extensibility;

    /// <summary>
    /// A post sharp adapter that log errors/messages compiletime, usefull for aspect development.
    /// </summary>
    public class PostsharpMessageLogger : ICompileLogger
    {
        /// <summary>
        /// Writes the error.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="args">The args.</param>
        public void WriteError(MethodBase method, string errorCode, string errorMessage, params object[] args)
        {
            Message.Write(method, SeverityType.Error, errorCode, errorMessage, args);
        }
    }
}
