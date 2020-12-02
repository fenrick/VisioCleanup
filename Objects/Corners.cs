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
        private double leftSide;

        private double rightSide;

        private double bottomSide;

        private double topSide;

        /// <summary>
        ///     Gets or sets left side of the shape.
        /// </summary>
        public double LeftSide
        {
            get => this.leftSide;
            set => this.leftSide = Math.Round(value, 3, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        ///     Gets or sets right side of the shape.
        /// </summary>
        public double RightSide
        {
            get => this.rightSide;
            set => this.rightSide = Math.Round(value, 3, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        ///     Gets or sets bottom of the shape.
        /// </summary>
        public double BottomSide
        {
            get => this.bottomSide;
            set => this.bottomSide = Math.Round(value, 3, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        ///     Gets or sets top of the shape.
        /// </summary>
        public double TopSide
        {
            get => this.topSide;
            set => this.topSide = Math.Round(value, 3, MidpointRounding.AwayFromZero);
        }

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
            return $"Top: {this.TopSide}, Right: {this.RightSide}, Bottom: {this.BottomSide}, Left: {this.LeftSide}";
        }

        /// <summary>
        ///     Calculate width of shape.
        /// </summary>
        /// <returns>Double representing width.</returns>
        public double Width()
        {
            return Math.Round(this.RightSide - this.LeftSide, 3, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        ///     Calculate height of shape.
        /// </summary>
        /// <returns>Double representing height.</returns>
        public double Height()
        {
            return Math.Round(this.TopSide - this.BottomSide, 3, MidpointRounding.AwayFromZero);
        }
    }
}