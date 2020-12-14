// -----------------------------------------------------------------------
// <copyright file="Corners.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Objects
{
    using System;

    /// <summary>
    ///     Corners of a visio shape.
    /// </summary>
    internal struct Corners : IEquatable<Corners>
    {
        /// <summary>
        ///     Gets or sets left side of the shape.
        /// </summary>
        public int LeftSide { get; set; }

        /// <summary>
        ///     Gets or sets right side of the shape.
        /// </summary>
        public int RightSide { get; set; }

        /// <summary>
        ///     Gets or sets bottom of the shape.
        /// </summary>
        public int BottomSide { get; set; }

        /// <summary>
        ///     Gets or sets top of the shape.
        /// </summary>
        public int TopSide { get; set; }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Corners corners && this.Equals(corners);
        }

        /// <inheritdoc />
        public bool Equals(Corners other)
        {
            return this.LeftSide.Equals(other.LeftSide) && this.RightSide.Equals(other.RightSide) && this.BottomSide.Equals(other.BottomSide)
                   && this.TopSide.Equals(other.TopSide);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.LeftSide, this.RightSide, this.BottomSide, this.TopSide);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Top: {(double)this.TopSide / 1000}, Right: {(double)this.RightSide / 1000}, Bottom: {(double)this.BottomSide / 1000}, Left: {(double)this.LeftSide / 1000}";
        }

        /// <summary>
        ///     Calculate width of shape.
        /// </summary>
        /// <returns>Double representing width.</returns>
        public int Width()
        {
            return this.RightSide - this.LeftSide;
        }

        /// <summary>
        ///     Calculate height of shape.
        /// </summary>
        /// <returns>Double representing height.</returns>
        public int Height()
        {
            return this.TopSide - this.BottomSide;
        }
    }
}