// <copyright file="OverrideCultureAspect.cs" company="Infosoft AS">
// Copyright (c) 2017 All Rights Reserved
// </copyright>
// <author>esbbach</author>
// <email>esbbach@INFOSOFT</email>
// <date>4/20/2017 8:08:11 AM</date>
// <summary></summary>
namespace Aspects.Testing
{
    using PostSharp.Aspects;
    using PostSharp.Extensibility;
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;

    /// <summary>
    /// An aspect that overrides the current culture of the thread during the execution of the method.
    /// Remember to set either CultureName or CultureId.
    /// </summary>
    /// <remarks>
    /// Since this is a thread-wide operation, tests that do this should preferably not run in parallel with other tests
    /// that also rely on the current culture. But that's the case whether you use this aspect or do it manually.
    /// </remarks>
    [Serializable]
    [LinesOfCodeAvoided(0)]
    public sealed class OverrideCultureAspect : OnMethodBoundaryAspect
    {
        /// <summary>
        /// The culture information object to override with.
        /// </summary>
        private CultureInfo cultureInfo;

        /// <summary>
        /// The culture information object that was in use before we did the override.
        /// </summary>
        private CultureInfo previousCultureInfo;

        /// <summary>
        /// Gets or sets the culture name to override with.
        /// </summary>
        public string CultureName { get; set; }

        /// <summary>
        /// Gets or sets the culture ID to override with.
        /// </summary>
        public int CultureId { get; set; }

        /// <summary>
        /// Method invoked at build time to initialize the instance fields of the current aspect.
        /// This method is invoked before any other build-time method.
        /// </summary>
        /// <param name="method">Method to which the current aspect is applied</param>
        /// <param name="aspectInfo">Reserved for future usage.</param>
        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            this.cultureInfo = this.GetCultureOrFail();
            base.CompileTimeInitialize(method, aspectInfo);
        }

        /// <summary>
        /// Method executed <b>before</b> the body of methods to which this aspect is applied.
        /// </summary>
        /// <param name="args">
        /// Event arguments specifying which method is being executed, which are its arguments, and how should the execution continue
        /// after the execution of <see cref="M:PostSharp.Aspects.IOnMethodBoundaryAspect.OnEntry(PostSharp.Aspects.MethodExecutionArgs)" />.
        /// </param>
        public sealed override void OnEntry(MethodExecutionArgs args)
        {
            this.previousCultureInfo = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = this.cultureInfo;
        }

        /// <summary>
        /// Method executed <b>after</b> the body of methods to which this aspect is applied,
        /// even when the method exists with an exception (this method is invoked from
        /// the <c>finally</c> block).
        /// </summary>
        /// <param name="args">Event arguments specifying which method is being executed and which are its arguments.</param>
        public sealed override void OnExit(MethodExecutionArgs args)
        {
            Thread.CurrentThread.CurrentCulture = this.previousCultureInfo;
        }

        /// <summary>
        /// Gets the specified culture (using the CultureName/CultureId), or throw an exception. Called at compile time.
        /// </summary>
        /// <returns>The CultureInfo to override with.</returns>
        private CultureInfo GetCultureOrFail()
        {
            if (!string.IsNullOrEmpty(this.CultureName))
            {
                if (this.CultureName == "InvariantCulture")
                {
                    return CultureInfo.InvariantCulture;
                }

                try
                {
                    return CultureInfo.GetCultureInfo(this.CultureName);
                }
                catch (CultureNotFoundException ex)
                {
                    string errorMessage = string.Format(CultureInfo.InvariantCulture, "Culture with name {0} was not found on this machine", this.CultureName);
                    throw new InvalidAnnotationException(errorMessage, ex);
                }
            }
            else if (this.CultureId > 0)
            {
                try
                {
                    return new CultureInfo(this.CultureId);
                }
                catch (CultureNotFoundException ex)
                {
                    string errorMessage = string.Format(CultureInfo.InvariantCulture, "Culture with ID {0} was not found on this machine", this.CultureId);
                    throw new InvalidAnnotationException(errorMessage, ex);
                }
            }
            else
            {
                throw new InvalidAnnotationException("No culture name/ID specified");
            }
        }
    }
}