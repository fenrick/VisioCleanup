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

    using Serilog;

    /// <summary>
    ///     Internal representation of a visio shape, including details of any child shapes.
    /// </summary>
    internal class DiagramShape : IEquatable<DiagramShape?>
    {
        private readonly ILogger logger;

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
            this.logger = Log.ForContext<DiagramShape>();
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
        ///     Gets or sets a value indicating whether the height has been increased.
        /// </summary>
        public bool IncreasedHeight { get; set; }

        /// <summary>
        ///     Gets or sets the number of shapes per line.
        /// </summary>
        public int LineLength { get; set; }

        public DiagramShape? ParentShape { get; set; }

        /// <summary>
        ///     Gets the shape above.
        /// </summary>
        public DiagramShape? ShapeAbove
        {
            get => this.shapeAbove;
            private set
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
        ///     Gets the shape text.
        /// </summary>
        public string? ShapeText { get; init; }

        /// <summary>
        ///     Gets the shape to the left.
        /// </summary>
        public DiagramShape? ShapeToLeft
        {
            get => this.shapeToLeft;
            private set
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
        ///     Gets or sets the shape type.
        /// </summary>
        public ShapeType ShapeType { get; set; }

        /// <summary>
        ///     Gets or sets the visio shape ID.
        /// </summary>
        public int VisioId { get; set; }

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
        ///     Gets or sets the shape below.
        /// </summary>
        private DiagramShape? ShapeBelow
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
        ///     Add child shape to parent.
        /// </summary>
        /// <param name="childShape">New child shape of this shape.</param>
        public void AddChildShape(DiagramShape childShape)
        {
            // add to array
            this.Children.Add(childShape);
            childShape.ParentShape = this;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return this.Equals(obj as DiagramShape);
        }

        /// <inheritdoc />
        public bool Equals(DiagramShape? other)
        {
            return (other != null) && (this.VisioId == other.VisioId);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return HashCode.Combine(this.VisioId);
        }

        /// <summary>
        ///     Sort child shapes by layout.
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException">key is <see langword="null" />.</exception>
        public void SortChildrenByNeighbour()
        {
            var childCounter = 0;
            var sortedChildren = new SortedList<int, DiagramShape>(this.Children.Count);
            var children = this.Children;

            var shapes = children.Where(
                shape =>
                    {
                        var max = children.Max(innerShape => innerShape.Corners.TopSide);
                        return shape.Corners.TopSide.Equals(max);
                    }).OrderBy(shape => shape.Corners.LeftSide);
            var topChild = shapes.First();
            var childLineStart = topChild;
            DiagramShape currentChild = topChild;
            do
            {
                sortedChildren.Add(childCounter++, currentChild);
                while (currentChild.ShapeToRight is not null)
                {
                    currentChild = currentChild.ShapeToRight;
                    sortedChildren.Add(childCounter++, currentChild);
                }

                if (childLineStart.ShapeBelow is not null)
                {
                    childLineStart = childLineStart.ShapeBelow;
                    currentChild = childLineStart;
                }
                else
                {
                    childLineStart = null;
                }
            }
            while (childLineStart is not null);

            this.Children.Clear();
            this.Children.AddRange(sortedChildren.Values);
        }

        /// <summary>
        ///     Sort child shapes by size.
        /// </summary>
        public void SortChildrenBySize()
        {
            var shapes = this.Children.OrderByDescending(shape => shape.TotalChildrenCount());
            var sortedChildren = shapes.ToList();

            this.Children.Clear();
            this.Children.AddRange(sortedChildren);

            this.LineLength = (int)Math.Round(this.Children.Count / 2D, 0, MidpointRounding.ToPositiveInfinity);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.VisioId.ToString()}: {this.ShapeText}";
        }

        internal bool AlignToLeftAndAbove(double spacer)
        {
            // shape to left
            var leftShape = this.ShapeToLeft;
            var aboveShape = this.ShapeAbove;
            if (aboveShape is null || leftShape is null)
            {
                throw new InvalidOperationException("This shouldn't happen!");
            }

            var topSide = Math.Round(aboveShape.Corners.BottomSide - spacer, 3, MidpointRounding.AwayFromZero);
            var leftSide = Math.Round(leftShape.Corners.RightSide + spacer, 3, MidpointRounding.AwayFromZero);

            return this.MoveToPosition(topSide, leftSide);
        }

        internal bool AlignToLeftShape(double spacer)
        {
            // shape to left
            if (this.ShapeToLeft is null)
            {
                throw new InvalidOperationException("This shouldn't happen!");
            }

            var shapeToLeftCorners = this.ShapeToLeft.Corners;
            var topSide = Math.Round(shapeToLeftCorners.TopSide, 3, MidpointRounding.AwayFromZero);
            var leftSide = Math.Round(shapeToLeftCorners.RightSide + spacer, 3, MidpointRounding.AwayFromZero);

            return this.MoveToPosition(topSide, leftSide);
        }

        internal bool AlignToShapeAbove(double spacer)
        {
            // nothing to left
            if (this.ShapeAbove is null)
            {
                throw new InvalidOperationException("This shouldn't happen!");
            }

            var shapeAboveCorners = this.ShapeAbove.Corners;
            var topSide = Math.Round(shapeAboveCorners.BottomSide - spacer, 3, MidpointRounding.AwayFromZero);
            var leftSide = Math.Round(shapeAboveCorners.LeftSide, 3, MidpointRounding.AwayFromZero);

            return this.MoveToPosition(topSide, leftSide);
        }

        /// <summary>
        ///     Map all neighbour shapes within tolerance of 10.
        ///     TODO: Rationalise this method.
        /// </summary>
        /// <exception cref="NotImplementedException">No idea what to do yet with this.</exception>
        internal void FindNeighbours()
        {
            if (!this.HasChildren())
            {
                return;
            }

            // reset all shapes.
            var children = this.Children;
            this.ClearNeighbours();

            const double Tolerance = 5;

            var lines = children.OrderBy(shape => shape.Corners.LeftSide).Select(shape => shape.Corners.LeftSide);
            foreach (var line in lines.Distinct())
            {
                bool AbsoluteShapeSize(DiagramShape shape)
                {
                    var side = shape.Corners.LeftSide - line;
                    return Math.Abs(side) < Tolerance;
                }

                var diagramShapes = children.Where(AbsoluteShapeSize);
                var bottomOrdered = diagramShapes.OrderBy(shape => shape.Corners.BottomSide);
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
                        case not null when (shape.Corners.BottomSide - currentShape.Corners.TopSide) < (Tolerance * 2):
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
                bool AbsoluteShapeSize(DiagramShape shape)
                {
                    var side = shape.Corners.TopSide - line;
                    return Math.Abs(side) < Tolerance;
                }

                var diagramShapes = children.Where(AbsoluteShapeSize);
                var bottomOrdered = diagramShapes.OrderBy(shape => shape.Corners.LeftSide);
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
                        case not null when (shape.Corners.LeftSide - currentShape.Corners.RightSide) < (Tolerance * 2):
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

        internal bool ManualChangeHeight(double newHeight)
        {
            var myHeight = this.Corners.Height();
            if (!(myHeight < newHeight))
            {
                return false;
            }

            var corners = this.Corners;
            corners.BottomSide = Math.Round(corners.TopSide - newHeight, 3, MidpointRounding.AwayFromZero);

            if (this.Corners.Equals(corners))
            {
                return false;
            }

            this.logger.Debug("Manual Resizing: {Shape}", this);
            this.logger.Debug("New size for shape: {Corners}", corners.ToString());
            this.Corners = corners;
            this.IncreasedHeight = true;
            return true;
        }

        internal bool PlaceInCorner(double topPadding, double leftPadding)
        {
            if (this.ParentShape == null)
            {
                return false;
            }

            // nothing to left
            var parentShapeCorners = this.ParentShape.Corners;
            var topSide = Math.Round(parentShapeCorners.TopSide - topPadding, 3, MidpointRounding.AwayFromZero);
            var leftSide = Math.Round(parentShapeCorners.LeftSide + leftPadding, 3, MidpointRounding.AwayFromZero);

            return this.MoveToPosition(topSide, leftSide);
        }

        internal void RealignShapes()
        {
            if (!this.HasChildren())
            {
                return;
            }

            var sortedChildren = this.Children;
            var lineLength = this.LineLength;
            this.ClearNeighbours();

            for (var i = 0; i < sortedChildren.Count; i++)
            {
                var child = sortedChildren[i];

                // line number & position
                var lineNumber = (int)Math.Truncate(i / (double)lineLength);
                var linePosition = i % lineLength;

                // find below
                var nextLineNumber = lineNumber + 1;
                var positionBelow = (nextLineNumber * lineLength) + linePosition;
                if (sortedChildren.Count > positionBelow)
                {
                    var belowChild = sortedChildren[positionBelow];
                    child.ShapeBelow = belowChild;
                }

                // find to left
                var nextLinePosition = linePosition + 1;
                if (nextLinePosition > (lineLength - 1))
                {
                    continue;
                }

                var positionToLeft = (lineNumber * lineLength) + nextLinePosition;
                if (sortedChildren.Count <= positionToLeft)
                {
                    continue;
                }

                var leftChild = sortedChildren[positionToLeft];
                child.ShapeToRight = leftChild;
            }
        }

        internal bool ShapesTooLong(double maxRightSide, double ultimateShapeWidth, double internalPadding, Func<DiagramShape, double> spacer)
        {
            // do we know the line length?
            if (this.LineLength <= 0)
            {
                return false;
            }

            if (!this.HasChildren())
            {
                return false;
            }

            // check right side of shape
            if (!(this.Corners.RightSide >= maxRightSide))
            {
                return false;
            }

            var maxRightSideMinusOneShape = maxRightSide - ultimateShapeWidth;
            if (!(this.Corners.LeftSide < maxRightSideMinusOneShape))
            {
                return false;
            }

            if (this.LineLength <= 1)
            {
                return false;
            }

            this.logger.Debug("To far over: {Shape}", this);

            // fit shape inside page
            this.RealignShapes(maxRightSide, internalPadding, spacer);

            // reset children to default representation.
            foreach (var child in this.Children.Where(child => child.HasChildren()))
            {
                child.LineLength = (int)Math.Round(child.Children.Count / 2D, 0, MidpointRounding.ToPositiveInfinity);
                this.RealignShapes();
            }

            return true;
        }

        /// <summary>
        ///     Remove all records of shape neighbours.
        /// </summary>
        private void ClearNeighbours()
        {
            // reset all shapes.
            foreach (var shape in this.Children)
            {
                shape.ShapeAbove = null;
                shape.ShapeBelow = null;
                shape.ShapeToLeft = null;
                shape.ShapeToRight = null;
                shape.IncreasedHeight = false;
            }
        }

        /// <summary>
        ///     Move a shape for a spacer.
        /// </summary>
        /// <param name="movement">how far are we moving.</param>
        /// <param name="movementAction">Action for movement.</param>
        private void ExecuteMovement(double movement, Action<double> movementAction)
        {
            this.logger.Debug("{@Action} by {Movement}", movementAction.Method, movement);
            if (movement != 0)
            {
                movementAction(movement);
            }
        }

        /// <summary>
        ///     Move shape left.
        /// </summary>
        /// <param name="movement">Amount to move shape.</param>
        private void MoveLeft(double movement)
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

        private bool MoveToPosition(double topSide, double leftSide)
        {
            if (this.Corners.TopSide.Equals(topSide) && this.Corners.LeftSide.Equals(leftSide))
            {
                return false;
            }

            this.logger.Debug("Moving: {Shape}", this);

            // move left
            this.ExecuteMovement(this.Corners.LeftSide - leftSide, this.MoveLeft);

            // move up
            this.ExecuteMovement(topSide - this.Corners.TopSide, this.MoveUp);
            this.logger.Debug("New position: {Corners}", this.Corners.ToString());
            return true;
        }

        /// <summary>
        ///     Move shape up.
        /// </summary>
        /// <param name="movement">Amount to move shape.</param>
        private void MoveUp(double movement)
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

        // TODO: Needs refactoring
        private void RealignShapes(double maxLineSize, double internalPadding, Func<DiagramShape, double> spacer)
        {
            if (!this.HasChildren())
            {
                return;
            }

            // reset all shapes.
            this.ClearNeighbours();

            var lines = new List<List<DiagramShape>>();
            var padding = this.Corners.LeftSide + internalPadding;
            var lineWidth = maxLineSize - padding;
            double lineLength = 0;
            var currentLine = 0;
            var currentPosition = 0;
            lines.Insert(currentLine, new List<DiagramShape>());

            foreach (var child in this.Children)
            {
                var shapeWidth = child.Corners.Width() + spacer(child);

                if (child.Corners.Width() > lineWidth)
                {
                    lineWidth = shapeWidth + 1;
                }

                var nextLineLength = lineLength + shapeWidth;
                if (!(nextLineLength < lineWidth))
                {
                    currentPosition = 0;
                    currentLine++;
                    lines.Insert(currentLine, new List<DiagramShape>());
                    lineLength = 0;
                }

                lines[currentLine].Insert(currentPosition, child);
                lineLength += shapeWidth;
                var previousLine = currentLine - 1;
                if (currentLine >= 1)
                {
                    if (lines.Count >= previousLine)
                    {
                        var lineAbove = lines[previousLine];
                        if (lineAbove.Count > currentPosition)
                        {
                            child.ShapeAbove = lineAbove[currentPosition];
                        }
                        else
                        {
                            this.logger.Error("Strange things are afoot at the circle K.");
                        }
                    }
                }

                var previousPosition = currentPosition - 1;
                if (currentPosition >= 1)
                {
                    if (lines[currentLine].Count >= previousPosition)
                    {
                        child.ShapeToLeft = lines[currentLine][previousPosition];
                    }
                }

                currentPosition++;
            }

            this.LineLength = lines[0].Count;

            var numberOfLines = this.Children.Count / (double)this.LineLength;
            if (numberOfLines >= 2)
            {
                return;
            }

            this.LineLength = (int)Math.Round(this.Children.Count / 2D, 0, MidpointRounding.ToPositiveInfinity);
            this.RealignShapes();
        }

        /// <summary>
        ///     Calculate # of total children.
        /// </summary>
        /// <returns>count of children, iterating.</returns>
        private int TotalChildrenCount()
        {
            return !this.HasChildren() ? 1 : this.Children.Sum(child => child.TotalChildrenCount());
        }
    }
}