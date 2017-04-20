// <copyright file="CursorAspect.cs">
// </copyright>
// <author></author>
// <email></email>
// <date></date>
// <summary></summary>
namespace Aspects.Forms
{
    using Aspects.Caching;
    using PostSharp.Aspects;
    using PostSharp.Aspects.Dependencies;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Windows.Forms;

    /// <summary>
    /// An aspect that changes the cursor of a form or control to Cursors.WaitCursor while a method is executing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The typical use case for this aspect is applying it to a form method that starts a potentially long-running operation.
    /// The aspect will automatically set the cursor of the containing form or control to be Cursors.WaitCursor when the
    /// method starts, and change it back to Cursors.Default when the method finishes, whether it succeeds or throws an exception.
    /// </para>
    /// <para>
    /// If necessary, the wait cursor and default cursor used can be overridden by changing the WaitCursor and DefaultCursor
    /// properties on the aspect.
    /// </para>
    /// <para>
    /// This aspect can only be applied to methods whose declaring types inherit from System.Windows.Forms.Control. Note that
    /// System.Windows.Forms.Form (the most typical containing type) does inherit from System.Windows.Forms.Control.
    /// </para>
    /// </remarks>
    [Serializable]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Tracing)]
    [AttributeUsage(AttributeTargets.Method)]
    [LinesOfCodeAvoided(3)]
    public sealed class CursorAspect : OnMethodBoundaryAspect
    {
        /// <summary>
        /// A compile logger with which to write compile time validation messages.
        /// </summary>
        private readonly ICompileLogger compileLogger;

        /// <summary>
        /// The wait cursor to use, i.e. the one to use while the method is executing. Default is Cursors.WaitCursor.
        /// </summary>
        private Cursor waitCursor;

        /// <summary>
        /// The default cursor to use, i.e. the one to reset to when the method has finished. Default is Cursors.Default.
        /// </summary>
        private Cursor defaultCursor;

        /// <summary>
        /// An aspect that changes the cursor of a form or control to Cursors.WaitCursor while a method is executing.
        /// </summary>
        /// <param name="compileLogger">A compile logger with which to write compile time validation messages.</param>
        [SuppressMessage(
            "Microsoft.StyleCop.CSharp.DocumentationRules",
            "SA1642:ConstructorSummaryDocumentationMustBeginWithStandardText",
            Justification = "The constructor is an aspect constructor, so the documentation text should describe the purpose of the aspect.")]
        public CursorAspect(ICompileLogger compileLogger)
        {
            this.compileLogger = compileLogger;
            this.DefaultCursorType = CursorType.Default;
            this.WaitCursorType = CursorType.Busy;
        }

        /// <summary>
        /// An aspect that changes the cursor of a form or control to Cursors.WaitCursor while a method is executing.
        /// </summary>
        [SuppressMessage(
            "Microsoft.StyleCop.CSharp.DocumentationRules",
            "SA1642:ConstructorSummaryDocumentationMustBeginWithStandardText",
            Justification = "The constructor is an aspect constructor, so the documentation text should describe the purpose of the aspect.")]
        public CursorAspect()
            : this(new PostsharpMessageLogger())
        {
        }

        /// <summary>
        /// Gets or sets the default cursor type to use, i.e. the one to reset to when the method has finished. Default is CursorType.Default.
        /// </summary>
        public CursorType DefaultCursorType { get; set; }

        /// <summary>
        /// Gets or sets the wait cursor to use, i.e. the one to use while the method is executing. Default is CursorType.Busy.
        /// </summary>
        public CursorType WaitCursorType { get; set; }

        /// <summary>
        /// Method invoked at build time to initialize the instance fields of the current aspect. This method is invoked
        /// before any other build-time method.
        /// </summary>
        /// <param name="method">Method to which the current aspect is applied</param>
        /// <param name="aspectInfo">Reserved for future usage.</param>
        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            this.defaultCursor = GetCursorByType(this.DefaultCursorType);
            this.waitCursor = GetCursorByType(this.WaitCursorType);
            base.CompileTimeInitialize(method, aspectInfo);
        }

        /// <summary>
        /// Method invoked at build time to ensure that the aspect has been applied to the right target.
        /// </summary>
        /// <param name="method">Method to which the aspect has been applied</param>
        /// <returns>
        /// <c>true</c> if the aspect was applied to an acceptable field, otherwise
        /// <c>false</c>.
        /// </returns>
        public override bool CompileTimeValidate(MethodBase method)
        {
            if (!typeof(Control).IsAssignableFrom(method.DeclaringType))
            {
                this.compileLogger.WriteError(
                    method,
                    "WaitCursorAspectTargetTypeMustBeControl",
                    "WaitCursorAspect: The target type {0} does not inherit from System.Windows.Forms.Control",
                    method.DeclaringType);
                return false;
            }
            else if (method.IsStatic)
            {
                this.compileLogger.WriteError(
                    method,
                    "WaitCursorAspectTargetMethodMustBeInstanceLevel",
                    "WaitCursorAspect: The target method {0} must not be static",
                    method);
                return false;
            }

            return base.CompileTimeValidate(method);
        }

        /// <summary>
        /// Method executed <b>before</b> the body of methods to which this aspect is applied.
        /// </summary>
        /// <param name="args">
        /// Event arguments specifying which method is being executed, which are its arguments, and how should the execution continue
        /// after the execution of <see cref="M:PostSharp.Aspects.IOnMethodBoundaryAspect.OnEntry(PostSharp.Aspects.MethodExecutionArgs)" />.
        /// </param>
        public override void OnEntry(MethodExecutionArgs args)
        {
            ((Control)args.Instance).Cursor = this.waitCursor;
            base.OnEntry(args);
        }

        /// <summary>
        /// Method executed <b>after</b> the body of methods to which this aspect is applied,
        /// even when the method exists with an exception (this method is invoked from
        /// the <c>finally</c> block).
        /// </summary>
        /// <param name="args">Event arguments specifying which method is being executed and which are its arguments.</param>
        public override void OnExit(MethodExecutionArgs args)
        {
            ((Control)args.Instance).Cursor = this.defaultCursor;
            base.OnExit(args);
        }

        /// <summary>
        /// Gets the Cursor associated with the given cursor type.
        /// </summary>
        /// <param name="cursorType">The cursor type for which to get a cursor object.</param>
        /// <returns>The corresponding cursor object.</returns>
        private static Cursor GetCursorByType(CursorType cursorType)
        {
            switch (cursorType)
            {
                case CursorType.Default:
                    return Cursors.Default;
                case CursorType.Busy:
                    return Cursors.WaitCursor;
                case CursorType.PartiallyBusy:
                    return Cursors.AppStarting;
                default:
                    throw new ArgumentException("Unknown cursor type", "cursorType");
            }
        }
    }
}