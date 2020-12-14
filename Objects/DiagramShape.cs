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
            this.Corners = new Corners
                               {
                                   TopSide = UltimateShapeHeight, LeftSide = 0, RightSide = UltimateShapeWidth, BottomSide = 0,
                               };
        }

        public static int BottomPadding { get; set; }

        /// <summary>
        ///     Gets or sets the horizontal spacer.
        /// </summary>
        public static int HorizontalSpacer { get; set; }

        public static int LeftPadding { get; set; }

        public static int RightPadding { get; set; }

        public static int TopPadding { get; set; }

        public static int UltimateShapeHeight { get; set; }

        public static int UltimateShapeWidth { get; set; }

        /// <summary>
        ///     Gets or sets the vertical spacer.
        /// </summary>
        public static int VerticalSpacer { get; set; }

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

        public string? SortValue { get; set; }

        public string? Stencil { get; set; }

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

                // Move surrounding shapes
                this.MoveSurroundingShapes();
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

                // Move surrounding shapes
                this.MoveSurroundingShapes();
            }
        }

        /// <summary>
        ///     Add child shape to parent.
        /// </summary>
        /// <param name="childShape">New child shape of this shape.</param>
        public void AddChildShape(DiagramShape childShape)
        {
            if (!this.Children.Contains(childShape))
            {
                this.Children.Add(childShape);
            }

            // add to array
            childShape.ParentShape = this;

            // foreach (var child in this.Children)
            // {
            // child.MoveSurroundingShapes();
            // }
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

            foreach (var child in this.Children.ToArray())
            {
                this.RemoveChildShape(child);
            }

            foreach (var child in sortedChildren.Values)
            {
                this.AddChildShape(child);
            }
        }

        /// <summary>
        ///     Sort child shapes by size.
        /// </summary>
        /// <param name="sortMethod"></param>
        public void SortChildrenBySize(Func<DiagramShape, object> sortMethod)
        {
            var shapes = this.Children.OrderBy(shape => sortMethod(shape)).ToList();

            foreach (var child in this.Children.ToArray())
            {
                this.RemoveChildShape(child);
            }

            foreach (var child in shapes)
            {
                this.AddChildShape(child);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.VisioId}: {this.ShapeText}";
        }

        internal static bool Align(DiagramShape diagramShape)
        {
            bool changed;
            var defaultSpacer = diagramShape.HasChildren() ? HorizontalSpacer : VerticalSpacer;
            DiagramShape? aboveShape;
            int? verticalSpacer;
            DiagramShape? leftShape;
            int? horizontalSpacer;
            switch (diagramShape.ShapeAbove)
            {
                case null:
                    switch (diagramShape.ShapeToLeft)
                    {
                        case null:
                            aboveShape = diagramShape.ParentShape;
                            verticalSpacer = (0 - diagramShape.ParentShape?.Corners.Height()) + TopPadding;
                            leftShape = diagramShape.ParentShape;
                            horizontalSpacer = (0 - diagramShape.ParentShape?.Corners.Width()) + LeftPadding;
                            break;
                        default:
                            aboveShape = diagramShape.ShapeToLeft;
                            verticalSpacer = 0 - diagramShape.ShapeToLeft.Corners.Height();
                            leftShape = diagramShape.ShapeToLeft;
                            horizontalSpacer = defaultSpacer;
                            break;
                    }

                    break;
                default:
                    switch (diagramShape.ShapeToLeft)
                    {
                        case null:
                            aboveShape = diagramShape.ShapeAbove;
                            verticalSpacer = defaultSpacer;
                            leftShape = diagramShape.ShapeAbove;
                            horizontalSpacer = 0 - diagramShape.ShapeAbove.Corners.Width();
                            break;
                        default:
                            aboveShape = diagramShape.ShapeAbove;
                            verticalSpacer = defaultSpacer;
                            leftShape = diagramShape.ShapeToLeft;
                            horizontalSpacer = defaultSpacer;
                            break;
                    }

                    break;
            }

            return diagramShape.AlignShape(aboveShape, verticalSpacer, leftShape, horizontalSpacer);
        }

        internal bool AlignShape(DiagramShape? aboveShape, int? verticalSpacer, DiagramShape? leftShape, int? horizontalSpacer)
        {
            // remove nulls
            if (aboveShape is null || leftShape is null)
            {
                return false;
                throw new InvalidOperationException("This shouldn't happen!");
            }

            if (verticalSpacer is null || horizontalSpacer is null)
            {
                return false;
                throw new InvalidOperationException("This shouldn't happen!");
            }

            // calculate placement
            var topSide = (int)(aboveShape.Corners.BottomSide - verticalSpacer);
            var leftSide = (int)(leftShape.Corners.RightSide + horizontalSpacer);

            // move shape
            var changed = this.MoveToPosition(topSide, leftSide);

            return changed;
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

        internal bool HasParent()
        {
            return this.ParentShape != null;
        }

        internal bool ManualChangeHeight(int newHeight)
        {
            var myHeight = this.Corners.Height();
            if (!(myHeight < newHeight))
            {
                return false;
            }

            var corners = this.Corners;
            corners.BottomSide = corners.TopSide - newHeight;

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

                // find to right
                var nextLinePosition = linePosition + 1;
                if (nextLinePosition > (lineLength - 1))
                {
                    continue;
                }

                var positionToRight = (lineNumber * lineLength) + nextLinePosition;
                if (sortedChildren.Count <= positionToRight)
                {
                    continue;
                }

                var rightChild = sortedChildren[positionToRight];
                child.ShapeToRight = rightChild;
            }

            foreach (var child in this.Children)
            {
                if (child.HasChildren())
                {
                    var defaultLineLength = (int)Math.Round(child.Children.Count / 2D, 0, MidpointRounding.ToPositiveInfinity);
                    if (child.LineLength < defaultLineLength)
                    {
                        child.LineLength = defaultLineLength;
                        child.RealignShapes();
                    }
                }
            }

            this.ResizeShape();
        }

        // TODO: Needs refactoring
        internal void RealignShapes(int maxWidth)
        {
            if (!this.HasChildren())
            {
                return;
            }
            
            this.ResizeShape();
            if (this.Corners.Width() <= maxWidth)
            {
                return;
            }

            // reset all shapes.
            this.ClearNeighbours();

            var lines = new List<List<DiagramShape>>();
            var padding = this.Corners.LeftSide + LeftPadding + RightPadding;
            var lineWidth = maxWidth - padding;
            var lineLength = 0;
            var currentLine = 0;
            var currentPosition = 0;
            lines.Insert(currentLine, new List<DiagramShape>());

            foreach (var child in this.Children)
            {
                child.ResizeShape();
                var shapeWidth = child.Corners.Width() + (child.HasChildren() ? HorizontalSpacer : VerticalSpacer);

                if (child.Corners.Width() > lineWidth)
                {
                    child.RealignShapes(lineWidth);
                    child.ResizeShape();
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

            this.LineLength = 0;
            // set line length to max size.
            foreach (var line in lines)
            {
                if (line.Count > this.LineLength)
                {
                    this.LineLength = line.Count;
                }
            }

            // ensure all children are multi-lined.
            foreach (var child in this.Children)
            {
                if (child.HasChildren())
                {
                    var defaultLineLength = (int)Math.Round(child.Children.Count / 2D, 0, MidpointRounding.ToPositiveInfinity);
                    if (child.LineLength > defaultLineLength)
                    {
                        child.LineLength = defaultLineLength;
                        child.RealignShapes();
                    }
                }
            }

            this.ResizeShape();

            if (this.Corners.Width() > maxWidth)
            {
                this.RealignShapes(maxWidth);
            }
        }

        internal bool ShapesTooLong(int maxRightSide)
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

            var maxRightSideMinusOneShape = maxRightSide - UltimateShapeWidth;
            if (!(this.Corners.LeftSide < maxRightSideMinusOneShape))
            {
                return false;
            }

            var newMaxWidth = maxRightSide - this.Corners.LeftSide;

            this.logger.Debug("To far over ({CurrentWidth} vs {NewMaxWidth}) : {Shape}", (double)this.Corners.Width() / 1000, (double)newMaxWidth / 1000, this);

            // fit shape inside page
            this.RealignShapes(newMaxWidth);

            this.logger.Debug("New width {CurrentWidth} : {Shape}", (double)this.Corners.Width() / 1000, this);

            // reset children to default representation.
            foreach (var child in this.Children.Where(child => child.HasChildren()))
            {
                child.RealignShapes(newMaxWidth);
            }

            return true;
        }

        /// <summary>
        ///     Calculate # of total children.
        /// </summary>
        /// <returns>count of children, iterating.</returns>
        internal int TotalChildrenCount()
        {
            return !this.HasChildren() ? 1 : this.Children.Sum(child => child.TotalChildrenCount());
        }

        /// <summary>
        ///     Remove all records of shape neighbours.
        /// </summary>
        private void ClearNeighbours()
        {
            // reset all shapes.
            foreach (var shape in this.Children)
            {
                shape.ShapeBelow = null;
                shape.ShapeAbove = null;
                shape.ShapeToRight = null;
                shape.ShapeToLeft = null;
                shape.IncreasedHeight = false;
            }
        }

        private bool MoveSurroundingShapes()
        {
            var changed = false;

            // move this shape.
            double defaultSpacer;
            changed = Align(this);

            // establish surrounding shapes
            var surrounds = new[] { this.ShapeToLeft, this.ShapeToRight, this.ShapeAbove, this.ShapeBelow };

            // move surrounding shapes
            foreach (var surround in surrounds)
            {
                if (surround is null)
                {
                    continue;
                }

                changed |= Align(surround);
            }

            // confirm parent is the right size.
            var parentShape = this.ParentShape;
            if (parentShape != null)
            {
                changed |= parentShape.ResizeShape();
            }

            return changed;
        }

        private bool MoveToPosition(int topSide, int leftSide)
        {
            if (this.Corners.TopSide.Equals(topSide) && this.Corners.LeftSide.Equals(leftSide))
            {
                return false;
            }

            this.logger.Debug("Moving: {Shape}", this);

            // execute movement
            var leftMovement = this.Corners.LeftSide - leftSide;
            var topMovement = topSide - this.Corners.TopSide;

            // move left
            this.logger.Debug("Left by {Movement}", (double)leftMovement / 1000);
            var currentCorners = this.Corners;
            currentCorners.LeftSide -= leftMovement;
            currentCorners.RightSide -= leftMovement;
            this.Corners = currentCorners;

            // move up
            this.logger.Debug("Up by {Movement}", (double)topMovement / 1000);
            currentCorners = this.Corners;
            currentCorners.TopSide += topMovement;
            currentCorners.BottomSide += topMovement;
            this.Corners = currentCorners;

            this.logger.Debug("New position: {Corners}", this.Corners.ToString());

            // move children
            foreach (var child in this.Children)
            {
                var childTopSide = topMovement + child.Corners.TopSide;
                var childLeftSide = child.Corners.LeftSide - leftMovement;

                child.MoveToPosition(childTopSide, childLeftSide);
            }

            return true;
        }

        private void RemoveChildShape(DiagramShape child)
        {
            if (this.Children.Contains(child))
            {
                this.Children.Remove(child);
            }

            if ((child.ParentShape != null) && child.ParentShape.Equals(this))
            {
                child.ParentShape = null;
            }

            child.ClearNeighbours();

            foreach (var remainingChildren in this.Children)
            {
                remainingChildren.MoveSurroundingShapes();
            }
        }

        private bool ResizeShape()
        {
            if (this.IncreasedHeight)
            {
                return false;
            }

            Corners newCorners;
            if (this.HasChildren())
            {
                newCorners = this.Corners;
                var children = this.Children;

                var minLeft = int.MaxValue;
                var maxRight = int.MinValue;
                var minBottom = int.MaxValue;
                var maxTop = int.MinValue;

                foreach (var child in children)
                {
                    var shapeCorners = child.Corners;
                    maxTop = shapeCorners.TopSide > maxTop ? shapeCorners.TopSide : maxTop;
                    minBottom = shapeCorners.BottomSide < minBottom ? shapeCorners.BottomSide : minBottom;
                    maxRight = shapeCorners.RightSide > maxRight ? shapeCorners.RightSide : maxRight;
                    minLeft = shapeCorners.LeftSide < minLeft ? shapeCorners.LeftSide : minLeft;
                }

                var leftSide = minLeft - LeftPadding;
                var rightSide = maxRight + RightPadding;
                var newWidth = rightSide - leftSide;

                if (rightSide < leftSide)
                {
                    throw new InvalidOperationException("This can't happen!");
                }

                var bottomSide = minBottom - BottomPadding;
                var topSide = maxTop + TopPadding;
                var newHeight = topSide - bottomSide;

                newCorners.BottomSide = newCorners.TopSide - newHeight;
                newCorners.RightSide = newCorners.LeftSide + newWidth;
            }
            else
            {
                newCorners = this.Corners;
                newCorners.BottomSide = newCorners.TopSide - UltimateShapeHeight;
                newCorners.RightSide = newCorners.LeftSide + UltimateShapeWidth;
            }

            if (this.Corners.Equals(newCorners))
            {
                return false;
            }

            this.logger.Debug("Resizing: {Shape}", this);
            this.logger.Debug("New size for shape: {Corners}", newCorners.ToString());
            this.Corners = newCorners;

            this.ParentShape?.ResizeShape();

            return true;
        }
    }
}