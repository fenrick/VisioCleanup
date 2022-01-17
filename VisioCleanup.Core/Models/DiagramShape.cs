﻿// -----------------------------------------------------------------------
// <copyright file="DiagramShape.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Models;

using System.Globalization;

using MathNet.Numerics.LinearAlgebra;

using Serilog;

using VisioCleanup.Core.Models.Config;

/// <summary>Representation of a single shape in a visio diagram.</summary>
public class DiagramShape
{
    private const int ConversionDigits = 3;

    private const int ConversionFactor = 1000;

    private const string CornerStringFormat = "Top: {0}, Left: {1}, Width: {2}, Height: {3}";

    private const string HorizontalDirection = "horizontal";

    private const string MovingShapeByMovementDirection = "Moving {Shape} by {Movement} {Direction}";

    private const string VerticalDirection = "vertical";

    private readonly ILogger logger;

    private int baseSide;

    private int positionX;

    private int positionY;

    private int rightSide;

    private DiagramShape? shapeBelow;

    private DiagramShape? shapeRight;

    /// <summary>Initialises a new instance of the <see cref="DiagramShape" /> class.</summary>
    /// <param name="visioId">Visio shape ID.</param>
    internal DiagramShape(int visioId)
    {
        this.logger = Log.ForContext<DiagramShape>();
        this.VisioId = visioId;
        this.Children = new SortedList<string, DiagramShape>(StringComparer.Ordinal);
        this.PositionY = ConvertMeasurement(AppConfig!.Height);
        this.PositionX = 0;
        this.RightSide = ConvertMeasurement(AppConfig.Width);
        this.BaseSide = 0;
        this.Master = string.Empty;
        this.ShapeText = string.Empty;
        this.SortValue = string.Empty;
        this.Matrix = new List<List<DiagramShape>> { new() };
    }

    /// <summary>Noifty on shape resize.</summary>
    public event EventHandler? ShapeResized;

    /// <summary>Gets or sets base of the shape.</summary>
    /// <value>Bottom of shape.</value>
    public int BaseSide
    {
        get => this.baseSide;
        set
        {
            if (this.baseSide == value)
            {
                return;
            }

            this.baseSide = value;
            this.OnShapeResize();

            // move shape below
            int movement;
            if (this.ShapeBelow is not null)
            {
                // calculate movement
                movement = this.ShapeBelow.PositionY - (this.baseSide - ConvertMeasurement(AppConfig!.VerticalSpacing));

                if (movement != 0)
                {
                    this.logger.Debug(MovingShapeByMovementDirection, this.ShapeBelow, movement, VerticalDirection);
                    this.ShapeBelow.MoveVertical(movement);
                }
            }

            // move shape on right
            if (this.ShapeRight is null)
            {
                return;
            }

            // calculate movement
            movement = this.ShapeRight.PositionY - this.PositionY;
            if (movement == 0)
            {
                return;
            }

            this.logger.Debug(MovingShapeByMovementDirection, this.ShapeRight, movement, VerticalDirection);
            this.ShapeRight.MoveVertical(movement);
        }
    }

    /// <summary>Gets the stencil used for drawing shape.</summary>
    /// <value>Master shape stencil.</value>
    public string Master { get; init; }

    /// <summary>Gets parent shape of curent shape.</summary>
    /// <value>Parent shape.</value>
    public DiagramShape? ParentShape { get; private set; }

    /// <summary>Gets or sets left side of the shape.</summary>
    /// <value>Left side of shape.</value>
    public int PositionX
    {
        get => this.positionX;
        set
        {
            if (this.positionX == value)
            {
                return;
            }

            this.positionX = value;
            this.OnShapeResize();
        }
    }

    /// <summary>Gets or sets top of the shape.</summary>
    /// <value>Top side of shape.</value>
    public int PositionY
    {
        get => this.positionY;
        set
        {
            if (this.positionY == value)
            {
                return;
            }

            this.positionY = value;
            this.OnShapeResize();
        }
    }

    /// <summary>Gets or sets <see cref="shapeRight" /> side of the shape.</summary>
    /// <value>Right side of shape.</value>
    public int RightSide
    {
        get => this.rightSide;
        set
        {
            if (this.rightSide == value)
            {
                return;
            }

            this.rightSide = value;
            this.OnShapeResize();

            // move shape to right to spacing width
            int movement;
            if (this.ShapeRight is not null)
            {
                // calculate movement
                movement = this.ShapeRight.PositionX - (this.rightSide + ConvertMeasurement(AppConfig!.HorizontalSpacing));

                if (movement != 0)
                {
                    this.logger.Debug(MovingShapeByMovementDirection, this.ShapeRight, movement, HorizontalDirection);
                    this.ShapeRight.MoveHorizontal(movement);
                }
            }

            // align shape below to left hand side.
            if (this.ShapeBelow is null)
            {
                return;
            }

            // calculate movement
            movement = this.ShapeBelow.PositionX - this.PositionX;

            if (movement == 0)
            {
                return;
            }

            this.logger.Debug(MovingShapeByMovementDirection, this.ShapeBelow, movement, HorizontalDirection);
            this.ShapeBelow.MoveHorizontal(movement);
        }
    }

    /// <summary>Gets or sets the shape above.</summary>
    /// <value>Shape above.</value>
    public DiagramShape? ShapeAbove { get; set; }

    /// <summary>Gets or sets the shape below.</summary>
    /// <value>Shape below.</value>
    public DiagramShape? ShapeBelow
    {
        get => this.shapeBelow;
        set
        {
            // remove existing relationship.
            if (this.shapeBelow is not null)
            {
                this.shapeBelow.ShapeAbove = null;
            }

            // set value.
            this.shapeBelow = value;

            if (this.shapeBelow is null)
            {
                return;
            }

            // set relationship.
            this.shapeBelow.ShapeAbove = this;

            // calculate movement
            var movement = this.shapeBelow.PositionX - this.PositionX;

            if (movement != 0)
            {
                this.logger.Debug(MovingShapeByMovementDirection, this.shapeBelow, movement, HorizontalDirection);
                this.shapeBelow.MoveHorizontal(movement);
            }

            // calculate movement
            movement = this.shapeBelow.PositionY - (this.baseSide - ConvertMeasurement(AppConfig!.VerticalSpacing));

            if (movement == 0)
            {
                return;
            }

            this.logger.Debug(MovingShapeByMovementDirection, this.shapeBelow, movement, VerticalDirection);
            this.shapeBelow.MoveVertical(movement);
        }
    }

    /// <summary>Gets a unique shape identifier.</summary>
    /// <value>Unique identifer.</value>
    public string? ShapeIdentifier { get; init; }

    /// <summary>Gets or sets the shape to the left.</summary>
    /// <value>Left shape.</value>
    public DiagramShape? ShapeLeft { get; set; }

    /// <summary>Gets or sets the shape to the right.</summary>
    /// <value>Shape to right.</value>
    public DiagramShape? ShapeRight
    {
        get => this.shapeRight;
        set
        {
            // remove existing relationship.
            if (this.shapeRight is not null)
            {
                this.shapeRight.ShapeLeft = null;
            }

            // set value.
            this.shapeRight = value;

            if (this.shapeRight is null)
            {
                return;
            }

            // set relationship.
            this.shapeRight.ShapeLeft = this;

            // calculate movement
            var movement = this.shapeRight.PositionX - (this.rightSide + ConvertMeasurement(AppConfig!.HorizontalSpacing));

            if (movement != 0)
            {
                this.logger.Debug(MovingShapeByMovementDirection, this.shapeRight, movement, HorizontalDirection);
                this.shapeRight.MoveHorizontal(movement);
            }

            // calculate movement
            movement = this.shapeRight.PositionY - this.PositionY;
            if (movement == 0)
            {
                return;
            }

            this.logger.Debug(MovingShapeByMovementDirection, this.shapeRight, movement, VerticalDirection);
            this.shapeRight.MoveVertical(movement);
        }
    }

    /// <summary>Gets the shape text.</summary>
    /// <value>Shape text.</value>
    public string ShapeText { get; init; }

    /// <summary>Gets or sets the shape type.</summary>
    /// <value>Shape type.</value>
    public ShapeType ShapeType { get; set; }

    /// <summary>Gets value used to sort shapes.</summary>
    /// <value>Sort value.</value>
    public string SortValue { get; init; }

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

    internal List<List<DiagramShape>> Matrix { get; set; }

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

        childShape.ShapeResized += this.ChildShapeShapeResized;

        // add to list of all children
        this.Children.Add(childShape.SortValue, childShape);

        // set parent shape
        childShape.ParentShape = this;
    }

    internal Matrix<float> Bitmap()
    {
        var bitmap = Matrix<float>.Build.Sparse(this.TotalChildrenCount(), this.TotalChildrenCount(), 0);

        if (this.Children.Count == 0)
        {
            return Matrix<float>.Build.Dense(1, 1, 0);
        }

        var rowCount = 0;
        var rowChild = this.Children.Values.First(child => child.ShapeLeft is null && child.ShapeAbove is null);

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

                columnChild = columnChild.ShapeRight;
            }

            rowChild = rowChild.ShapeBelow;
            rowCount++;
        }

        // remove empty rows & columns
        var columns = bitmap.EnumerateColumns().Count(values => values.Sum() > 0);
        var rows = bitmap.EnumerateRows().Count(values => values.Sum() > 0);

        return bitmap.SubMatrix(0, rows, 0, columns);
    }

    /// <summary>Correct shape and child shapes.</summary>
    /// <returns>If any shape was changed.</returns>
    internal bool CorrectDiagram()
    {
        var result = this.FixPosition();

        // depth first correction process
        foreach (var diagramShape in this.Children.Values.Where(diagramShape => diagramShape.CorrectDiagram()))
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
            child.ShapeRight = null;
            child.ShapeBelow = null;
        }

        var tolerance = ((AppConfig!.HorizontalSpacing + AppConfig.VerticalSpacing) * ConversionFactor) / 2d;

        var lines = children.Values.OrderBy(shape => shape.PositionX).Select(shape => shape.PositionX);
        foreach (var line in lines.Distinct())
        {
            bool AbsoluteShapeSize(DiagramShape shape)
            {
                var side = shape.PositionX - line;
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
                    case not null when (shape.BaseSide - currentShape.PositionY) < (tolerance + tolerance):
                        shape.ShapeBelow = currentShape;

                        currentShape = shape;
                        break;
                    default:
                        currentShape = shape;
                        break;
                }
            }
        }

        lines = children.Values.OrderBy(shape => shape.PositionY).Select(shape => shape.PositionY);
        foreach (var line in lines.Distinct())
        {
            bool AbsoluteShapeSize(DiagramShape shape)
            {
                var side = shape.PositionY - line;
                return Math.Abs(side) < tolerance;
            }

            var diagramShapes = children.Values.Where(AbsoluteShapeSize);
            var bottomOrdered = diagramShapes.OrderBy(shape => shape.PositionX);
            DiagramShape? currentShape = null;

            foreach (var shape in bottomOrdered)
            {
                switch (currentShape)
                {
                    case not null when currentShape.PositionX.Equals(shape.PositionX):
                        // overlap!
                        this.logger.Error("Weird circumstances, check diagram for overlaps between {Shape} and {Shape2}", currentShape, shape);
                        continue;
                    case not null when currentShape.PositionX >= shape.PositionX:
                        continue;
                    case not null when (shape.PositionX - currentShape.RightSide) < (tolerance + tolerance):
                        currentShape.ShapeRight = shape;

                        currentShape = shape;
                        break;
                    default:
                        currentShape = shape;
                        break;
                }
            }
        }
    }

    /// <summary>Does this shape have a parent.</summary>
    /// <returns>True if a parent.</returns>
    internal bool HasParent() => this.ParentShape is not null;

    /// <summary>Calculate the height of the shape.</summary>
    /// <returns>Height.</returns>
    internal int Height() => this.PositionY - this.BaseSide;

    /// <summary>Resize the shape based on appconfig.</summary>
    /// <returns>If shape changed.</returns>
    internal bool ResizeShape()
    {
        int width;
        int height;
        if (this.Children.Count > 0)
        {
            // default values
            var childShapes = this.Children.Values;

            var minLeftSide = childShapes.Min(shape => shape.PositionX) - ConvertMeasurement(AppConfig!.Left);
            var maxRightSide = childShapes.Max(shape => shape.RightSide) + ConvertMeasurement(AppConfig.Right);
            width = maxRightSide - minLeftSide;

            var minBaseSide = childShapes.Min(shape => shape.BaseSide) - ConvertMeasurement(AppConfig.Base);
            var maxTopSide = childShapes.Max(shape => shape.PositionY) + ConvertMeasurement(AppConfig.Top);
            height = maxTopSide - minBaseSide;

            // compare to left
            if (this.ShapeLeft?.Height() > height)
            {
                height = this.ShapeLeft.Height();
            }

            // compare to right
            if (this.ShapeRight?.Height() > height)
            {
                height = this.ShapeRight.Height();
            }
        }
        else
        {
            height = ConvertMeasurement(AppConfig!.Height);
            width = ConvertMeasurement(AppConfig.Width);
        }

        var newBaseSide = this.PositionY - height;
        var newRightSide = this.PositionX + width;

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

    internal int TotalChildrenCount() => !this.Children.Any() ? 1 : 1 + this.Children.Values.Sum(child => child.TotalChildrenCount());

    /// <summary>Calculate the width of the shape.</summary>
    /// <returns>Width.</returns>
    internal int Width() => this.RightSide - this.PositionX;

    /// <summary>Notify of a shape resize.</summary>
    protected virtual void OnShapeResize()
    {
        var handler = this.ShapeResized;
        handler?.Invoke(this, EventArgs.Empty);
    }

    private void ChildShapeShapeResized(object? sender, EventArgs e)
    {
        this.logger.Debug("Child shape was resized!");
        this.ResizeShape();
    }

    private string CornerString()
    {
        var culture = CultureInfo.CurrentCulture;
        return string.Format(
            culture,
            CornerStringFormat,
            ConvertMeasurement(this.PositionY),
            ConvertMeasurement(this.PositionX),
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
        if (this.ShapeAbove is null && this.ShapeLeft is null)
        {
            var newLeft = this.ParentShape.PositionX + ConvertMeasurement(AppConfig!.Left);
            var newTop = this.ParentShape.PositionY - ConvertMeasurement(AppConfig.Top);

            var topMovement = this.PositionY - newTop;

            var leftMovement = this.PositionX - newLeft;

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
        if (this.ShapeRight is not null)
        {
            // calculate movement
            movement = this.ShapeRight.PositionX - (this.rightSide + ConvertMeasurement(AppConfig!.HorizontalSpacing));

            if (movement != 0)
            {
                this.logger.Debug(MovingShapeByMovementDirection, this.ShapeRight, movement, HorizontalDirection);
                this.ShapeRight.MoveHorizontal(movement);
                result = true;
            }

            // calculate movement
            movement = this.ShapeRight.PositionY - this.PositionY;
            if (movement != 0)
            {
                this.logger.Debug(MovingShapeByMovementDirection, this.ShapeRight, movement, VerticalDirection);
                this.ShapeRight.MoveVertical(movement);
                result = true;
            }
        }

        // align shape below to left hand side.
        if (this.ShapeBelow is not null)
        {
            movement = this.ShapeBelow.PositionX - this.PositionX;

            if (movement != 0)
            {
                this.logger.Debug(MovingShapeByMovementDirection, this.ShapeBelow, movement, HorizontalDirection);
                this.ShapeBelow.MoveHorizontal(movement);
                result = true;
            }

            // calculate movement
            movement = this.ShapeBelow.PositionY - (this.baseSide - ConvertMeasurement(AppConfig!.VerticalSpacing));

            if (movement == 0)
            {
                return result;
            }

            this.logger.Debug(MovingShapeByMovementDirection, this.ShapeBelow, movement, VerticalDirection);
            this.ShapeBelow.MoveVertical(movement);
            return true;
        }

        // calculate movement
        return result;
    }

    private void MoveHorizontal(int movement)
    {
        this.PositionX -= movement;
        this.RightSide -= movement;
    }

    private void MoveVertical(int movement)
    {
        this.PositionY -= movement;
        this.BaseSide -= movement;
    }
}
