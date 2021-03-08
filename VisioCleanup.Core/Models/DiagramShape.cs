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

        /// <summary>Gets or sets parent shape of curent shape.</summary>
        private DiagramShape? parentShape;

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

        /// <summary>Gets or sets base of the shape.</summary>
        public int BaseSide
        {
            get => this.baseSide;
            set
            {
                this.baseSide = value;

                // move shape below
                if (this.Below is not null)
                {
                    // calculate movement
                    var movement = this.Below.TopSide - (this.baseSide - ConvertMeasurement(AppConfig!.VerticalSpacing));

                    this.logger.Debug("Moving {shape} by {movement} vertical.", this.Below, movement);
                    this.Below.MoveVertical(movement);
                }

                // move shape on right
                if (this.Right is not null)
                {
                    // calculate movement
                    var movement = this.Right.TopSide - this.TopSide;

                    this.logger.Debug("Moving {shape} by {movement} vertical.", this.Right, movement);
                    this.Right.MoveVertical(movement);
                }
            }
        }

        /// <summary>Gets collection of child shapes.</summary>
        public Collection<DiagramShape> Children { get; }

        /// <summary>Gets or sets left side of the shape.</summary>
        public int LeftSide { get; set; }

        /// <summary>Gets or sets right side of the shape.</summary>
        public int RightSide
        {
            get => this.rightSide;
            set
            {
                this.rightSide = value;

                // move shape to right to spacing width
                if (this.Right is not null)
                {
                    // calculate movement
                    var movement = this.Right.LeftSide - (this.rightSide + ConvertMeasurement(AppConfig!.HorizontalSpacing));

                    this.logger.Debug("Moving {shape} by {movement} horizontal.", this.Right, movement);
                    this.Right.MoveHorizontal(movement);
                }

                // align shape below to left hand side.
                if (this.Below is not null)
                {
                    // calculate movement
                    var movement = this.Below.LeftSide - this.LeftSide;

                    this.logger.Debug("Moving {shape} by {movement} horizontal.", this.Below, movement);
                    this.Below.MoveHorizontal(movement);
                }
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

        /// <summary>Gets or sets the shape above.</summary>
        private DiagramShape? Above
        {
            get => this.above;
            set
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

        /// <summary>Gets or sets the shape to the left.</summary>
        private DiagramShape? Left
        {
            get => this.left;
            set
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

        /// <summary>Gets or sets the shape to the right.</summary>
        private DiagramShape? Right
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
            childShape.parentShape = this;
        }

        /// <summary>Correct shape and child shapes.</summary>
        /// <returns>If any shape was changed.</returns>
        public bool CorrectDiagram()
        {
            var result = false;

            // if no shape above or to left, then move
            if (this.parentShape is not null && this.Above is null && this.Left is null)
            {
                var newLeft = this.parentShape.LeftSide + ConvertMeasurement(AppConfig!.Left);
                var newTop = this.parentShape.TopSide - ConvertMeasurement(AppConfig!.Top);

                var topMovement = this.TopSide - newTop;

                var leftMovement = this.LeftSide - newLeft;

                if ((topMovement != 0) || (leftMovement != 0))
                {
                    this.logger.Debug("Aligning {shape} to {parent}.", this, this.parentShape);
                    this.MoveVertical(topMovement);
                    this.MoveHorizontal(leftMovement);
                    result = true;
                }
            }

            // resize shape
            if (this.ResizeShape())
            {
                result = true;
            }

            // depth first correction process
            foreach (var diagramShape in this.Children)
            {
                if (diagramShape.CorrectDiagram())
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>Calculate the height of the shape.</summary>
        /// <returns>Height.</returns>
        public int Height() => this.TopSide - this.BaseSide;

        /// <summary>Resize the shape based on appconfig.</summary>
        /// <returns>If shape changed.</returns>
        public bool ResizeShape()
        {
            int width;
            int height;
            if (this.Children.Count > 0)
            {
                var children = this.Children;

                var minLeftSide = children.Select(shape => shape.LeftSide).Min() - ConvertMeasurement(AppConfig!.Left);
                var maxRightSide = children.Select(shape => shape.RightSide).Max() + ConvertMeasurement(AppConfig!.Right);
                width = maxRightSide - minLeftSide;

                var minBaseSide = children.Select(shape => shape.BaseSide).Min() - ConvertMeasurement(AppConfig!.Base);
                var maxTopSide = children.Select(shape => shape.TopSide).Max() + ConvertMeasurement(AppConfig!.Top);
                height = maxTopSide - minBaseSide;
            }
            else
            {
                height = ConvertMeasurement(AppConfig!.Height);
                width = ConvertMeasurement(AppConfig!.Width);
            }

            var newBaseSide = this.TopSide - height;
            var newRightSide = this.LeftSide + width;

            if ((this.RightSide == newRightSide) && (this.BaseSide == newBaseSide))
            {
                return false;
            }

            this.logger.Debug("Resizing: {Shape}", this);
            this.logger.Debug("New size for shape: {Corners}", this.CornerString());
            this.RightSide = newRightSide;
            this.BaseSide = newBaseSide;
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
            if (!(this.Children.Count > 0))
            {
                return;
            }

            // reset all shapes.
            var children = this.Children;

            const double Tolerance = 5000;

            var lines = children.OrderBy(shape => shape.LeftSide).Select(shape => shape.LeftSide);
            foreach (var line in lines.Distinct())
            {
                bool AbsoluteShapeSize(DiagramShape shape)
                {
                    var side = shape.LeftSide - line;
                    return Math.Abs(side) < Tolerance;
                }

                var diagramShapes = children.Where(AbsoluteShapeSize);
                var bottomOrdered = diagramShapes.OrderBy(shape => shape.BaseSide).ToList();
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

            lines = children.OrderBy(shape => shape.TopSide).Select(shape => shape.TopSide);
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

        private string CornerString() =>
            $"Top: {ConvertMeasurement(this.TopSide)}, Right: {ConvertMeasurement(this.RightSide)}, Base: {ConvertMeasurement(this.BaseSide)}, Left: {ConvertMeasurement(this.LeftSide)}";

        private void MoveHorizontal(int movement)
        {
            this.LeftSide -= movement;
            this.RightSide -= movement;
        }

        private void MoveVertical(int movement)
        {
            this.TopSide -= movement;
            this.BaseSide -= movement;
        }
    }
}