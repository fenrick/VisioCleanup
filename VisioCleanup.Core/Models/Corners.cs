// -----------------------------------------------------------------------
// <copyright file="Corners.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Models
{
    using System;

    /// <summary>
    ///     Corners of a visio shape.
    /// </summary>
    public struct Corners : IEquatable<Corners>
    {
        /// <summary>
        ///     Gets or sets left side of the shape.
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        ///     Gets or sets right side of the shape.
        /// </summary>
        public int Right { get; set; }

        /// <summary>
        ///     Gets or sets base of the shape.
        /// </summary>
        public int Base { get; set; }

        /// <summary>
        ///     Gets or sets top of the shape.
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// Operator for equals.
        /// </summary>
        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        /// <returns>Are they equals.</returns>
        public static bool operator ==(Corners left, Corners right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Operator for not equals.
        /// </summary>
        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        /// <returns>Are they not equals.</returns>
        public static bool operator !=(Corners left, Corners right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Convert a visio measurement into an easier mathematical model.
        /// </summary>
        /// <param name="measurement">Measurement from visio.</param>
        /// <returns>Easier internal measurement.</returns>
        public static int ConvertMeasurement(double measurement)
        {
            return (int)(Math.Round(measurement, 3, MidpointRounding.AwayFromZero) * 1000);
        }

        /// <summary>
        /// Convert an easier measurement back to visio model.
        /// </summary>
        /// <param name="measurement">Easier internal measurement.</param>
        /// <returns>Measurement for visio.</returns>
        public static double ConvertMeasurement(int measurement)
        {
            return (double)measurement / 1000;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Top: {ConvertMeasurement(this.Top)}, Right: {ConvertMeasurement(this.Right)}, Base: {ConvertMeasurement(this.Base)}, Left: {ConvertMeasurement(this.Left)}";
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Corners corners && this.Equals(corners);
        }

        /// <inheritdoc />
        public bool Equals(Corners other)
        {
            return this.Left.Equals(other.Left) && this.Right.Equals(other.Right) && this.Base.Equals(other.Base) && this.Top.Equals(other.Top);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return HashCode.Combine(this.Left, this.Right, this.Base, this.Top);
        }
    }
}