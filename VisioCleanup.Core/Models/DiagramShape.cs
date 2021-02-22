// -----------------------------------------------------------------------
// <copyright file="DiagramShape.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Models
{
    using VisioCleanup.Core.Models.Config;

    internal class DiagramShape
    {
        public static AppConfig AppConfig { get; set; }

        public Collection<DiagramShape> Children { get; set; }

        /// <summary>
        ///     Gets or sets the corner structure.
        /// </summary>
        public Corners Corners { get; set; }

        /// <summary>
        ///     Gets the shape text.
        /// </summary>
        public string? ShapeText { get; init; }

        /// <summary>
        ///     Gets or sets the shape type.
        /// </summary>
        public ShapeType ShapeType { get; set; }

        public int VisioId { get; set; }
    }
}