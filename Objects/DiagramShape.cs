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
    using System.Linq;

    /// <summary>
    ///     Internal representation of a visio shape, including details of any child shapes.
    /// </summary>
    internal class DiagramShape : IEquatable<DiagramShape?>
    {
        private DiagramShape? shapeAbove;

        private DiagramShape? shapeBelow;

        private DiagramShape? shapeToLeft;

        private DiagramShape? shapeToRight;

        /// <summary>
        ///     Initialises a new instance of the <see cref="DiagramShape" /> class.
        /// </summary>
        /// <param name="visioId">Visio shape ID.</param>
        public DiagramShape(int visioId)
        {
            this.VisioId = visioId;
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
        ///     Gets or sets the shape text.
        /// </summary>
        public string? ShapeText { get; set; }

        /// <summary>
        ///     Gets or sets the shape type.
        /// </summary>
        public ShapeType ShapeType { get; set; }

        /// <summary>
        ///     Gets visio shape ID.
        /// </summary>
        public int VisioId { get; }

        /// <summary>
        ///     Gets or sets the shape above.
        /// </summary>
        internal DiagramShape? ShapeAbove
        {
            get => this.shapeAbove;
            set
            {
                this.shapeAbove = value;
                if (value == null)
                {
                    return;
                }

                if (value.ShapeBelow == null)
                {
                    value.ShapeBelow = this;
                }
                else if (!value.ShapeBelow.Equals(this))
                {
                    value.ShapeBelow = this;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the shape below.
        /// </summary>
        internal DiagramShape? ShapeBelow
        {
            get => this.shapeBelow;
            set
            {
                this.shapeBelow = value;
                if (value == null)
                {
                    return;
                }

                if (value.ShapeAbove == null)
                {
                    value.ShapeAbove = this;
                }
                else if (!value.ShapeAbove.Equals(this))
                {
                    value.ShapeAbove = this;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the shape to the left.
        /// </summary>
        internal DiagramShape? ShapeToLeft
        {
            get => this.shapeToLeft;
            set
            {
                this.shapeToLeft = value;
                if (value == null)
                {
                    return;
                }

                if (value.ShapeToRight == null)
                {
                    value.ShapeToRight = this;
                }
                else if (!value.ShapeToRight.Equals(this))
                {
                    value.ShapeToRight = this;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the shape to the right.
        /// </summary>
        internal DiagramShape? ShapeToRight
        {
            get => this.shapeToRight;
            set
            {
                this.shapeToRight = value;
                if (value == null)
                {
                    return;
                }

                if (value.ShapeToLeft == null)
                {
                    value.ShapeToLeft = this;
                }
                else if (!value.ShapeToLeft.Equals(this))
                {
                    value.ShapeToLeft = this;
                }
            }
        }

        /// <summary>
        ///     Add child shape to parent.
        /// </summary>
        /// <param name="childShape">New child shape of this shape.</param>
        public void AddChildShape(DiagramShape childShape)
        {
            // add to array
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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.VisioId}: {this.ShapeText}";
        }

        /// <summary>
        ///  Map all neighbour shapes within tolerance of 10.
        /// </summary>
        internal void FindNeighbours()
        {
            // reset all shapes.
            var children = this.Children;
            foreach (var shape in children)
            {
                shape.ShapeAbove = null;
                shape.ShapeBelow = null;
                shape.ShapeToLeft = null;
                shape.ShapeToRight = null;
            }

            var lines = children.OrderBy(shape => shape.Corners.LeftSide).Select(shape => shape.Corners.LeftSide);
            foreach (var line in lines.Distinct())
            {
                var bottomOrdered = children.Where(shape => shape.Corners.LeftSide.Equals(line))
                    .OrderBy(shape => shape.Corners.BottomSide);
                DiagramShape? currentShape = null;

                foreach (var shape in bottomOrdered)
                {
                    switch (currentShape)
                    {
                        case not null when currentShape.Corners.BottomSide.Equals(shape.Corners.BottomSide):
                            // overlap!
                            throw new NotImplementedException("No idea what to do yet with this!");
                        case not null when !(currentShape.Corners.BottomSide < shape.Corners.BottomSide):
                            continue;
                        case not null:
                            shape.ShapeBelow = currentShape;

                            currentShape = shape;
                            break;
                        default:
                            currentShape = shape;
                            break;
                    }
                }
            }

            lines = children.OrderBy(shape => shape.Corners.TopSide).Select(shape => shape.Corners.TopSide);
            foreach (var line in lines.Distinct())
            {
                var bottomOrdered = children.Where(shape => shape.Corners.TopSide.Equals(line))
                    .OrderBy(shape => shape.Corners.LeftSide);
                DiagramShape? currentShape = null;

                foreach (var shape in bottomOrdered)
                {
                    switch (currentShape)
                    {
                        case not null when currentShape.Corners.LeftSide.Equals(shape.Corners.LeftSide):
                            // overlap!
                            throw new NotImplementedException("No idea what to do yet with this!");
                        case not null when !(currentShape.Corners.LeftSide < shape.Corners.LeftSide):
                            continue;
                        case not null when shape.Corners.LeftSide - currentShape.Corners.RightSide < 10:
                            shape.ShapeToLeft = currentShape;

                            currentShape = shape;
                            break;
                        default:
                            currentShape = shape;
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Does this shape have children.
        /// </summary>
        /// <returns>true if at least one.</returns>
        internal bool HasChildren()
        {
            return this.Children.Count > 0;
        }

        /// <summary>
        ///     Move shape down.
        /// </summary>
        /// <param name="movement">Amount to move shape.</param>
        internal void MoveDown(double movement)
        {
            var currentCorners = this.Corners;
            currentCorners.TopSide -= movement;
            currentCorners.BottomSide -= movement;
            this.Corners = currentCorners;

            foreach (var child in this.Children)
            {
                child.MoveDown(movement);
            }
        }

        /// <summary>
        ///     Move shape left.
        /// </summary>
        /// <param name="movement">Amount to move shape.</param>
        internal void MoveLeft(double movement)
        {
            var currentCorners = this.Corners;
            currentCorners.LeftSide -= movement;
            currentCorners.RightSide -= movement;
            this.Corners = currentCorners;

            foreach (var child in this.Children)
            {
                child.MoveLeft(movement);
            }
        }

        /// <summary>
        ///     Move shape right.
        /// </summary>
        /// <param name="movement">Amount to move shape.</param>
        internal void MoveRight(double movement)
        {
            var currentCorners = this.Corners;
            currentCorners.LeftSide += movement;
            currentCorners.RightSide += movement;
            this.Corners = currentCorners;

            foreach (var child in this.Children)
            {
                child.MoveRight(movement);
            }
        }

        /// <summary>
        ///     Move shape up.
        /// </summary>
        /// <param name="movement">Amount to move shape.</param>
        internal void MoveUp(double movement)
        {
            var currentCorners = this.Corners;
            currentCorners.TopSide += movement;
            currentCorners.BottomSide += movement;
            this.Corners = currentCorners;

            foreach (var child in this.Children)
            {
                child.MoveUp(movement);
            }
        }
    }
}