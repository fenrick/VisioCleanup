// -----------------------------------------------------------------------
// <copyright file="ShapeType.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Models
{
    /// <summary>
    ///     Type of diagram shape.
    /// </summary>
    public enum ShapeType
    {
        /// <summary>
        ///     Existing diagram shape.
        /// </summary>
        Existing,

        /// <summary>
        ///     New shape needed to be created.
        /// </summary>
        NewShape,

        /// <summary>
        ///     Fake shape.
        /// </summary>
        FakeShape,
    }
}