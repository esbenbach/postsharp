// <copyright file="MethodLogAspect.cs">
// 
// </copyright>
// <author></author>
// <email></email>
// <date></date>
// <summary></summary>
namespace Aspects.Diagnostics
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Text;
    using log4net;
    using PostSharp.Aspects;
    using PostSharp.Aspects.Dependencies;
    using PostSharp.Extensibility;

    /// <summary>
    /// Aspect that, when applied on a method, emits a trace message before and
    /// after the method execution.
    /// </summary>
    /// <remarks>
    /// The aspect is currently dependent on log4net - this could be rather easily changed to a different logging engine.
    /// 
    /// Together with the <see cref="ToStringAspect"/> this aspect provides easy and useful logging for most existing methods.
    /// </remarks>
    [Serializable]
    [ProvideAspectRole(StandardRoles.Tracing)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Caching)]
    [MulticastAttributeUsage(MulticastTargets.Method, Inheritance = MulticastInheritance.Multicast)]
    [ExcludeFromCodeCoverage]
    public sealed class MethodLogAspect : OnMethodBoundaryAspect
    {
        /// <summary>
        /// Field for the local logger
        /// </summary>
        private ILog logger;

        /// <summary>
        /// Field defining the type of the class that the aspect is applied to. Used for determining which logger to instantiate.
        /// </summary>
        private Type loggerClassType;

        /// <summary>
        /// Field containing the name of the method where the a spect is applied. Used to avoid reflection of the method name runtime.
        /// </summary>
        private string methodName;

        /// <summary>
        /// Gets or sets a custom log message used when entering the method.
        /// </summary>
        /// <value>The custom log message.</value>
        public string CustomLogMessage
        {
            get;
            set;
        }

        /// <summary>
        /// Method executed at build time. Initializes the aspect instance. After the execution
        /// of <see cref="CompileTimeInitialize"/>, the aspect is serialized as a managed 
        /// resource inside the transformed assembly, and deserialized at runtime.
        /// </summary>
        /// <param name="method">Method to which the current aspect instance has been applied.</param>
        /// <param name="aspectInfo">Contains aspect information, not used currently</param>
        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            // Define the method name at compile time so we don't need reflection to do it runtime.
            this.methodName = method.DeclaringType.FullName + "." + method.Name;

            // Define the logger at compile time so we don't need to do it runtime.
            this.loggerClassType = method.DeclaringType;
        }

        /// <summary>
        /// Method invoked before the execution of the method to which the current
        /// aspect is applied.
        /// </summary>
        /// <param name="args">Execution arguments, not used</param>
        [ExcludeFromCodeCoverage]
        public override void OnEntry(MethodExecutionArgs args)
        {
            if (this.logger.IsInfoEnabled)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(string.Format("Entering method: {0}", this.methodName));

                for (int i = 0; i < args.Arguments.Count; i++)
                {
                    if (i > 0)
                    {
                        stringBuilder.Append(", ");
                    }

                    stringBuilder.Append(args.Arguments.GetArgument(i) ?? "null");
                }

                if (this.CustomLogMessage != null)
                {
                    stringBuilder.AppendLine(this.CustomLogMessage);
                }

                this.logger.Info(stringBuilder.ToString());
            }
        }

        /// <summary>
        /// Method invoked after failure of the method to which the current
        /// aspect is applied.
        /// </summary>
        /// <param name="args">Execution arguments, not used.</param>
        [ExcludeFromCodeCoverage]
        public override void OnException(MethodExecutionArgs args)
        {
            if (this.logger.IsErrorEnabled)
            {
                StringBuilder stringBuilder = new StringBuilder(1024);

                // Write the exit message.
                stringBuilder.Append(this.methodName);
                stringBuilder.Append('(');

                // Write the current instance object, unless the method
                // is static.
                object instance = args.Instance;
                if (instance != null)
                {
                    stringBuilder.Append("this=");
                    stringBuilder.Append(instance);
                    if (args.Arguments.Count > 0)
                    {
                        stringBuilder.Append("; ");
                    }
                }

                // Write the list of all arguments.
                for (int i = 0; i < args.Arguments.Count; i++)
                {
                    if (i > 0)
                    {
                        stringBuilder.Append(", ");
                    }

                    stringBuilder.Append(args.Arguments.GetArgument(i) ?? "null");
                }

                // Write the exception message.
                stringBuilder.AppendFormat("): Exception ");

                // Finally emit the error.
                this.logger.Error(stringBuilder.ToString(), args.Exception);
            }
        }

        /// <summary>
        /// Method invoked after successfull execution of the method to which the current
        /// aspect is applied.
        /// </summary>
        /// <param name="args">Execution arguments, not used.</param>
        [ExcludeFromCodeCoverage]
        public override void OnSuccess(MethodExecutionArgs args)
        {
            this.logger.Info(string.Format("Finished execution of {0}. Output: {1} ", this.methodName, args.ReturnValue));
        }

        /// <summary>
        /// Initializes the current aspect at runtime. Defines the logger of the instance for use by the aspect methods.
        /// </summary>
        /// <param name="method">Method to which the current aspect is applied.</param>
        public override void RuntimeInitialize(MethodBase method)
        {
            base.RuntimeInitialize(method);
            this.logger = LogManager.GetLogger(this.loggerClassType);
        }
    }
}
