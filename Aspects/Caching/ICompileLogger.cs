
namespace Aspects.Caching
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
