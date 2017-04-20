// <copyright file="RequireUtcAttribute.cs" company="Infosoft AS">
// Copyright (c) 2017 All Rights Reserved
// </copyright>
// <author>esbbach</author>
// <email>esbbach@INFOSOFT</email>
// <date>4/20/2017 8:02:52 AM</date>
// <summary></summary>
namespace Aspects.Contracts
{
    using PostSharp.Aspects;
    using PostSharp.Patterns.Contracts;
    using PostSharp.Reflection;
    using System;

    /// <summary>
    /// A contract aspect attribute that when applied on a <see cref="System.DateTime"/> or <see cref="Nullable{DateTime}"/> will verify that the time zone of the date is UTC.
    /// Use this to enforce clients to consider time zones.
    /// </summary>
    /// <seealso cref="PostSharp.Patterns.Contracts.LocationContractAttribute" />
    /// <seealso cref="PostSharp.Aspects.ILocationValidationAspect{T}" />
    public sealed class RequireUtcAttribute : LocationContractAttribute, ILocationValidationAspect<DateTime>, ILocationValidationAspect<DateTime?>
    {
        /// <summary>
        /// Validates the value being assigned to the location to which the current aspect has been applied.
        /// </summary>
        /// <param name="value">Value being applied to the location.</param>
        /// <param name="locationName">Name of the location.</param>
        /// <param name="locationKind">Location kind (<see cref="F:PostSharp.Reflection.LocationKind.Field" />, <see cref="F:PostSharp.Reflection.LocationKind.Property" />, or
        /// <see cref="F:PostSharp.Reflection.LocationKind.Parameter" />).</param>
        /// <returns>
        /// An <see cref="T:System.ArgumentOutOfRangeException" /> to be thrown, or <c>null</c> if no exception needs to be thrown.
        /// </returns>
        public Exception ValidateValue(DateTime value, string locationName, LocationKind locationKind)
        {
            if (!IsUtcKind(value))
            {
                return this.CreateArgumentOutOfRangeException(value, locationName, locationKind);
            }

            return null;
        }

        /// <summary>
        /// Validates the value being assigned to the location to which the current aspect has been applied.
        /// </summary>
        /// <param name="value">Value being applied to the location.</param>
        /// <param name="locationName">Name of the location.</param>
        /// <param name="locationKind">Location kind (<see cref="F:PostSharp.Reflection.LocationKind.Field" />, <see cref="F:PostSharp.Reflection.LocationKind.Property" />, or
        /// <see cref="F:PostSharp.Reflection.LocationKind.Parameter" />).</param>
        /// <returns>
        /// The <see cref="T:System.Exception" /> to be thrown, or <c>null</c> if no exception needs to be thrown.
        /// </returns>
        public Exception ValidateValue(DateTime? value, string locationName, LocationKind locationKind)
        {
            if (value == null)
            {
                return null;
            }

            return this.ValidateValue(value.Value, locationName, locationKind);
        }

        /// <summary>
        /// Gets unformatted error message as defined by the instance.
        /// </summary>
        /// <returns>
        /// An error message string indicating that the DateTimeKind must be UTC.
        /// </returns>
        protected override string GetErrorMessage()
        {
            return $"Date element {{0}} must be of kind { nameof(DateTimeKind.Utc) }";
        }

        /// <summary>
        /// Determines whether the value has UTC as the specified DateTimeKind.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>True</c> if UTC is specified, <c>false</c> in all other cases</returns>
        private static bool IsUtcKind(DateTime value)
        {
            return value.Kind == DateTimeKind.Utc;
        }
    }
}