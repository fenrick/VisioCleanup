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
        /// <param name="shapeText">Visio shape text.</param>
        /// <param name="corners">Corners of the shape.</param>
        /// <param name="shapeType">Shape type.</param>
        public DiagramShape(int visioId, string shapeText, Corners corners, ShapeType shapeType)
        {
            this.VisioId = visioId;
            this.ShapeText = shapeText;
            this.Corners = corners;
            this.ParentShape = null;
            this.ShapeType = shapeType;
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
        /// Gets or sets the shape type.
        /// </summary>
        public ShapeType ShapeType { get; set; }

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

        /// <summary>
        ///     Loop through child shapes and move them until no overlaps.
        /// </summary>
        /// <param name="verticalSpacer">Vertical space between shapes.</param>
        /// <param name="horizontalSpacer">Horizontal space between shapes.</param>
        /// <param name="ultimateChildWidth">Width of child shapes.</param>
        /// <param name="ultimateChildHeight">Height of child shapes.</param>
        public void AdjustDiagram(
            double verticalSpacer,
            double horizontalSpacer,
            double ultimateChildWidth,
            double ultimateChildHeight)
        {
            if (this.Children.Count == 0)
            {
                var newCorners = this.Corners;
                newCorners.BottomSide = newCorners.TopSide - ultimateChildHeight;
                newCorners.RightSide = newCorners.LeftSide + ultimateChildWidth;
                this.Corners = newCorners;
            }

            foreach (var shape in this.Children)
            {
                shape.AdjustDiagram(
                    verticalSpacer,
                    horizontalSpacer,
                    ultimateChildWidth,
                    ultimateChildHeight);

                // space above
                if (shape.ShapeAbove is not null)
                {
                    // compare top to bottom
                    var currentSpace = shape.ShapeAbove.Corners.BottomSide - shape.Corners.TopSide;
                    if (shape.Children.Count > 0)
                    {
                        if (!currentSpace.Equals(horizontalSpacer))
                        {
                            shape.ShapeAbove.MoveUp(horizontalSpacer - currentSpace);
                        }
                    }
                    else
                    {
                        if (!currentSpace.Equals(verticalSpacer))
                        {
                            shape.ShapeAbove.MoveUp(verticalSpacer - currentSpace);
                        }
                    }
                }

                // space below
                if (shape.ShapeBelow is not null)
                {
                    // compare top to bottom
                    var currentSpace = shape.Corners.BottomSide - shape.ShapeBelow.Corners.TopSide;
                    if (shape.Children.Count > 0)
                    {
                        if (!currentSpace.Equals(horizontalSpacer))
                        {
                            shape.ShapeBelow.MoveDown(verticalSpacer - currentSpace);
                        }
                    }
                    else
                    {
                        if (!currentSpace.Equals(verticalSpacer))
                        {
                            shape.ShapeBelow.MoveDown(verticalSpacer - currentSpace);
                        }
                    }
                }

                // space left
                if (shape.ShapeToLeft is not null)
                {
                    // compare top to bottom
                    var currentSpace = shape.Corners.LeftSide - shape.ShapeToLeft.Corners.RightSide;
                    if (!currentSpace.Equals(horizontalSpacer))
                    {
                        shape.ShapeToLeft.MoveLeft(horizontalSpacer - currentSpace);
                    }
                }

                // space right
                if (shape.ShapeToRight is not null)
                {
                    // compare top to bottom
                    var currentSpace = shape.ShapeToRight.Corners.LeftSide - shape.Corners.RightSide;
                    if (!currentSpace.Equals(horizontalSpacer))
                    {
                        shape.ShapeToRight.MoveRight(horizontalSpacer - currentSpace);
                    }
                }
            }

            var offsetChecker = false;

            offsetChecker = this.ShrinkToChildren(
                horizontalSpacer,
                horizontalSpacer,
                horizontalSpacer,
                10);

            if (this.Children.Count > 0)
            {
                // find far left side (assume non-ragged side).
                var leftSide = this.Children.OrderBy(shape => shape.Corners.LeftSide)
                    .Select(shape => shape.Corners.LeftSide).Min();
                var bottomOrdered = this.Children.Where(shape => shape.Corners.LeftSide.Equals(leftSide));

                foreach (var shape in bottomOrdered)
                {
                    var nextShape = shape.ShapeToRight;
                    while (nextShape != null)
                    {
                        var offset = shape.Corners.TopSide - nextShape.Corners.TopSide;
                        if (offset != 0)
                        {
                            nextShape.MoveUp(offset);
                            offsetChecker = true;
                        }

                        nextShape = nextShape.ShapeToRight;
                    }
                }
            }

            // if we moved the shapes at the end, let's just go over again to confirm all good.
            if (offsetChecker)
            {
                this.AdjustDiagram(
                    verticalSpacer,
                    horizontalSpacer,
                    ultimateChildWidth,
                    ultimateChildHeight);
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
        public bool ShrinkToChildren(double leftPadding, double rightPadding, double bottomPadding, double topPadding)
        {
            if (this.Children.Count <= 0)
            {
                return false;
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

            if (this.Corners.Equals(newCorners))
            {
                return false;
            }

            this.Corners = newCorners;
            return true;
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