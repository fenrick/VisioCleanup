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
        ///     Gets or sets the shape above.
        /// </summary>
        public DiagramShape? ShapeAbove
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
        public DiagramShape? ShapeBelow
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
        ///     Gets the shape text.
        /// </summary>
        public string ShapeText { get; }

        /// <summary>
        ///     Gets or sets the shape to the left.
        /// </summary>
        public DiagramShape? ShapeToLeft
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
        public DiagramShape? ShapeToRight
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
        ///     Gets visio shape ID.
        /// </summary>
        public int VisioId { get; }

        /// <summary>
        ///     Add child shape to parent.
        /// </summary>
        /// <param name="childShape">New child shape of this shape.</param>
        public void AddChildShape(DiagramShape childShape)
        {
            // set parent
            childShape.ParentShape = this;

            // add to array
            this.Children.Add(childShape);

            this.FindNeighbours();
        }
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

        /// <summary>
        ///     Move shape down.
        /// </summary>
        /// <param name="movement">Amount to move shape.</param>
        public void MoveDown(double movement)
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
        public void MoveLeft(double movement)
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
        public void MoveRight(double movement)
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
        public void MoveUp(double movement)
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

        /// <summary>
        ///     Shrink shape to size of internal shapes.
        /// </summary>
        /// <param name="leftPadding">Left padding.</param>
        /// <param name="rightPadding">Right padding.</param>
        /// <param name="bottomPadding">Bottom padding.</param>
        /// <param name="topPadding">Top padding.</param>
        public void ShrinkToChildren(double leftPadding, double rightPadding, double bottomPadding, double topPadding)
        {
            if (this.Children.Count <= 0)
            {
                return;
            }

            var newCorners = this.Corners;

            // left side
            var leftSide = this.Children.OrderBy(shape => shape.Corners.LeftSide)
                .Select(shape => shape.Corners.LeftSide).Min();
            newCorners.LeftSide = leftSide - leftPadding;

            // right side
            var rightSide = this.Children.OrderBy(shape => shape.Corners.RightSide)
                .Select(shape => shape.Corners.RightSide).Max();
            newCorners.RightSide = rightSide + rightPadding;

            // bottom side
            var bottomSide = this.Children.OrderBy(shape => shape.Corners.BottomSide)
                .Select(shape => shape.Corners.BottomSide).Min();
            newCorners.BottomSide = bottomSide - bottomPadding;

            // top side
            var topSide = this.Children.OrderBy(shape => shape.Corners.TopSide).Select(shape => shape.Corners.TopSide)
                .Max();
            newCorners.TopSide = topSide + topPadding;

            this.Corners = newCorners;
        }

        private void FindNeighbours()
        {
            // reset all shapes.
            foreach (var shape in this.Children)
            {
                shape.ShapeAbove = null;
                shape.ShapeBelow = null;
                shape.ShapeToLeft = null;
                shape.ShapeToRight = null;
            }

            var lines = this.Children.OrderBy(shape => shape.Corners.LeftSide).Select(shape => shape.Corners.LeftSide);
            foreach (var line in lines.Distinct())
            {
                var bottomOrdered = this.Children.Where(shape => shape.Corners.LeftSide.Equals(line))
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

            lines = this.Children.OrderBy(shape => shape.Corners.BottomSide).Select(shape => shape.Corners.BottomSide);
            foreach (var line in lines.Distinct())
            {
                var bottomOrdered = this.Children.Where(shape => shape.Corners.BottomSide.Equals(line))
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
                        case not null:
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
    }
}