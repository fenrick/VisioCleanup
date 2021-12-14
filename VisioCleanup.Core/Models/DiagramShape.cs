// -----------------------------------------------------------------------
// <copyright file="DiagramShape.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using MathNet.Numerics.LinearAlgebra;

using Serilog;

using VisioCleanup.Core.Models.Config;

/// <summary>Representation of a single shape in a visio diagram.</summary>
public class DiagramShape
{
    private const int ConversionDigits = 3;

    private const int ConversionFactor = 1000;

    private const string MovingShapeByMovementDirection = "Moving {Shape} by {Movement} {Direction}";

    private const string VerticalDirection = "vertical";

    private const string HorizontalDirection = "horizontal";

    private const string CornerStringFormat = "Top: {0}, Left: {1}, Width: {2}, Height: {3}";

    private readonly ILogger logger;

    private int baseSide;

    private DiagramShape? diagramShapeBelowThisDiagramShape;

    private DiagramShape? diagramShapeToRightOfThisDiagramShape;

    private int rightSide;

    private int leftSide;

    private int topSide;

    /// <summary>Initialises a new instance of the <see cref="DiagramShape" /> class.</summary>
    /// <param name="visioId">Visio shape ID.</param>
    internal DiagramShape(int visioId)
    {
        this.logger = Log.ForContext<DiagramShape>();
        this.VisioId = visioId;
        this.Children = new SortedList<string, DiagramShape>(StringComparer.Ordinal);
        this.TopSide = ConvertMeasurement(AppConfig!.Height);
        this.LeftSide = 0;
        this.RightSide = ConvertMeasurement(AppConfig.Width);
        this.BaseSide = 0;
        this.Master = string.Empty;
        this.ShapeText = string.Empty;
        this.SortValue = string.Empty;
    }

    /// <summary>Noifty on shape resize.</summary>
    public event EventHandler? ShapeResize;

    /// <summary>Gets the stencil used for drawing shape.</summary>
    /// <value>Master shape stencil.</value>
    public string Master { get; init; }

    /// <summary>Gets parent shape of curent shape.</summary>
    /// <value>Parent shape.</value>
    public DiagramShape? ParentShape { get; private set; }

    /// <summary>Gets a unique shape identifier.</summary>
    /// <value>Unique identifer.</value>
    public string? ShapeIdentifier { get; init; }

    /// <summary>Gets the shape text.</summary>
    /// <value>Shape text.</value>
    public string ShapeText { get; init; }

    /// <summary>Gets or sets the shape type.</summary>
    /// <value>Shape type.</value>
    public ShapeType ShapeType { get; set; }

    /// <summary>Gets value used to sort shapes.</summary>
    /// <value>Sort value.</value>
    public string SortValue { get; init; }

    /// <summary>Gets or sets the shape above.</summary>
    /// <value>Shape above.</value>
    public DiagramShape? Above { get; set; }

    /// <summary>Gets or sets base of the shape.</summary>
    /// <value>Bottom of shape.</value>
    public int BaseSide
    {
        get => this.baseSide;
        set
        {
            this.baseSide = value;
            this.OnShapeResize();

            // move shape below
            int movement;
            if (this.DiagramShapeBelow is not null)
            {
                // calculate movement
                movement = this.DiagramShapeBelow.TopSide - (this.baseSide - ConvertMeasurement(AppConfig!.VerticalSpacing));

                if (movement != 0)
                {
                    this.logger.Debug(MovingShapeByMovementDirection, this.DiagramShapeBelow, movement, VerticalDirection);
                    this.DiagramShapeBelow.MoveVertical(movement);
                }
            }

            // move shape on right
            if (this.DiagramShapeRight is null)
            {
                return;
            }

            // calculate movement
            movement = this.DiagramShapeRight.TopSide - this.TopSide;
            if (movement == 0)
            {
                return;
            }

            this.logger.Debug(MovingShapeByMovementDirection, this.DiagramShapeRight, movement, VerticalDirection);
            this.DiagramShapeRight.MoveVertical(movement);
        }
    }

    /// <summary>Gets or sets the shape below.</summary>
    /// <value>Shape below.</value>
    public DiagramShape? DiagramShapeBelow
    {
        get => this.diagramShapeBelowThisDiagramShape;
        set
        {
            // remove existing relationship.
            if (this.diagramShapeBelowThisDiagramShape is not null)
            {
                this.diagramShapeBelowThisDiagramShape.Above = null;
            }

            // set value.
            this.diagramShapeBelowThisDiagramShape = value;

            if (this.diagramShapeBelowThisDiagramShape is null)
            {
                return;
            }

            // set relationship.
            this.diagramShapeBelowThisDiagramShape.Above = this;

            // calculate movement
            var movement = this.diagramShapeBelowThisDiagramShape.LeftSide - this.LeftSide;

            if (movement != 0)
            {
                this.logger.Debug(MovingShapeByMovementDirection, this.diagramShapeBelowThisDiagramShape, movement, HorizontalDirection);
                this.diagramShapeBelowThisDiagramShape.MoveHorizontal(movement);
            }

            // calculate movement
            movement = this.diagramShapeBelowThisDiagramShape.TopSide - (this.baseSide - ConvertMeasurement(AppConfig!.VerticalSpacing));

            if (movement == 0)
            {
                return;
            }

            this.logger.Debug(MovingShapeByMovementDirection, this.diagramShapeBelowThisDiagramShape, movement, VerticalDirection);
            this.diagramShapeBelowThisDiagramShape.MoveVertical(movement);
        }
    }

    /// <summary>Gets or sets the shape to the left.</summary>
    /// <value>Left shape.</value>
    public DiagramShape? Left { get; set; }

    /// <summary>Gets or sets left side of the shape.</summary>
    /// <value>Left side of shape.</value>
    public int LeftSide
    {
        get => this.leftSide;
        set
        {
            this.leftSide = value;
            this.OnShapeResize();
        }
    }

    /// <summary>Gets or sets the shape to the right.</summary>
    /// <value>Shape to right.</value>
    public DiagramShape? DiagramShapeRight
    {
        get => this.diagramShapeToRightOfThisDiagramShape;
        set
        {
            // remove existing relationship.
            if (this.diagramShapeToRightOfThisDiagramShape is not null)
            {
                this.diagramShapeToRightOfThisDiagramShape.Left = null;
            }

            // set value.
            this.diagramShapeToRightOfThisDiagramShape = value;

            if (this.diagramShapeToRightOfThisDiagramShape is null)
            {
                return;
            }

            // set relationship.
            this.diagramShapeToRightOfThisDiagramShape.Left = this;

            // calculate movement
            var movement = this.diagramShapeToRightOfThisDiagramShape.LeftSide - (this.rightSide + ConvertMeasurement(AppConfig!.HorizontalSpacing));

            if (movement != 0)
            {
                this.logger.Debug(MovingShapeByMovementDirection, this.diagramShapeToRightOfThisDiagramShape, movement, HorizontalDirection);
                this.diagramShapeToRightOfThisDiagramShape.MoveHorizontal(movement);
            }

            // calculate movement
            movement = this.diagramShapeToRightOfThisDiagramShape.TopSide - this.TopSide;
            if (movement == 0)
            {
                return;
            }

            this.logger.Debug(MovingShapeByMovementDirection, this.diagramShapeToRightOfThisDiagramShape, movement, VerticalDirection);
            this.diagramShapeToRightOfThisDiagramShape.MoveVertical(movement);
        }
    }

    /// <summary>Gets or sets <see cref="diagramShapeToRightOfThisDiagramShape" /> side of the shape.</summary>
    /// <value>Right side of shape.</value>
    public int RightSide
    {
        get => this.rightSide;
        set
        {
            this.rightSide = value;
            this.OnShapeResize();

            // move shape to right to spacing width
            int movement;
            if (this.DiagramShapeRight is not null)
            {
                // calculate movement
                movement = this.DiagramShapeRight.LeftSide - (this.rightSide + ConvertMeasurement(AppConfig!.HorizontalSpacing));

                if (movement != 0)
                {
                    this.logger.Debug(MovingShapeByMovementDirection, this.DiagramShapeRight, movement, HorizontalDirection);
                    this.DiagramShapeRight.MoveHorizontal(movement);
                }
            }

            // align shape below to left hand side.
            if (this.DiagramShapeBelow is null)
            {
                return;
            }

            // calculate movement
            movement = this.DiagramShapeBelow.LeftSide - this.LeftSide;

            if (movement == 0)
            {
                return;
            }

            this.logger.Debug(MovingShapeByMovementDirection, this.DiagramShapeBelow, movement, HorizontalDirection);
            this.DiagramShapeBelow.MoveHorizontal(movement);
        }
    }

    /// <summary>Gets or sets top of the shape.</summary>
    /// <value>Top side of shape.</value>
    public int TopSide
    {
        get => this.topSide;
        set
        {
            this.topSide = value;
            this.OnShapeResize();
        }
    }

    /// <summary>Gets or sets visio shape id.</summary>
    /// <value>Visio identifer.</value>
    public int VisioId { get; set; }

    internal static AppConfig? AppConfig { get; set; }

    /// <summary>Gets collection of child shapes.</summary>
    /// <value>child shapes.</value>
    internal SortedList<string, DiagramShape> Children { get; }

    /// <summary>Gets or sets how deep is the rendered children.</summary>
    /// <value>depth of children.</value>
    internal int ChildrenDepth { get; set; }

    /// <inheritdoc />
    public override string ToString() => string.Format(CultureInfo.CurrentCulture, "{0}: {1}", this.VisioId, this.ShapeText);

    /// <summary>Convert a visio <paramref name="measurement" /> into an easier mathematical model.</summary>
    /// <param name="measurement">Measurement from visio.</param>
    /// <returns>Easier <see langword="internal" /> measurement.</returns>
    internal static int ConvertMeasurement(double measurement) => (int)(Math.Round(measurement, ConversionDigits, MidpointRounding.AwayFromZero) * ConversionFactor);

    /// <summary>Convert an easier <paramref name="measurement" /> back to visio model.</summary>
    /// <param name="measurement">Easier <see langword="internal" /> measurement.</param>
    /// <returns>Measurement for visio.</returns>
    internal static double ConvertMeasurement(int measurement) => (double)measurement / ConversionFactor;

    /// <summary>Add child shape to parent.</summary>
    /// <param name="childShape">New child shape of this shape.</param>
    internal void AddChildShape(DiagramShape childShape)
    {
        if (childShape is null)
        {
            throw new ArgumentNullException(nameof(childShape));
        }

        if (this.Children.ContainsValue(childShape))
        {
            return;
        }

        childShape.ShapeResize += this.ChildShape_ShapeResize;

        // add to list of all children
        this.Children.Add(childShape.SortValue, childShape);

        // set parent shape
        childShape.ParentShape = this;
    }

    /// <summary>Correct shape and child shapes.</summary>
    /// <returns>If any shape was changed.</returns>
    internal bool CorrectDiagram()
    {
        var result = this.FixPosition();

        // depth first correction process
        foreach (var shape in this.Children.Values.Where(diagramShape => diagramShape.CorrectDiagram()))
        {
            result = true;
        }

        // resize shape
        if (this.ResizeShape())
        {
            result = true;
        }

        if (this.Children.Count <= 0)
        {
            return result;
        }

        // if shape to left is bigger
        if (this.Left is not null && (this.Left.Height() > this.Height()))
        {
            this.logger.Debug("Resizing: {Shape}", this);
            this.BaseSide = this.TopSide - this.Left.Height();
            this.logger.Debug("New size for {Shape}: {Corners}", this, this.CornerString());
            result = true;
        }

        // if share to right is bigger
        if (this.DiagramShapeRight is null)
        {
            return result;
        }

        if (this.DiagramShapeRight.Height() <= this.Height())
        {
            return result;
        }

        this.logger.Debug("Resizing: {Shape}", this);
        this.BaseSide = this.TopSide - this.DiagramShapeRight.Height();
        this.logger.Debug("New size for {Shape}: {Corners}", this, this.CornerString());
        return true;
    }

    /// <summary>Does this shape have a parent.</summary>
    /// <returns>True if a parent.</returns>
    internal bool HasParent() => this.ParentShape is not null;

    /// <summary>Calculate the height of the shape.</summary>
    /// <returns>Height.</returns>
    internal int Height() => this.TopSide - this.BaseSide;

    /// <summary>Resize the shape based on appconfig.</summary>
    /// <returns>If shape changed.</returns>
    internal bool ResizeShape()
    {
        int width;
        int height;
        if (this.Children.Count > 0)
        {
            var minLeftSide = this.Children.Values.Select(shape => shape.LeftSide).Min() - ConvertMeasurement(AppConfig!.Left);
            var maxRightSide = this.Children.Values.Select(shape => shape.RightSide).Max() + ConvertMeasurement(AppConfig.Right);
            width = maxRightSide - minLeftSide;

            var minBaseSide = this.Children.Values.Select(shape => shape.BaseSide).Min() - ConvertMeasurement(AppConfig.Base);
            var maxTopSide = this.Children.Values.Select(shape => shape.TopSide).Max() + ConvertMeasurement(AppConfig.Top);
            height = maxTopSide - minBaseSide;
        }
        else
        {
            height = ConvertMeasurement(AppConfig!.Height);
            width = ConvertMeasurement(AppConfig.Width);
        }

        var newBaseSide = this.TopSide - height;
        var newRightSide = this.LeftSide + width;

        if ((this.RightSide == newRightSide) && (this.BaseSide == newBaseSide))
        {
            return false;
        }

        this.logger.Debug("Resizing: {Shape}", this);
        this.RightSide = newRightSide;
        this.BaseSide = newBaseSide;
        this.logger.Debug("New size for {Shape}: {Corners}", this, this.CornerString());
        return true;
    }

    /// <summary>Calculate the width of the shape.</summary>
    /// <returns>Width.</returns>
    internal int Width() => this.RightSide - this.LeftSide;

    internal Matrix<float> Bitmap()
    {
        var bitmap = Matrix<float>.Build.Sparse(this.TotalChildrenCount(), this.TotalChildrenCount(), 0);

        if (this.Children.Count == 0)
        {
            return Matrix<float>.Build.Dense(1, 1, 0);
        }

        var rowCount = 0;
        var rowChild = this.Children.Values.First(child => child.Left is null && child.Above is null);

        while (rowChild is not null)
        {
            var columnCount = 0;
            var columnChild = rowChild;

            while (columnChild is not null)
            {
                var childBitmap = columnChild.Bitmap().Add(1);

                var existingSpace = bitmap.SubMatrix(rowCount, childBitmap.RowCount, columnCount, childBitmap.ColumnCount);

                // space is empty!
                if (existingSpace.ColumnSums().Sum() <= 0)
                {
                    bitmap.SetSubMatrix(rowCount, columnCount, childBitmap);
                    columnCount += childBitmap.ColumnCount;
                }
                else
                {
                    // where do we put it?
                    rowCount++;
                    continue;
                }

                columnChild = columnChild.DiagramShapeRight;
            }

            rowChild = rowChild.DiagramShapeBelow;
            rowCount++;
        }

        // remove empty rows & columns
        var columns = bitmap.EnumerateColumns().Count(values => values.Sum() > 0);
        var rows = bitmap.EnumerateRows().Count(values => values.Sum() > 0);

        return bitmap.SubMatrix(0, rows, 0, columns);
    }

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
        foreach (var child in children.Values)
        {
            child.DiagramShapeRight = null;
            child.DiagramShapeBelow = null;
        }

        var tolerance = ((AppConfig!.HorizontalSpacing + AppConfig!.VerticalSpacing) * ConversionFactor) / 2d;

        var lines = children.Values.OrderBy(shape => shape.LeftSide).Select(shape => shape.LeftSide);
        foreach (var line in lines.Distinct())
        {
            bool AbsoluteShapeSize(DiagramShape shape)
            {
                var side = shape.LeftSide - line;
                return Math.Abs(side) < tolerance;
            }

            var diagramShapes = children.Values.Where(AbsoluteShapeSize);
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
                    case not null when (shape.BaseSide - currentShape.TopSide) < (tolerance + tolerance):
                        shape.DiagramShapeBelow = currentShape;

                        currentShape = shape;
                        break;
                    default:
                        currentShape = shape;
                        break;
                }
            }
        }

        lines = children.Values.OrderBy(shape => shape.TopSide).Select(shape => shape.TopSide);
        foreach (var line in lines.Distinct())
        {
            bool AbsoluteShapeSize(DiagramShape shape)
            {
                var side = shape.TopSide - line;
                return Math.Abs(side) < tolerance;
            }

            var diagramShapes = children.Values.Where(AbsoluteShapeSize);
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
                    case not null when (shape.LeftSide - currentShape.RightSide) < (tolerance + tolerance):
                        currentShape.DiagramShapeRight = shape;

                        currentShape = shape;
                        break;
                    default:
                        currentShape = shape;
                        break;
                }
            }
        }
    }

    internal int TotalChildrenCount() => !this.Children.Any() ? 1 : 1 + this.Children.Values.Sum(child => child.TotalChildrenCount());

    /// <summary>Notify of a shape resize.</summary>
    protected virtual void OnShapeResize()
    {
        var handler = this.ShapeResize;
        handler?.Invoke(this, EventArgs.Empty);
    }

    private void ChildShape_ShapeResize(object? sender, EventArgs e) => this.logger.Debug("Child shape was resized!");

    private string CornerString()
    {
        var culture = CultureInfo.CurrentCulture;
        return string.Format(
            culture,
            CornerStringFormat,
            ConvertMeasurement(this.TopSide),
            ConvertMeasurement(this.LeftSide),
            ConvertMeasurement(this.Width()),
            ConvertMeasurement(this.Height()));
    }

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
                result = true;
            }

            if (leftMovement != 0)
            {
                this.logger.Debug("Aligning {Shape} to {Parent}", this, this.ParentShape);
                this.MoveHorizontal(leftMovement);
                result = true;
            }
        }

        // move shape to right to spacing width
        int movement;
        if (this.DiagramShapeRight is not null)
        {
            // calculate movement
            movement = this.DiagramShapeRight.LeftSide - (this.rightSide + ConvertMeasurement(AppConfig!.HorizontalSpacing));

            if (movement != 0)
            {
                this.logger.Debug(MovingShapeByMovementDirection, this.DiagramShapeRight, movement, HorizontalDirection);
                this.DiagramShapeRight.MoveHorizontal(movement);
                result = true;
            }

            // calculate movement
            movement = this.DiagramShapeRight.TopSide - this.TopSide;
            if (movement != 0)
            {
                this.logger.Debug(MovingShapeByMovementDirection, this.DiagramShapeRight, movement, VerticalDirection);
                this.DiagramShapeRight.MoveVertical(movement);
                result = true;
            }
        }

        // align shape below to left hand side.
        if (this.DiagramShapeBelow is not null)
        {
            movement = this.DiagramShapeBelow.LeftSide - this.LeftSide;

            if (movement != 0)
            {
                this.logger.Debug(MovingShapeByMovementDirection, this.DiagramShapeBelow, movement, HorizontalDirection);
                this.DiagramShapeBelow.MoveHorizontal(movement);
                result = true;
            }

            // calculate movement
            movement = this.DiagramShapeBelow.TopSide - (this.baseSide - ConvertMeasurement(AppConfig!.VerticalSpacing));

            if (movement == 0)
            {
                return result;
            }

            this.logger.Debug(MovingShapeByMovementDirection, this.DiagramShapeBelow, movement, VerticalDirection);
            this.DiagramShapeBelow.MoveVertical(movement);
            return true;
        }

        // calculate movement
        return result;
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
