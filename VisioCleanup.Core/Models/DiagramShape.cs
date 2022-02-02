// -----------------------------------------------------------------------
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

    private int height;

    private int positionX;

    private int positionY;

    private DiagramShape? shapeBelow;

    private DiagramShape? shapeRight;

    private int width;

    /// <summary>Initialises a new instance of the <see cref="DiagramShape" /> class.</summary>
    /// <param name="visioId">Visio shape ID.</param>
    internal DiagramShape(int visioId)
    {
        this.logger = Log.ForContext<DiagramShape>();
        this.VisioId = visioId;
        this.Children = new SortedList<string, DiagramShape>(StringComparer.Ordinal);
        this.PositionY = 0;
        this.PositionX = 0;
        this.Width = ConvertMeasurement(AppConfig!.Width);
        this.Height = ConvertMeasurement(AppConfig!.Height);
        this.Master = string.Empty;
        this.ShapeText = string.Empty;
        this.SortValue = string.Empty;
        this.Matrix = new List<List<DiagramShape>> { new() };
    }

    /// <summary>Noifty on shape resize.</summary>
    public event EventHandler? ShapeChanged;

    /// <summary>Gets or sets the height of the shape.</summary>
    /// <value>Height of the shape in visio units.</value>
    public int Height
    {
        get => this.height;
        set
        {
            if (this.height == value)
            {
                return;
            }

            this.height = value;
            this.OnShapeChange();

            // move shape below
            int movement;
            if (this.ShapeBelow is not null)
            {
                var shapeSize = this.PositionY - this.Height - ConvertMeasurement(AppConfig!.VerticalSpacing);

                // calculate movement
                movement = this.ShapeBelow.PositionY - shapeSize;

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

    /// <summary>Gets or sets the theoretical maximum right side of for this shape.</summary>
    /// <value> The theoretical maximum right side of for this shape. </value>
    public int MaxRight { get; set; }

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
            this.OnShapeChange();
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
            this.OnShapeChange();
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
            var shapeSize = this.PositionY - this.Height - ConvertMeasurement(AppConfig!.VerticalSpacing);
            movement = this.shapeBelow.PositionY - shapeSize;

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
            var shapeSize = this.PositionX + this.Width + ConvertMeasurement(AppConfig!.HorizontalSpacing);
            var movement = this.shapeRight.PositionX - shapeSize;

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

    /// <summary>Gets or sets the width of the shape.</summary>
    /// <value>Width.</value>
    public int Width
    {
        get => this.width;
        set
        {
            if (this.width == value)
            {
                return;
            }

            this.width = value;
            this.OnShapeChange();

            // move shape to right to spacing width
            int movement;
            if (this.ShapeRight is not null)
            {
                // calculate movement
                var shapeSize = this.PositionX + this.Width + ConvertMeasurement(AppConfig!.HorizontalSpacing);

                movement = this.ShapeRight.PositionX - shapeSize;

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

    internal static AppConfig? AppConfig { get; set; }

    /// <summary>Gets collection of child shapes.</summary>
    /// <value>child shapes.</value>
    internal SortedList<string, DiagramShape> Children { get; }

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

        childShape.ShapeChanged += this.ChildShapeShapeChanged;

        // add to list of all children
        this.Children.Add(childShape.SortValue, childShape);

        // set parent shape
        childShape.ParentShape = this;

        // if new shape
        if (childShape.ShapeType == ShapeType.NewShape)
        {
            // set max right
            childShape.MaxRight = this.MaxRight - this.GetInternalMargin(Side.Right);
        }
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
    /// <param name="matchLine"></param>
    /// <returns>If any shape was changed.</returns>
    internal bool CorrectDiagram(bool matchLine = true)
    {
        var result = this.FixPosition();

        // depth first correction process
        foreach (var diagramShape in this.Children.Values.Where(diagramShape => diagramShape.CorrectDiagram(matchLine)))
        {
            result = true;
        }

        // resize shape
        if (this.ResizeShape(matchLine: matchLine))
        {
            result = true;
        }

        return result;
    }

    internal int FindMaxHeightOnLine(int newHeight)
    {
        var maxHeight = newHeight;

        // goto left
        var shape = this.ShapeLeft;
        while (shape is not null)
        {
            if (maxHeight <= shape.Height)
            {
                maxHeight = shape.Height;
            }

            shape = shape.ShapeLeft;
        }

        // goto right
        shape = this.ShapeRight;
        while (shape is not null)
        {
            if (maxHeight <= shape.Height)
            {
                maxHeight = shape.Height;
            }

            shape = shape.ShapeRight;
        }

        return maxHeight;
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
            var topOrdered = diagramShapes.OrderBy(shape => shape.PositionY).ToList();
            DiagramShape? currentShape = null;

            foreach (var shape in topOrdered)
            {
                switch (currentShape)
                {
                    case not null when currentShape.PositionY.Equals(shape.PositionY):
                        // overlap!
                        this.logger.Error("Weird circumstances, check diagram for overlaps between {Shape} and {Shape2}", currentShape, shape);
                        continue;
                    case not null when currentShape.PositionY >= shape.PositionY:
                        continue;
                    case not null when (shape.PositionY - shape.Height - currentShape.PositionY) < (tolerance + tolerance):
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
                    case not null when (shape.PositionX - (currentShape.PositionX + currentShape.Width)) < (tolerance + tolerance):
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

    internal int GetInternalMargin(Side side)
    {
        if (this.ShapeType == ShapeType.FakeShape)
        {
            return 0;
        }

        return side switch
        {
            Side.Left => ConvertMeasurement(AppConfig!.Left),
            Side.Right => ConvertMeasurement(AppConfig!.Right),
            Side.Top => ConvertMeasurement(AppConfig!.Top),
            Side.Base => ConvertMeasurement(AppConfig!.Base),
            _ => 0,
        };
    }

    /// <summary>Does this shape have a parent.</summary>
    /// <returns>True if a parent.</returns>
    internal bool HasParent() => this.ParentShape is not null;

    /// <summary>Resize the shape based on appconfig.</summary>
    /// <param name="matchLine"></param>
    /// <returns>If shape changed.</returns>
    internal bool ResizeShape(bool matchLine)
    {
        int newWidth;
        int newHeight;
        if (this.Children.Count > 0)
        {
            // default values
            var childShapes = this.Children.Values;

            var minLeftSide = childShapes.Min(shape => shape.PositionX) - this.GetInternalMargin(Side.Left);
            var maxRightSide = childShapes.Max(shape => shape.PositionX + shape.Width) + this.GetInternalMargin(Side.Right);
            newWidth = maxRightSide - minLeftSide;

            var minBaseSide = childShapes.Min(shape => shape.PositionY - shape.Height) - this.GetInternalMargin(Side.Base);
            var maxTopSide = childShapes.Max(shape => shape.PositionY) + this.GetInternalMargin(Side.Top);
            newHeight = maxTopSide - minBaseSide;

            if (matchLine)
            {
                var maxHeight = this.FindMaxHeightOnLine(newHeight);
                if (maxHeight > newHeight)
                {
                    newHeight = maxHeight;
                }
            }
        }
        else
        {
            newHeight = ConvertMeasurement(AppConfig!.Height);
            newWidth = ConvertMeasurement(AppConfig.Width);
        }

        if ((this.Width == newWidth) && (this.Height == newHeight))
        {
            return false;
        }

        // don't change width of fakeshape
        this.logger.Debug("Resizing: {Shape}", this);
        if (this.ShapeType != ShapeType.FakeShape)
        {
            this.Width = newWidth;
        }

        this.Height = newHeight;
        this.logger.Debug("New size for {Shape}: {Corners}", this, this.CornerString());
        return true;
    }

    /// <summary>Sort the children of the diagram shape.</summary>
    internal void SortChildren()
    {
        var children = this.Children.Values;

        foreach (var child in children.Where(child => child.Children.Count > 0))
        {
            child.SortChildren();
        }

        var maxShapesPerLine = this.CalculateMaxShapesPerLine();
        this.ClearExistingRelationships();
        Queue<DiagramShape> childrenQueue = new(children);

        while (childrenQueue.Count > 0)
        {
            // what is the current line we're adding to.
            var currentLineNumber = this.Matrix.Count - 1;
            var currentLine = this.Matrix[currentLineNumber];

            if (currentLine.Count == 0)
            {
                if (currentLineNumber > 0)
                {
                    var shapeAbove = this.Matrix[currentLineNumber - 1][0];
                    shapeAbove.ShapeBelow = childrenQueue.Peek();
                    this.AddShapeToLine(currentLine, childrenQueue.Dequeue());
                    continue;
                }

                this.AddShapeToLine(currentLine, childrenQueue.Dequeue());
                continue;
            }

            // if we're at maxline, then add new line and loop again.
            if (currentLine.Count >= maxShapesPerLine)
            {
                this.Matrix.Add(new List<DiagramShape>());
                continue;
            }

            // find current width
            var lineWidth = currentLine.Max(shape => shape.PositionX + shape.Width);
            var newlineWidth = lineWidth + childrenQueue.Peek().Width + ConvertMeasurement(AppConfig!.HorizontalSpacing);

            // we can't fit shape on line
            if (newlineWidth >= childrenQueue.Peek().MaxRight)
            {
                this.Matrix.Add(new List<DiagramShape>());
                continue;
            }

            var previousShape = currentLine[^1];
            previousShape.ShapeRight = childrenQueue.Peek();
            this.AddShapeToLine(currentLine, childrenQueue.Dequeue());
        }

        this.FindNeighbours();
        this.CorrectDiagram();
    }

    internal int TotalChildrenCount() => !this.Children.Any() ? 1 : 1 + this.Children.Values.Sum(child => child.TotalChildrenCount());

    /// <summary>Notify of a shape resize.</summary>
    protected virtual void OnShapeChange()
    {
        var handler = this.ShapeChanged;
        handler?.Invoke(this, EventArgs.Empty);
    }

    private void AddShapeToLine(ICollection<DiagramShape> currentLine, DiagramShape childShape)
    {
        currentLine.Add(childShape);
        this.CorrectDiagram();
    }

    private int CalculateMaxShapesPerLine()
    {
        var defaultMaxBoxLines = (int)(AppConfig!.MaxBoxLines ?? 5d);
        var childrenCount = this.Children.Count;
        if (childrenCount != (this.TotalChildrenCount() - 1))
        {
            return int.MaxValue;
        }

        var temp = Math.Round(Math.Sqrt(childrenCount), MidpointRounding.AwayFromZero);
        var drawLines = childrenCount / temp;
        if (drawLines > defaultMaxBoxLines)
        {
            return childrenCount / defaultMaxBoxLines;
        }

        return (int)temp;
    }

    private void ChildShapeShapeChanged(object? sender, EventArgs e)
    {
        this.logger.Debug("Child shape was resized!");
        this.ResizeShape(matchLine: false);
    }

    private void ClearExistingRelationships()
    {
        // clear existing relationships.
        foreach (var child in this.Children.Values)
        {
            child.ShapeRight = null;
            child.ShapeBelow = null;
        }

        this.Matrix = new List<List<DiagramShape>> { new() };
    }

    private string CornerString()
    {
        var culture = CultureInfo.CurrentCulture;
        return string.Format(
            culture,
            CornerStringFormat,
            ConvertMeasurement(this.PositionY),
            ConvertMeasurement(this.PositionX),
            ConvertMeasurement(this.Width),
            ConvertMeasurement(this.Height));
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
            var newLeft = this.ParentShape.PositionX + this.ParentShape.GetInternalMargin(Side.Left);
            var newTop = this.ParentShape.PositionY - this.ParentShape.GetInternalMargin(Side.Top);

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
            var shapeSize = this.PositionX + this.Width + ConvertMeasurement(AppConfig!.HorizontalSpacing);

            movement = this.ShapeRight.PositionX - shapeSize;

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
            var shapeSize = this.PositionY - this.Height - ConvertMeasurement(AppConfig!.VerticalSpacing);
            movement = this.ShapeBelow.PositionY - shapeSize;

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

    private void MoveHorizontal(int movement) => this.PositionX -= movement;

    private void MoveVertical(int movement) => this.PositionY -= movement;
}
