// -----------------------------------------------------------------------
// <copyright file="VisioCleanupService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using VisioCleanup.Objects;

    /// <summary>
    ///     Background service that runs engine.
    /// </summary>
    internal class VisioCleanupService : BackgroundService
    {
        private readonly IHostApplicationLifetime appLifetime;

        private readonly IExcelHandler excelHandler;

        private readonly ILogger<VisioCleanupService> logger;

        private readonly VisioCleanupSettings settings;

        private readonly IVisioHandler visioHandler;

        /// <summary>
        ///     Initialises a new instance of the <see cref="VisioCleanupService" /> class.
        /// </summary>
        /// <param name="settings">External settings.</param>
        /// <param name="logger">Logging.</param>
        /// <param name="visioHandler">Visio Handler.</param>
        /// <param name="excelHandler">Excel Handler.</param>
        /// <param name="appLifetime">Host application lifetime.</param>
        public VisioCleanupService(
            IOptions<VisioCleanupSettings> settings,
            ILogger<VisioCleanupService> logger,
            IVisioHandler visioHandler,
            IExcelHandler excelHandler,
            IHostApplicationLifetime appLifetime)
        {
            this.settings = settings.Value;
            this.logger = logger;
            this.visioHandler = visioHandler;
            this.excelHandler = excelHandler;
            this.appLifetime = appLifetime;
        }

        /// <inheritdoc />
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            // clean-up tasks!
            this.logger.LogDebug(@"Cleaning up visio handler");
            this.visioHandler.Close();

            return base.StopAsync(stoppingToken);
        }

        /// <inheritdoc />
        /// <exception cref="T:System.Threading.Tasks.TaskCanceledException">The task has been cancelled.</exception>
        /// \
        /// TODO: Needs refactoring
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("Starting Visio Cleanup Service");

            stoppingToken.Register(() => { this.logger.LogInformation("Stopping Visio Cleanup Service."); });

            await Task.Run(
                () =>
                    {
                        try
                        {
                            // open
                            this.visioHandler.Open();

                            var selection = this.visioHandler.Selection();

                            DiagramShape parentShape;
                            if (selection.Length >= 1)
                            {
                                // create a fake master.
                                var page = this.visioHandler.GetPageSize(15, 0);
                                this.logger.LogInformation("Creating a fake master for selection.");
                                parentShape = new DiagramShape(0) { ShapeText = "FAKE", Corners = page, ShapeType = ShapeType.FakeShape };

                                this.logger.LogInformation("Adding selection as children");

                                foreach (var childId in selection)
                                {
                                    // add to diagramShape
                                    var childShape = new DiagramShape(childId)
                                                         {
                                                             ShapeText = this.visioHandler.GetShapeText(childId),
                                                             Corners = this.visioHandler.CalculateCorners(childId),
                                                             ShapeType = ShapeType.Existing,
                                                         };
                                    parentShape.AddChildShape(childShape);

                                    this.ProcessChildren(childShape);

                                    childShape.FindNeighbours();
                                }

                                parentShape.FindNeighbours();

                                _ = this.ResizeShape(parentShape);
                            }
                            else
                            {
                                // create diagram
                                // this.visioHandler.CreateDocument();
                                var page = this.visioHandler.GetPageSize(15, 0);

                                // open excel handler
                                this.excelHandler.Open();

                                var results = this.excelHandler.RetrieveRecords();

                                if (results.Count < 1)
                                {
                                    return Task.CompletedTask;
                                }

                                // close excel
                                this.excelHandler.Close();

                                var shapeCounter = 0;

                                // fake diagramShape
                                parentShape = new DiagramShape(shapeCounter++) { ShapeText = "FAKE", Corners = page, ShapeType = ShapeType.FakeShape, LineLength = 1 };

                                this.ProcessTreeChildren(results, ref shapeCounter, parentShape);
                            }

                            this.logger.LogInformation("Moving and adjusting.");
                            var maxRightSide = parentShape.Corners.RightSide;

                            // adjust spacing
                            while (this.AdjustDiagram(parentShape, maxRightSide))
                            {
                            }

                            this.logger.LogInformation("Redraw shapes.");

                            this.visioHandler.VisualChanges(false);

                            this.visioHandler.ReDrawShapes(parentShape);

                            this.visioHandler.VisualChanges(true);
                        }

                        // ReSharper disable once CatchAllClause
                        catch (Exception e)
                        {
                            this.logger.LogError(e, "Error processing");
                        }

                        return Task.CompletedTask;
                    }, stoppingToken).ConfigureAwait(false);

            this.logger.LogInformation("Completed processing Visio Cleanup.");
            this.visioHandler.Close();

            this.appLifetime.StopApplication();
        }

        private static void RealignShapes(DiagramShape diagramShape)
        {
            if (!diagramShape.HasChildren())
            {
                return;
            }

            var sortedChildren = diagramShape.Children;
            var lineLength = diagramShape.LineLength;
            diagramShape.ClearNeighbours();

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

        /// <summary>
        ///     Iterates changes until there are no more.
        /// </summary>
        /// <param name="diagramShape">Shape being adjusted.</param>
        /// <param name="maxRightSide">Max right side.</param>
        /// TODO: Needs refactoring
        private bool AdjustDiagram(DiagramShape diagramShape, double maxRightSide)
        {
            var changed = false;

            // loop through children.
            foreach (var shape in diagramShape.Children.Where(shape => this.AdjustDiagram(shape, maxRightSide)))
            {
                changed = true;
            }

            if (changed)
            {
                return true;
            }

            // confirm size of shape.
            if (this.ResizeShape(diagramShape))
            {
                // loop again
                return true;
            }

            // place children within diagramShape
            foreach (var child in diagramShape.Children)
            {
                if (child.ShapeAbove is null)
                {
                    // nothing above
                    changed = changed || child.ShapeToLeft is null ? this.PlaceInCorner(diagramShape, child) : this.AlignToLeftShape(child);
                }
                else
                {
                    // shape above
                    changed = changed || child.ShapeToLeft is null ? this.AlignToShapeAbove(child) : this.AlignToLeftAndAbove(child);
                }
            }

            if (changed)
            {
                return true;
            }

            foreach (var shape in diagramShape.Children)
            {
                // resize to taller of neighbour
                var shapeToRight = shape.ShapeToRight;
                var shapeToLeft = shape.ShapeToLeft;
                if (shapeToLeft is not null)
                {
                    var heightToLeft = Math.Round(shapeToLeft.Corners.TopSide - shapeToLeft.Corners.BottomSide, 3, MidpointRounding.AwayFromZero);

                    changed = changed || this.ManualChangeHeight(heightToLeft, shape);
                }

                if (shapeToRight is null)
                {
                    continue;
                }

                var heightToRight = Math.Round(shapeToRight.Corners.TopSide - shapeToRight.Corners.BottomSide, 3, MidpointRounding.AwayFromZero);

                changed = changed || this.ManualChangeHeight(heightToRight, shape);
            }

            return changed || this.ShapesTooLong(diagramShape, maxRightSide);
        }

        private bool AlignToLeftAndAbove(DiagramShape child)
        {
            // shape to left
            var spacer = child.HasChildren() ? this.settings.VisioHorizontalSpacer : this.settings.VisioVerticalSpacer;
            var shapeAbove = child.ShapeAbove;
            var shapeToLeft = child.ShapeToLeft;
            if (shapeAbove is null || shapeToLeft is null)
            {
                throw new InvalidOperationException("This shouldn't happen!");
            }

            var topSide = Math.Round(shapeAbove.Corners.BottomSide - spacer, 3, MidpointRounding.AwayFromZero);
            var leftSide = Math.Round(shapeToLeft.Corners.RightSide + spacer, 3, MidpointRounding.AwayFromZero);

            return this.MoveToPosition(child, topSide, leftSide);
        }

        private bool AlignToLeftShape(DiagramShape child)
        {
            // shape to left
            var spacer = child.HasChildren() ? this.settings.VisioHorizontalSpacer : this.settings.VisioVerticalSpacer;
            var shapeToLeft = child.ShapeToLeft;
            if (shapeToLeft is null)
            {
                throw new InvalidOperationException("This shouldn't happen!");
            }

            var topSide = Math.Round(shapeToLeft.Corners.TopSide, 3, MidpointRounding.AwayFromZero);
            var leftSide = Math.Round(shapeToLeft.Corners.RightSide + spacer, 3, MidpointRounding.AwayFromZero);

            return this.MoveToPosition(child, topSide, leftSide);
        }

        private bool AlignToShapeAbove(DiagramShape child)
        {
            // nothing to left
            var spacer = child.HasChildren() ? this.settings.VisioHorizontalSpacer : this.settings.VisioVerticalSpacer;
            var shapeAbove = child.ShapeAbove;
            if (shapeAbove is null)
            {
                throw new InvalidOperationException("This shouldn't happen!");
            }

            var topSide = Math.Round(shapeAbove.Corners.BottomSide - spacer, 3, MidpointRounding.AwayFromZero);
            var leftSide = Math.Round(shapeAbove.Corners.LeftSide, 3, MidpointRounding.AwayFromZero);

            return this.MoveToPosition(child, topSide, leftSide);
        }

        /// <summary>
        ///     Move a shape for a spacer.
        /// </summary>
        /// <param name="movement">how far are we moving.</param>
        /// <param name="movementAction">Action for movement.</param>
        private void ExecuteMovement(double movement, Action<double> movementAction)
        {
            this.logger.LogDebug("{@Action} by {Movement}", movementAction.Method, movement);
            if (movement != 0)
            {
                movementAction(movement);
            }
        }

        private bool ManualChangeHeight(double newHeight, DiagramShape shape)
        {
            var myHeight = shape.Corners.Height();
            if (!(myHeight < newHeight))
            {
                return false;
            }

            var corners = shape.Corners;
            corners.BottomSide = Math.Round(corners.TopSide - newHeight, 3, MidpointRounding.AwayFromZero);

            if (shape.Corners.Equals(corners))
            {
                return false;
            }

            this.logger.LogDebug("Manual Resizing: {Shape}", shape);
            this.logger.LogDebug("New size for shape: {Corners}", corners);
            shape.Corners = corners;
            shape.IncreasedHeight = true;
            return true;
        }

        private bool MoveToPosition(DiagramShape diagramShape, double topSide, double leftSide)
        {
            if (diagramShape.Corners.TopSide.Equals(topSide) && diagramShape.Corners.LeftSide.Equals(leftSide))
            {
                return false;
            }

            this.logger.LogDebug("Moving: {Shape}", diagramShape);

            // move left
            this.ExecuteMovement(diagramShape.Corners.LeftSide - leftSide, diagramShape.MoveLeft);

            // move up
            this.ExecuteMovement(topSide - diagramShape.Corners.TopSide, diagramShape.MoveUp);
            this.logger.LogDebug("New position: {Corners}", diagramShape.Corners);
            return true;
        }

        private bool PlaceInCorner(DiagramShape diagramShape, DiagramShape child)
        {
            // nothing to left
            var topSide = Math.Round(diagramShape.Corners.TopSide - this.settings.TopPadding, 3, MidpointRounding.AwayFromZero);
            var leftSide = Math.Round(diagramShape.Corners.LeftSide + this.settings.LeftPadding, 3, MidpointRounding.AwayFromZero);

            return this.MoveToPosition(child, topSide, leftSide);
        }

        // TODO: Needs refactoring
        private void ProcessChildren(DiagramShape parentShape)
        {
            // find children
            var childShapeIds = this.visioHandler.GetChildren(parentShape.VisioId);
            foreach (var childId in childShapeIds)
            {
                // add to diagramShape
                var childShape = new DiagramShape(childId)
                                     {
                                         ShapeText = this.visioHandler.GetShapeText(childId),
                                         Corners = this.visioHandler.CalculateCorners(childId),
                                         ShapeType = ShapeType.Existing,
                                     };
                parentShape.AddChildShape(childShape);

                this.ProcessChildren(childShape);

                childShape.FindNeighbours();
            }

            if (parentShape.HasChildren())
            {
                parentShape.SortChildrenByNeighbour();
            }
        }

        // TODO: Needs refactoring
        private void ProcessTreeChildren(MyTree<string> results, ref int shapeCounter, DiagramShape diagramShape)
        {
            // loop children
            foreach (var result in results)
            {
                var childShape = new DiagramShape(shapeCounter++) { ShapeText = result.Value, ShapeType = ShapeType.NewShape };

                diagramShape.AddChildShape(childShape);

                // children
                if (result.Count > 0)
                {
                    this.ProcessTreeChildren(result, ref shapeCounter, childShape);
                }
            }

            // sort children
            if (diagramShape.HasChildren())
            {
                diagramShape.SortChildrenBySize();
                RealignShapes(diagramShape);
            }
        }

        // TODO: Needs refactoring
        private void RealignShapes(DiagramShape diagramShape, double maxLineSize)
        {
            if (!diagramShape.HasChildren())
            {
                return;
            }

            // reset all shapes.
            diagramShape.ClearNeighbours();

            var lines = new List<List<DiagramShape>>();
            var padding = diagramShape.Corners.LeftSide + this.settings.LeftPadding + this.settings.RightPadding;
            var lineWidth = maxLineSize - padding;
            double lineLength = 0;
            var currentLine = 0;
            var currentPosition = 0;
            lines.Insert(currentLine, new List<DiagramShape>());

            foreach (var child in diagramShape.Children)
            {
                var spacer = child.HasChildren() ? this.settings.VisioHorizontalSpacer : this.settings.VisioVerticalSpacer;
                var shapeWidth = child.Corners.Width() + spacer;

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
                            this.logger.LogError("Strange things are afoot at the circle K.");
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

            diagramShape.LineLength = lines[0].Count;

            var numberOfLines = diagramShape.Children.Count / (double)diagramShape.LineLength;
            if (numberOfLines >= 2)
            {
                return;
            }

            diagramShape.LineLength = (int)Math.Round(diagramShape.Children.Count / 2D, 0, MidpointRounding.ToPositiveInfinity);
            RealignShapes(diagramShape);
        }

        // TODO: Needs refactoring
        private bool ResizeShape(DiagramShape diagramShape)
        {
            if (diagramShape.IncreasedHeight)
            {
                return false;
            }

            Corners newCorners;
            if (diagramShape.HasChildren())
            {
                newCorners = diagramShape.Corners;
                var children = diagramShape.Children;

                var leftSide = children.Select(shape => shape.Corners.LeftSide).Min() - this.settings.LeftPadding;
                var rightSide = children.Select(shape => shape.Corners.RightSide).Max() + this.settings.RightPadding;
                var newWidth = rightSide - leftSide;

                var bottomSide = children.Select(shape => shape.Corners.BottomSide).Min() - this.settings.BottomPadding;
                var topSide = children.Select(shape => shape.Corners.TopSide).Max() + this.settings.TopPadding;
                var newHeight = topSide - bottomSide;

                newCorners.BottomSide = Math.Round(newCorners.TopSide - newHeight, 3, MidpointRounding.AwayFromZero);
                newCorners.RightSide = Math.Round(newCorners.LeftSide + newWidth, 3, MidpointRounding.AwayFromZero);
            }
            else
            {
                newCorners = diagramShape.Corners;
                newCorners.BottomSide = Math.Round(newCorners.TopSide - this.settings.UltimateShapeHeight, 3, MidpointRounding.AwayFromZero);
                newCorners.RightSide = Math.Round(newCorners.LeftSide + this.settings.UltimateShapeWidth, 3, MidpointRounding.AwayFromZero);
            }

            if (diagramShape.Corners.Equals(newCorners))
            {
                return false;
            }

            this.logger.LogDebug("Resizing: {Shape}", diagramShape);
            this.logger.LogDebug("New size for shape: {Corners}", newCorners);
            diagramShape.Corners = newCorners;
            return true;
        }

        private bool ShapesTooLong(DiagramShape diagramShape, double maxRightSide)
        {
            // do we know the line length?
            if (diagramShape.LineLength <= 0)
            {
                return false;
            }

            if (!diagramShape.HasChildren())
            {
                return false;
            }

            // check right side of shape
            if (!(diagramShape.Corners.RightSide >= maxRightSide))
            {
                return false;
            }

            var maxRightSideMinusOneShape = maxRightSide - this.settings.UltimateShapeWidth;
            if (!(diagramShape.Corners.LeftSide < maxRightSideMinusOneShape))
            {
                return false;
            }

            if (diagramShape.LineLength <= 1)
            {
                return false;
            }

            this.logger.LogDebug("To far over: {Shape}", diagramShape);

            // fit shape inside page
            this.RealignShapes(diagramShape, maxRightSide);

            // reset children to default representation.
            foreach (var child in diagramShape.Children.Where(child => child.HasChildren()))
            {
                child.LineLength = (int)Math.Round(child.Children.Count / 2D, 0, MidpointRounding.ToPositiveInfinity);
                RealignShapes(child);
            }

            return true;
        }
    }
}