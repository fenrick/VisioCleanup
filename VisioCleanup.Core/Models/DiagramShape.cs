// -----------------------------------------------------------------------
// <copyright file="DiagramShape.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Serilog;

    using VisioCleanup.Core.Models.Config;

    /// <summary>Representation of a single shape in a visio diagram.</summary>
    public class DiagramShape
    {
        private readonly ILogger logger;

        private DiagramShape? above;

        private int baseSide;

        private DiagramShape? below;

        private DiagramShape? left;

        private DiagramShape? right;

        private int rightSide;

        /// <summary>Initialises a new instance of the <see cref="DiagramShape" /> class.</summary>
        /// <param name="visioId">Visio shape ID.</param>
        public DiagramShape(int visioId)
        {
            this.logger = Log.ForContext<DiagramShape>();
            this.VisioId = visioId;
            this.Children = new Collection<DiagramShape>();
            this.TopSide = ConvertMeasurement(AppConfig!.Height);
            this.LeftSide = 0;
            this.RightSide = ConvertMeasurement(AppConfig!.Width);
            this.BaseSide = 0;
        }

        /// <summary>Gets the shape above.</summary>
        public DiagramShape? Above
        {
            get => this.above;
            private set
            {
                this.above = value;
                if (value == null)
                {
                    return;
                }

                if (value.Below == null)
                {
                    value.Below = this;
                }
                else if (!value.Below.Equals(this))
                {
                    value.Below = this;
                }
            }
        }

        /// <summary>Gets or sets base of the shape.</summary>
        public int BaseSide
        {
            get => this.baseSide;
            set
            {
                if (this.baseSide == value)
                {
                    return;
                }

                // check if surrounding shapes are "locked" or at Vertical Spacing
                // if so move them before moving this.
                if (this.Below is not null)
                {
                    // calculate movement
                    var movement = this.Below.TopSide - (value - ConvertMeasurement(AppConfig!.VerticalSpacing));

                    this.logger.Debug("Moving {shape} by {movement} vertical.", this.Below, movement);
                    this.Below.MoveVertical(movement);
                }

                this.baseSide = value;
            }
        }

        /// <summary>Gets collection of child shapes.</summary>
        public Collection<DiagramShape> Children { get; }

        /// <summary>Gets the shape to the left.</summary>
        public DiagramShape? Left
        {
            get => this.left;
            private set
            {
                this.left = value;
                if (value == null)
                {
                    return;
                }

                if (value.Right == null)
                {
                    value.Right = this;
                }
                else if (!value.Right.Equals(this))
                {
                    value.Right = this;
                }
            }
        }

        /// <summary>Gets or sets left side of the shape.</summary>
        public int LeftSide { get; set; }

        /// <summary>Gets or sets right side of the shape.</summary>
        public int RightSide
        {
            get => this.rightSide;
            set
            {
                if (this.rightSide == value)
                {
                    return;
                }

                // check if surrounding shapes are "locked" or at Vertical Spacing
                // if so move them before moving this.
                if (this.Right is not null)
                {
                    // calculate movement
                    var movement = this.Right.LeftSide - (value + ConvertMeasurement(AppConfig!.HorizontalSpacing));

                    this.logger.Debug("Moving {shape} by {movement} horizontal.", this.Right, movement);
                    this.Right.MoveHorizontal(movement);
                }

                this.rightSide = value;
            }
        }

        /// <summary>Gets the shape text.</summary>
        public string? ShapeText { get; init; }

        /// <summary>Gets or sets the shape type.</summary>
        public ShapeType ShapeType { get; set; }

        /// <summary>Gets or sets top of the shape.</summary>
        public int TopSide { get; set; }

        /// <summary>Gets or sets visio shape id.</summary>
        public int VisioId { get; set; }

        internal static AppConfig? AppConfig { get; set; }

        /// <summary>Gets or sets parent shape of curent shape.</summary>
        internal DiagramShape? ParentShape { get; set; }

        /// <summary>Gets or sets the shape to the right.</summary>
        internal DiagramShape? Right
        {
            get => this.right;
            set
            {
                this.right = value;
                if (value == null)
                {
                    return;
                }

                if (value.Left == null)
                {
                    value.Left = this;
                }
                else if (!value.Left.Equals(this))
                {
                    value.Left = this;
                }
            }
        }

        /// <summary>Gets or sets the shape below.</summary>
        private DiagramShape? Below
        {
            get => this.below;
            set
            {
                this.below = value;
                if (value == null)
                {
                    return;
                }

                if (value.Above == null)
                {
                    value.Above = this;
                }
                else if (!value.Above.Equals(this))
                {
                    value.Above = this;
                }
            }
        }

        /// <summary>Convert a visio measurement into an easier mathematical model.</summary>
        /// <param name="measurement">Measurement from visio.</param>
        /// <returns>Easier internal measurement.</returns>
        public static int ConvertMeasurement(double measurement) => (int)(Math.Round(measurement, 3, MidpointRounding.AwayFromZero) * 1000);

        /// <summary>Convert an easier measurement back to visio model.</summary>
        /// <param name="measurement">Easier internal measurement.</param>
        /// <returns>Measurement for visio.</returns>
        public static double ConvertMeasurement(int measurement) => (double)measurement / 1000;

        /// <summary>Add child shape to parent.</summary>
        /// <param name="childShape">New child shape of this shape.</param>
        public void AddChildShape(DiagramShape childShape)
        {
            if (!this.Children.Contains(childShape))
            {
                this.Children.Add(childShape);
            }

            // add to array
            childShape.ParentShape = this;
        }

        public bool AlignToParent()
        {
            if (this.ParentShape is null)
            {
                return false;
            }

            var newLeft = this.ParentShape.LeftSide + ConvertMeasurement(AppConfig!.Left);
            var newTop = this.ParentShape.TopSide - ConvertMeasurement(AppConfig!.Top);

            var topMovement = this.TopSide - newTop;

            var leftMovement = this.LeftSide - newLeft;

            if ((topMovement == 0) && (leftMovement == 0))
            {
                return false;
            }

            this.logger.Debug("Aligning {shape} to {parent}.", this, this.ParentShape);
            this.MoveVertical(topMovement);
            this.MoveHorizontal(leftMovement);
            return true;
        }

        public bool FixAlignment()
        {
            var result = false;

            // align vertically on left side
            if (this.Below is not null)
            {
                if (this.Below.LeftSide != this.LeftSide)
                {
                    // calculate movement
                    var movement = this.Below.LeftSide - this.LeftSide;

                    this.logger.Debug("Moving {shape} by {movement} horizontal.", this.Below, movement);
                    this.Below.MoveHorizontal(movement);
                    result = true;
                }
            }

            // align horizontally on top side
            if (this.Right is not null)
            {
                if (this.Right.TopSide != this.TopSide)
                {
                    // calculate movement
                    var movement = this.Right.TopSide - this.TopSide;

                    this.logger.Debug("Moving {shape} by {movement} vertical.", this.Right, movement);
                    this.Right.MoveVertical(movement);
                    result = true;
                }
            }

            return result;
        }

        public bool FixSpacing()
        {
            var result = false;

            // fix vertical spacing
            if (this.Below is not null)
            {
                if (Math.Abs(this.Below.TopSide - this.baseSide) != ConvertMeasurement(AppConfig!.VerticalSpacing))
                {
                    // calculate movement
                    var movement = this.Below.TopSide - (this.baseSide - ConvertMeasurement(AppConfig!.VerticalSpacing));

                    this.logger.Debug("Moving {shape} by {movement} vertical.", this.Below, movement);
                    this.Below.MoveVertical(movement);
                    result = true;
                }
            }

            // fix horizontal spacing
            if (this.Right is not null)
            {
                if (Math.Abs(this.Right.LeftSide - this.RightSide) != ConvertMeasurement(AppConfig!.HorizontalSpacing))
                {
                    // calculate movement
                    var movement = this.Right.LeftSide - (this.RightSide + ConvertMeasurement(AppConfig!.HorizontalSpacing));

                    this.logger.Debug("Moving {shape} by {movement} horizontal.", this.Right, movement);
                    this.Right.MoveHorizontal(movement);
                    result = true;
                }
            }

            return result;
        }

        /// <summary>Calculate the height of the shape.</summary>
        /// <returns>Height.</returns>
        public int Height() => this.TopSide - this.BaseSide;

        /// <summary>Resize the shape based on appconfig.</summary>
        public bool ResizeShape()
        {
            int? newLeftSide, newRightSide, newBaseSide, newTopSide;
            if (this.HasChildren())
            {
                var children = this.Children;

                newLeftSide = children.Select(shape => shape?.LeftSide).Min() - ConvertMeasurement(AppConfig!.Left);
                newRightSide = children.Select(shape => shape?.RightSide).Max() + ConvertMeasurement(AppConfig!.Right);

                newBaseSide = children.Select(shape => shape?.BaseSide).Min() - ConvertMeasurement(AppConfig!.Base);
                newTopSide = children.Select(shape => shape?.TopSide).Max() + ConvertMeasurement(AppConfig!.Top);
            }
            else
            {
                newTopSide = this.TopSide;
                newBaseSide = this.TopSide - ConvertMeasurement(AppConfig!.Height);
                newRightSide = this.LeftSide + ConvertMeasurement(AppConfig!.Width);
                newLeftSide = this.LeftSide;
            }

            if ((this.LeftSide == newLeftSide) && (this.RightSide == newRightSide) && (this.TopSide == newTopSide) && (this.BaseSide == newBaseSide))
            {
                return false;
            }

            this.logger.Debug("Resizing: {Shape}", this);
            this.logger.Debug("New size for shape: {Corners}", this.CornerString());
            this.TopSide = newTopSide ?? this.TopSide;
            this.LeftSide = newLeftSide ?? this.LeftSide;
            this.RightSide = newRightSide ?? this.RightSide;
            this.BaseSide = newBaseSide ?? this.BaseSide;
            return true;
        }

        /// <inheritdoc />
        public override string ToString() => $"{this.VisioId}: {this.ShapeText}";

        /// <summary>Calculate the width of the shape.</summary>
        /// <returns>Width.</returns>
        public int Width() => this.RightSide - this.LeftSide;

        /// <summary>Map all neighbour shapes within tolerance of 10.</summary>
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

            const double Tolerance = 5000;

            var lines = children.OrderBy(shape => shape?.LeftSide).Select(shape => shape.LeftSide);
            foreach (var line in lines.Distinct())
            {
                bool AbsoluteShapeSize(DiagramShape shape)
                {
                    var side = shape.LeftSide - line;
                    return Math.Abs(side) < Tolerance;
                }

                var diagramShapes = children.Where(AbsoluteShapeSize);
                var bottomOrdered = diagramShapes.OrderBy(shape => shape?.BaseSide).ToList();
                DiagramShape? currentShape = null;

                foreach (var shape in bottomOrdered)
                {
                    switch (currentShape)
                    {
                        case not null when currentShape.BaseSide.Equals(shape.BaseSide):
                            // overlap!
                            throw new NotImplementedException("No idea what to do yet with this!");
                        case not null when currentShape.BaseSide >= shape.BaseSide:
                            continue;
                        case not null when (shape.BaseSide - currentShape.TopSide) < (Tolerance * 2):
                            shape.Below = currentShape;

                            currentShape = shape;
                            break;
                        default:
                            currentShape = shape;
                            break;
                    }
                }
            }

            lines = children.OrderBy(shape => shape?.TopSide).Select(shape => shape.TopSide);
            foreach (var line in lines.Distinct())
            {
                bool AbsoluteShapeSize(DiagramShape shape)
                {
                    var side = shape.TopSide - line;
                    return Math.Abs(side) < Tolerance;
                }

                var diagramShapes = children.Where(AbsoluteShapeSize);
                var bottomOrdered = diagramShapes.OrderBy(shape => shape.LeftSide);
                DiagramShape? currentShape = null;

                foreach (var shape in bottomOrdered)
                {
                    switch (currentShape)
                    {
                        case not null when currentShape.LeftSide.Equals(shape.LeftSide):
                            // overlap!
                            throw new NotImplementedException("No idea what to do yet with this!");
                        case not null when currentShape.LeftSide >= shape.LeftSide:
                            continue;
                        case not null when (shape.LeftSide - currentShape.RightSide) < (Tolerance * 2):
                            shape.Left = currentShape;

                            currentShape = shape;
                            break;
                        default:
                            currentShape = shape;
                            break;
                    }
                }
            }
        }

        /// <summary>Does this shape have children.</summary>
        /// <returns>true if at least one.</returns>
        internal bool HasChildren() => this.Children.Count > 0;

        /// <summary>Remove all records of shape neighbours.</summary>
        private void ClearNeighbours()
        {
            // reset all shapes.
            foreach (var shape in this.Children)
            {
                shape.below = null;
                shape.above = null;
                shape.right = null;
                shape.left = null;
            }
        }

        private string CornerString() =>
            $"Top: {ConvertMeasurement(this.TopSide)}, Right: {ConvertMeasurement(this.RightSide)}, Base: {ConvertMeasurement(this.BaseSide)}, Left: {ConvertMeasurement(this.LeftSide)}";

        private void MoveHorizontal(int movement)
        {
            this.LeftSide -= movement;
            this.RightSide -= movement;

            foreach (var shape in this.Children)
            {
                this.logger.Debug("Moving {shape} by {movement} horizontal.", shape, movement);
                shape.MoveHorizontal(movement);
            }
        }

        private void MoveVertical(int movement)
        {
            this.TopSide -= movement;
            this.BaseSide -= movement;

            foreach (var shape in this.Children)
            {
                this.logger.Debug("Moving {shape} by {movement} vertical.", shape, movement);
                shape.MoveVertical(movement);
            }
        }
    }
}