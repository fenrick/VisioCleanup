// -----------------------------------------------------------------------
// <copyright file="Corners.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Objects
{
    /// <summary>
    ///     Corners of a visio shape.
    /// </summary>
    internal struct Corners
    {
        /// <summary>
        ///     Gets or sets left side of the shape.
        /// </summary>
        public double LeftSide { get; set; }

        /// <summary>
        ///     Gets or sets right side of the shape.
        /// </summary>
        public double RightSide { get; set; }

        /// <summary>
        ///     Gets or sets bottom of the shape.
        /// </summary>
        public double BottomSide { get; set; }

        /// <summary>
        ///     Gets or sets top of the shape.
        /// </summary>
        public double TopSide { get; set; }
    }
}