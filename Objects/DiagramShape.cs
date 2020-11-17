// -----------------------------------------------------------------------
// <copyright file="DiagramShape.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Objects
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Internal representation of a visio shape, including details of any child shapes.
    /// </summary>
    internal class DiagramShape : IEquatable<DiagramShape?>
    {
        /// <summary>
        ///     Initialises a new instance of the <see cref="DiagramShape" /> class.
        /// </summary>
        /// <param name="visioId">Visio shape ID.</param>
        /// <param name="corners">Corners of the shape.</param>
        public DiagramShape(int visioId, Corners corners)
        {
            this.VisioId = visioId;
            this.Corners = corners;
            this.ParentShape = null;
            this.Children = new List<DiagramShape>();
        }

        /// <summary>
        ///     Gets child shapes.
        /// </summary>
        public List<DiagramShape> Children { get; }

        /// <summary>
        ///     Gets or sets the corner structure.
        /// </summary>
        public Corners Corners { get; set; }

        /// <summary>
        ///     Gets the parent shape for this shape.
        /// </summary>
        public DiagramShape? ParentShape { get; private set; }

        /// <summary>
        ///     Gets visio shape ID.
        /// </summary>
        public int VisioId { get; }

        /// <summary>
        ///     Add child shape to parent.
        /// </summary>
        /// <param name="childShape">New child shape of this shape.</param>
        public void AddChildShape(DiagramShape childShape)
        {
            childShape.ParentShape = this;
            this.Children.Add(childShape);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return this.Equals(obj as DiagramShape);
        }

        /// <inheritdoc />
        public bool Equals(DiagramShape? other)
        {
            return other != null && this.VisioId == other.VisioId;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.VisioId);
        }
    }
}