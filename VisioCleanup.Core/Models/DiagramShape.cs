﻿// -----------------------------------------------------------------------
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

    public class DiagramShape
    {
        private readonly ILogger logger;

        private DiagramShape? above;

        private int baseSide;

        private DiagramShape? below;

        private DiagramShape? left;

        private DiagramShape? right;

        private int topSide;

        /// <summary>
        ///     Initialises a new instance of the <see cref="DiagramShape" /> class.
        /// </summary>
        /// <param name="visioId">Visio shape ID.</param>
        public DiagramShape(int visioId)
        {
            this.logger = Log.ForContext<DiagramShape>();
            this.VisioId = visioId;
            this.Children = new Collection<DiagramShape>();
            this.TopSide = AppConfig!.Height;
            this.LeftSide = 0;
            this.RightSide = AppConfig!.Width;
            this.BaseSide = 0;
        }

        public static AppConfig? AppConfig { get; set; }

        /// <summary>
        ///     Gets the shape above.
        /// </summary>
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

        /// <summary>
        ///     Gets or sets base of the shape.
        /// </summary>
        public int BaseSide
        {
            get => this.baseSide;
            set
            {
                if (this.baseSide == value)
                {
                    return;
                }

                // calculate movement
                var movement = this.baseSide - value;

                // check if surrounding shapes are "locked" or at Vertical Spacing
                // if so move them before moving this.
                if (this.Below != null)
                {
                    if (Math.Abs(this.Below.BaseSide - this.baseSide) == AppConfig!.VerticalSpacing)
                    {
                        this.Below.MoveVertical(movement);
                    }
                }

                this.baseSide = value;
            }
        }

        public Collection<DiagramShape> Children { get; set; }

        /// <summary>
        ///     Gets the shape to the left.
        /// </summary>
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

        /// <summary>
        ///     Gets or sets left side of the shape.
        /// </summary>
        public int LeftSide { get; set; }

        public DiagramShape? ParentShape { get; set; }

        /// <summary>
        ///     Gets or sets right side of the shape.
        /// </summary>
        public int RightSide { get; set; }

        /// <summary>
        ///     Gets the shape text.
        /// </summary>
        public string? ShapeText { get; init; }

        /// <summary>
        ///     Gets or sets the shape type.
        /// </summary>
        public ShapeType ShapeType { get; set; }

        /// <summary>
        ///     Gets or sets top of the shape.
        /// </summary>
        public int TopSide
        {
            get => this.topSide;
            set
            {
                if (this.topSide == value)
                {
                    return;
                }

                // calculate movement
                var movement = this.topSide - value;

                // check if surrounding shapes are "locked" or at Vertical Spacing
                // if so move them before moving this.
                if (this.Above != null)
                {
                    if (Math.Abs(this.Above.BaseSide - this.TopSide) == AppConfig!.VerticalSpacing)
                    {
                        this.Above.MoveVertical(movement);
                    }
                }

                this.topSide = value;
            }
        }

        public int VisioId { get; set; }

        /// <summary>
        ///     Gets or sets the shape to the right.
        /// </summary>
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

        /// <summary>
        ///     Gets or sets the shape below.
        /// </summary>
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

        /// <summary>
        /// Convert a visio measurement into an easier mathematical model.
        /// </summary>
        /// <param name="measurement">Measurement from visio.</param>
        /// <returns>Easier internal measurement.</returns>
        public static int ConvertMeasurement(double measurement)
        {
            return (int)(Math.Round(measurement, 3, MidpointRounding.AwayFromZero) * 1000);
        }

        /// <summary>
        /// Convert an easier measurement back to visio model.
        /// </summary>
        /// <param name="measurement">Easier internal measurement.</param>
        /// <returns>Measurement for visio.</returns>
        public static double ConvertMeasurement(int measurement)
        {
            return (double)measurement / 1000;
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
        }

        public int Height()
        {
            return this.TopSide - this.BaseSide;
        }

        public void MoveHorizontal(int movement)
        {
            this.LeftSide = this.LeftSide + movement;
            this.RightSide = this.RightSide + movement;
        }

        public void MoveVertical(int movement)
        {
            this.TopSide = this.TopSide + movement;
            this.BaseSide = this.BaseSide + movement;
        }

        public void ResizeShape()
        {
            int newLeftSide, newRightSide, newBaseSide, newTopSide;
            if (this.HasChildren())
            {
                var children = this.Children;

                newLeftSide = children.Select(shape => shape!.LeftSide).Min() - AppConfig!.Left;
                newRightSide = children.Select(shape => shape!.RightSide).Max() + AppConfig!.Right;

                newBaseSide = children.Select(shape => shape!.BaseSide).Min() - AppConfig!.Base;
                newTopSide = children.Select(shape => shape!.TopSide).Max() + AppConfig!.Top;
            }
            else
            {
                newTopSide = this.TopSide;
                newBaseSide = this.TopSide - AppConfig!.Height;
                newRightSide = this.LeftSide + AppConfig!.Width;
                newLeftSide = this.LeftSide;
            }

            if ((this.LeftSide == newLeftSide) && (this.RightSide == newRightSide) && (this.TopSide == newTopSide) && (this.BaseSide == newBaseSide))
            {
                return;
            }

            this.logger.Debug("Resizing: {Shape}", this);
            this.logger.Debug("New size for shape: {Corners}", this.CornerString());
            this.TopSide = newTopSide;
            this.LeftSide = newLeftSide;
            this.RightSide = newRightSide;
            this.BaseSide = newBaseSide;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.VisioId}: {this.ShapeText}";
        }

        public int Width()
        {
            return this.RightSide - this.LeftSide;
        }

        /// <summary>
        ///     Map all neighbour shapes within tolerance of 10.
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

        /// <summary>
        ///     Does this shape have children.
        /// </summary>
        /// <returns>true if at least one.</returns>
        internal bool HasChildren()
        {
            return this.Children.Count > 0;
        }

        /// <summary>
        ///     Remove all records of shape neighbours.
        /// </summary>
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

        private string CornerString()
        {
            return
                $"Top: {ConvertMeasurement(this.TopSide)}, Right: {ConvertMeasurement(this.RightSide)}, Base: {ConvertMeasurement(this.BaseSide)}, Left: {ConvertMeasurement(this.LeftSide)}";
        }
    }
}