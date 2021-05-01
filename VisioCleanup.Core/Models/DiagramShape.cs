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

    using JetBrains.Annotations;

    using Serilog;

    using VisioCleanup.Core.Models.Config;

    /// <summary>Representation of a single shape in a visio diagram.</summary>
    public class DiagramShape
    {
        private readonly ILogger logger;

        private int baseSide;

        private DiagramShape? below;

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
            this.RightSide = ConvertMeasurement(AppConfig.Width);
            this.BaseSide = 0;
            this.Master = string.Empty;
            this.ShapeText = string.Empty;
            this.SortValue = string.Empty;
        }

        /// <summary>Gets the shape above.</summary>
        public DiagramShape? Above { get; private set; }

        /// <summary>Gets or sets base of the shape.</summary>
        public int BaseSide
        {
            get => this.baseSide;
            set
            {
                this.baseSide = value;

                // move shape below
                int movement;
                if (this.Below is not null)
                {
                    // calculate movement
                    movement = this.Below.TopSide - (this.baseSide - ConvertMeasurement(AppConfig!.VerticalSpacing));

                    if (movement != 0)
                    {
                        this.logger.Debug("Moving {Shape} by {Movement} vertical", this.Below, movement);
                        this.Below.MoveVertical(movement);
                    }
                }

                // move shape on right
                if (this.Right is null)
                {
                    return;
                }

                // calculate movement
                movement = this.Right.TopSide - this.TopSide;
                if (movement == 0)
                {
                    return;
                }

                this.logger.Debug("Moving {Shape} by {Movement} vertical", this.Right, movement);
                this.Right.MoveVertical(movement);
            }
        }

        /// <summary>Gets or sets the shape below.</summary>
        public DiagramShape? Below
        {
            get => this.below;
            set
            {
                // remove existing relationship.
                if (this.below is not null)
                {
                    this.below.Above = null;
                }

                // set value.
                this.below = value;

                if (this.below is null)
                {
                    return;
                }

                // set relationship.
                this.below.Above = this;

                // calculate movement
                var movement = this.below.LeftSide - this.LeftSide;

                if (movement != 0)
                {
                    this.logger.Debug("Moving {Shape} by {Movement} horizontal", this.below, movement);
                    this.below.MoveHorizontal(movement);
                }

                // calculate movement
                movement = this.below.TopSide - (this.baseSide - ConvertMeasurement(AppConfig!.VerticalSpacing));

                if (movement == 0)
                {
                    return;
                }

                this.logger.Debug("Moving {Shape} by {Movement} vertical", this.below, movement);
                this.below.MoveVertical(movement);
            }
        }

        /// <summary>Gets collection of child shapes.</summary>
        public Collection<DiagramShape> Children { get; }

        /// <summary>Gets or sets how deep is the rendered children.</summary>
        public int ChildrenDepth { get; set; }

        /// <summary>Gets the shape to the left.</summary>
        public DiagramShape? Left { get; private set; }

        /// <summary>Gets or sets left side of the shape.</summary>
        public int LeftSide { get; set; }

        /// <summary>Gets the stencil used for drawing shape.</summary>
        public string Master { get; init; }

        /// <summary>Gets parent shape of curent shape.</summary>
        public DiagramShape? ParentShape { get; private set; }

        /// <summary>Gets or sets the shape to the right.</summary>
        public DiagramShape? Right
        {
            get => this.right;
            set
            {
                // remove existing relationship.
                if (this.right is not null)
                {
                    this.right.Left = null;
                }

                // set value.
                this.right = value;

                if (this.right is null)
                {
                    return;
                }

                // set relationship.
                this.right.Left = this;

                // calculate movement
                var movement = this.right.LeftSide - (this.rightSide + ConvertMeasurement(AppConfig!.HorizontalSpacing));

                if (movement != 0)
                {
                    this.logger.Debug("Moving {Shape} by {Movement} horizontal", this.right, movement);
                    this.right.MoveHorizontal(movement);
                }

                // calculate movement
                movement = this.right.TopSide - this.TopSide;
                if (movement == 0)
                {
                    return;
                }

                this.logger.Debug("Moving {Shape} by {Movement} vertical", this.right, movement);
                this.right.MoveVertical(movement);
            }
        }

        /// <summary>Gets or sets <see cref="VisioCleanup.Core.Models.DiagramShape.right" /> side of the shape.</summary>
        public int RightSide
        {
            get => this.rightSide;
            set
            {
                this.rightSide = value;

                // move shape to right to spacing width
                int movement;
                if (this.Right is not null)
                {
                    // calculate movement
                    movement = this.Right.LeftSide - (this.rightSide + ConvertMeasurement(AppConfig!.HorizontalSpacing));

                    if (movement != 0)
                    {
                        this.logger.Debug("Moving {Shape} by {Movement} horizontal", this.Right, movement);
                        this.Right.MoveHorizontal(movement);
                    }
                }

                // align shape below to left hand side.
                if (this.Below is null)
                {
                    return;
                }

                // calculate movement
                movement = this.Below.LeftSide - this.LeftSide;

                if (movement == 0)
                {
                    return;
                }

                this.logger.Debug("Moving {Shape} by {Movement} horizontal", this.Below, movement);
                this.Below.MoveHorizontal(movement);
            }
        }

        /// <summary>Gets a unique shape identifier.</summary>
        public string? ShapeIdentifier { get; init; }

        /// <summary>Gets the shape text.</summary>
        public string ShapeText { get; init; }

        /// <summary>Gets or sets the shape type.</summary>
        public ShapeType ShapeType { get; set; }

        /// <summary>Gets value used to sort shapes.</summary>
        public string? SortValue
        {
            get;
            [UsedImplicitly]
            init;
        }

        /// <summary>Gets or sets top of the shape.</summary>
        public int TopSide { get; set; }

        /// <summary>Gets or sets visio shape id.</summary>
        public int VisioId { get; set; }

        internal static AppConfig? AppConfig { get; set; }

        /// <summary>Convert a visio <paramref name="measurement" /> into an easier mathematical model.</summary>
        /// <param name="measurement">Measurement from visio.</param>
        /// <returns>Easier <see langword="internal" /> measurement.</returns>
        public static int ConvertMeasurement(double measurement) => (int)(Math.Round(measurement, 3, MidpointRounding.AwayFromZero) * 1000);

        /// <summary>Convert an easier <paramref name="measurement" /> back to visio model.</summary>
        /// <param name="measurement">Easier <see langword="internal" /> measurement.</param>
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

        /// <summary>Correct shape and child shapes.</summary>
        /// <returns>If any shape was changed.</returns>
        public bool CorrectDiagram()
        {
            var result = this.FixPosition();

            // depth first correction process
            foreach (var diagramShape in this.Children.Where(diagramShape => diagramShape.CorrectDiagram()))
            {
                result = true;
            }

            // resize shape
            if (this.ResizeShape())
            {
                result = true;
            }

            return result;
        }

        /// <summary>Does this shape have a parent.</summary>
        /// <returns>True if a parent.</returns>
        public bool HasParent() => this.ParentShape is not null;

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
                var minLeftSide = this.Children.Select(shape => shape.LeftSide).Min() - ConvertMeasurement(AppConfig!.Left);
                var maxRightSide = this.Children.Select(shape => shape.RightSide).Max() + ConvertMeasurement(AppConfig.Right);
                width = maxRightSide - minLeftSide;

                var minBaseSide = this.Children.Select(shape => shape.BaseSide).Min() - ConvertMeasurement(AppConfig.Base);
                var maxTopSide = this.Children.Select(shape => shape.TopSide).Max() + ConvertMeasurement(AppConfig.Top);
                height = maxTopSide - minBaseSide;
            }
            else
            {
                height = ConvertMeasurement(AppConfig!.Height);
                width = ConvertMeasurement(AppConfig.Width);
            }

            var newBaseSide = this.TopSide - height;
            var newRightSide = this.LeftSide + width;

            if (this.RightSide == newRightSide)
            {
                if (this.BaseSide == newBaseSide)
                {
                    return false;
                }
            }

            this.logger.Debug("Resizing: {Shape}", this);
            this.RightSide = newRightSide;
            this.BaseSide = newBaseSide;
            this.logger.Debug("New size for {Shape}: {Corners}", this, this.CornerString());
            return true;
        }

        /// <inheritdoc />
        public override string ToString() => $"{this.VisioId}: {this.ShapeText}";

        /// <summary>Calculate the width of the shape.</summary>
        /// <returns>Width.</returns>
        public int Width() => this.RightSide - this.LeftSide;

        /// <summary>Map all neighbour shapes within tolerance of 10.</summary>
        /// <exception cref="System.NotImplementedException">No idea what to do yet with this.</exception>
        internal void FindNeighbours()
        {
            if (this.Children.Count <= 0)
            {
                return;
            }

            // reset all shapes, without triggering movements.
            var children = this.Children;
            foreach (var child in children)
            {
                child.Right = null;
                child.Below = null;
            }

            double tolerance = ConvertMeasurement(AppConfig!.HorizontalSpacing + AppConfig!.VerticalSpacing) / 2;

            var lines = children.OrderBy(shape => shape.LeftSide).Select(shape => shape.LeftSide);
            foreach (var line in lines.Distinct())
            {
                bool AbsoluteShapeSize(DiagramShape shape)
                {
                    var side = shape.LeftSide - line;
                    return Math.Abs(side) < tolerance;
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
                            this.logger.Error("Weird circumstances, check diagram for overlaps between {Shape} and {Shape2}", currentShape, shape);
                            continue;
                        case not null when currentShape.BaseSide >= shape.BaseSide:
                            continue;
                        case not null when (shape.BaseSide - currentShape.TopSide) < (tolerance * 2):
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
                    return Math.Abs(side) < tolerance;
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
                            this.logger.Error("Weird circumstances, check diagram for overlaps between {Shape} and {Shape2}", currentShape, shape);
                            continue;
                        case not null when currentShape.LeftSide >= shape.LeftSide:
                            continue;
                        case not null when (shape.LeftSide - currentShape.RightSide) < (tolerance * 2):
                            currentShape.Right = shape;

                            currentShape = shape;
                            break;
                        default:
                            currentShape = shape;
                            break;
                    }
                }
            }
        }

        internal int TotalChildrenCount()
        {
            return !this.Children.Any() ? 1 : 1 + this.Children.Sum(child => child.TotalChildrenCount());
        }

        private string CornerString() =>
            $"Top: {ConvertMeasurement(this.TopSide)}, Left: {ConvertMeasurement(this.LeftSide)}, Width: {ConvertMeasurement(this.Width())}, Height: {ConvertMeasurement(this.Height())}";

        private bool FixPosition()
        {
            var result = false;

            // if no shape above or to left, then move
            if (this.ParentShape is null)
            {
                return false;
            }

            // top left
            if (this.Above is null && this.Left is null)
            {
                var newLeft = this.ParentShape.LeftSide + ConvertMeasurement(AppConfig!.Left);
                var newTop = this.ParentShape.TopSide - ConvertMeasurement(AppConfig.Top);

                var topMovement = this.TopSide - newTop;

                var leftMovement = this.LeftSide - newLeft;

                if (topMovement != 0)
                {
                    this.logger.Debug("Aligning {Shape} to {Parent}", this, this.ParentShape);
                    this.MoveVertical(topMovement);
                    this.MoveHorizontal(leftMovement);
                    result = true;
                }

                if (leftMovement != 0)
                {
                    this.logger.Debug("Aligning {Shape} to {Parent}", this, this.ParentShape);
                    this.MoveVertical(topMovement);
                    this.MoveHorizontal(leftMovement);
                    result = true;
                }
            }

            // move shape to right to spacing width
            int movement;
            if (this.Right is not null)
            {
                // calculate movement
                movement = this.Right.LeftSide - (this.rightSide + ConvertMeasurement(AppConfig!.HorizontalSpacing));

                if (movement != 0)
                {
                    this.logger.Debug("Moving {Shape} by {Movement} horizontal", this.Right, movement);
                    this.Right.MoveHorizontal(movement);
                    result = true;
                }

                // calculate movement
                movement = this.Right.TopSide - this.TopSide;
                if (movement != 0)
                {
                    this.logger.Debug("Moving {Shape} by {Movement} vertical", this.Right, movement);
                    this.Right.MoveVertical(movement);
                    result = true;
                }
            }

            // align shape below to left hand side.
            if (this.Below is null)
            {
                return result;
            }

            // calculate movement
            movement = this.Below.LeftSide - this.LeftSide;

            if (movement != 0)
            {
                this.logger.Debug("Moving {Shape} by {Movement} horizontal", this.Below, movement);
                this.Below.MoveHorizontal(movement);
                result = true;
            }

            // calculate movement
            movement = this.Below.TopSide - (this.baseSide - ConvertMeasurement(AppConfig!.VerticalSpacing));

            if (movement == 0)
            {
                return result;
            }

            this.logger.Debug("Moving {Shape} by {Movement} vertical", this.Below, movement);
            this.Below.MoveVertical(movement);
            return true;
        }

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
