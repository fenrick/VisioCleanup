// -----------------------------------------------------------------------
// <copyright file="Corners.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Contracts
{
    using System;

    /// <summary>
    ///     Corners of a visio shape.
    /// </summary>
    public struct Corners
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
    }
}