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
                                parentShape = new DiagramShape(shapeCounter++) { ShapeText = "FAKE", Corners = page, ShapeType = ShapeType.FakeShape };

                                this.ProcessTreeChildren(results, ref shapeCounter, parentShape);
                            }

                            this.logger.LogInformation("Moving and adjusting.");

                            // adjust spacing
                            while (this.AdjustDiagram(parentShape, parentShape.Corners.RightSide))
                            {
                            }

                            this.logger.LogInformation("Redraw shapes.");

                            this.visioHandler.VisualChanges(false);

                            this.visioHandler.ReDrawShapes(parentShape);

                            this.visioHandler.VisualChanges(true);
                        }
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

        /// <summary>
        ///     Iterates changes until there are no more.
        /// </summary>
        /// <param name="diagramShape">Shape being adjusted.</param>
        /// <param name="maxRightSide"></param>
        private bool AdjustDiagram(DiagramShape diagramShape, double maxRightSide)
        {
            var changed = false;

            // loop through children.
            foreach (var shape in diagramShape.Children)
            {
                // loop through children.
                if (this.AdjustDiagram(shape, maxRightSide))
                {
                    changed = true;
                }
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
            foreach (var shape in diagramShape.Children)
            {
                if (shape.ShapeAbove is null)
                {
                    // nothing above
                    if (shape.ShapeToLeft is null)
                    {
                        // nothing to left
                        var topSide = Math.Round(diagramShape.Corners.TopSide - this.settings.TopPadding, 3, MidpointRounding.AwayFromZero);
                        var leftSide = Math.Round(diagramShape.Corners.LeftSide + this.settings.LeftPadding, 3, MidpointRounding.AwayFromZero);

                        if (!(shape.Corners.TopSide.Equals(topSide) && shape.Corners.LeftSide.Equals(leftSide)))
                        {
                            this.logger.LogDebug("Moving: {Shape}", shape);
                            this.MoveTo(shape, leftSide, topSide);
                            this.logger.LogDebug("New position: {Corners}", shape.Corners);
                            changed = true;
                        }
                    }
                    else
                    {
                        // shape to left
                        var spacer = shape.HasChildren() ? this.settings.VisioHorizontalSpacer : this.settings.VisioVerticalSpacer;
                        var topSide = Math.Round(diagramShape.Corners.TopSide - this.settings.TopPadding, 3, MidpointRounding.AwayFromZero);
                        var leftSide = Math.Round(shape.ShapeToLeft.Corners.RightSide + spacer, 3, MidpointRounding.AwayFromZero);

                        if (!(shape.Corners.TopSide.Equals(topSide) && shape.Corners.LeftSide.Equals(leftSide)))
                        {
                            this.logger.LogDebug("Moving: {Shape}", shape);
                            this.MoveTo(shape, leftSide, topSide);
                            this.logger.LogDebug("New position: {Corners}", shape.Corners);
                            changed = true;
                        }
                    }
                }
                else
                {
                    // shape above
                    if (shape.ShapeToLeft is null)
                    {
                        // nothing to left
                        var spacer = shape.HasChildren() ? this.settings.VisioHorizontalSpacer : this.settings.VisioVerticalSpacer;
                        var topSide = Math.Round(shape.ShapeAbove.Corners.BottomSide - spacer, 3, MidpointRounding.AwayFromZero);
                        var leftSide = Math.Round(diagramShape.Corners.LeftSide + this.settings.LeftPadding, 3, MidpointRounding.AwayFromZero);

                        if (!(shape.Corners.TopSide.Equals(topSide) && shape.Corners.LeftSide.Equals(leftSide)))
                        {
                            this.logger.LogDebug("Moving: {Shape}", shape);
                            this.MoveTo(shape, leftSide, topSide);
                            this.logger.LogDebug("New position: {Corners}", shape.Corners);
                            changed = true;
                        }
                    }
                    else
                    {
                        // shape to left
                        var horizontalSpacer = shape.HasChildren() ? this.settings.VisioHorizontalSpacer : this.settings.VisioVerticalSpacer;
                        var topSide = Math.Round(shape.ShapeAbove.Corners.BottomSide - horizontalSpacer, 3, MidpointRounding.AwayFromZero);
                        var leftSide = Math.Round(shape.ShapeToLeft.Corners.RightSide + horizontalSpacer, 3, MidpointRounding.AwayFromZero);

                        if (!(shape.Corners.TopSide.Equals(topSide) && shape.Corners.LeftSide.Equals(leftSide)))
                        {
                            this.logger.LogDebug("Moving: {Shape}", shape);
                            this.MoveTo(shape, leftSide, topSide);
                            this.logger.LogDebug("New position: {Corners}", shape.Corners);
                            changed = true;
                        }
                    }
                }
            }

            if (changed)
            {
                return true;
            }

            foreach (var shape in diagramShape.Children)
            {
                // resize to taller of neighbour
                if (shape.ShapeToLeft is not null)
                {
                    var myHeight = diagramShape.Corners.Height();
                    var neighbourHeight = Math.Round(shape.ShapeToLeft.Corners.TopSide - shape.ShapeToLeft.Corners.BottomSide, 3, MidpointRounding.AwayFromZero);

                    if (myHeight < neighbourHeight)
                    {
                        var corners = shape.Corners;
                        corners.BottomSide = Math.Round(corners.TopSide - neighbourHeight, 3, MidpointRounding.AwayFromZero);

                        if (!shape.Corners.Equals(corners))
                        {
                            this.logger.LogDebug("Manual Resizing: {Shape}", diagramShape);
                            this.logger.LogDebug("New size for shape: {Corners}", corners);
                            shape.Corners = corners;
                            shape.IncreasedHeight = true;
                            changed = true;
                        }
                    }
                }

                if (shape.ShapeToRight is not null)
                {
                    var myHeight = diagramShape.Corners.Height();
                    var neighbourHeight = Math.Round(shape.ShapeToRight.Corners.TopSide - shape.ShapeToRight.Corners.BottomSide, 3, MidpointRounding.AwayFromZero);

                    if (myHeight < neighbourHeight)
                    {
                        var corners = shape.Corners;
                        corners.BottomSide = Math.Round(corners.TopSide - neighbourHeight, 3, MidpointRounding.AwayFromZero);

                        if (!shape.Corners.Equals(corners))
                        {
                            this.logger.LogDebug("Manual Resizing: {Shape}", diagramShape);
                            this.logger.LogDebug("New size for shape: {Corners}", corners);
                            shape.Corners = corners;
                            shape.IncreasedHeight = true;
                            changed = true;
                        }
                    }
                }
            }

            if (changed)
            {
                return true;
            }

            // do we know the line length?
            if (diagramShape.LineLength > 0 && diagramShape.HasChildren())
            {
                // check right side of shape
                if (diagramShape.Corners.RightSide >= maxRightSide && diagramShape.Corners.LeftSide < maxRightSide - this.settings.UltimateShapeWidth)
                {
                    if (diagramShape.LineLength > 1)
                    {
                        this.logger.LogDebug("To far over: {Shape}", diagramShape);

                        if (diagramShape.ParentShape.Children.Count > diagramShape.ParentShape.LineLength)
                        {
                            diagramShape.ParentShape.LineLength = diagramShape.ParentShape.Children.Count;
                        }

                        this.RealignShapes(diagramShape, maxRightSide);
                        return true;
                    }
                }

                // if line length is 1
            }

            return false;
        }

        /// <summary>
        ///     Move a shape for a spacer.
        /// </summary>
        /// <param name="movement">how far are we moving.</param>
        /// <param name="movementAction">Action for movement.</param>
        private void MoveForSpacer(double movement, Action<double> movementAction)
        {
            this.logger.LogDebug("{@Action} by {Movement}", movementAction.Method, movement);
            if (movement != 0)
            {
                movementAction(movement);
            }
        }

        private void MoveTo(DiagramShape diagramShape, double leftSide, double topSide)
        {
            this.MoveForSpacer(diagramShape.Corners.LeftSide - leftSide, diagramShape.MoveLeft);
            this.MoveForSpacer(topSide - diagramShape.Corners.TopSide, diagramShape.MoveUp);
        }

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

            if (!parentShape.HasChildren())
            {
                return;
            }

            var childCounter = 0;
            SortedList<int, DiagramShape> sortedChildren = new SortedList<int, DiagramShape>(parentShape.Children.Count);
            var children = parentShape.Children;

            // var childLineStart;
            // var currentChild;
            var topChild = children.Where(shape => shape.Corners.TopSide == children.Max(innerShape => innerShape.Corners.TopSide)).OrderBy(shape => shape.Corners.LeftSide)
                .First();
            var childLineStart = topChild;
            var currentChild = topChild;
            do
            {
                sortedChildren.Add(childCounter++, currentChild);
                while (currentChild.ShapeToRight is not null)
                {
                    currentChild = currentChild.ShapeToRight;
                    sortedChildren.Add(childCounter++, currentChild);
                }

                childLineStart = childLineStart.ShapeBelow;
                currentChild = childLineStart;
            }
            while (childLineStart is not null);

            parentShape.Children.Clear();
            parentShape.Children.AddRange(sortedChildren.Values);
        }

        private void ProcessTreeChildren(MyTree<string> results, ref int shapeCounter, DiagramShape parentShape)
        {
            // loop children
            foreach (var result in results)
            {
                var childShape = new DiagramShape(shapeCounter++) { ShapeText = result.Value, ShapeType = ShapeType.NewShape };

                parentShape.AddChildShape(childShape);

                // children
                if (result.Count > 0)
                {
                    this.ProcessTreeChildren(result, ref shapeCounter, childShape);
                }
            }

            // sort children
            if (!parentShape.HasChildren())
            {
                return;
            }

            var sortedChildren = parentShape.Children.OrderByDescending(shape => shape.TotalChildrenCount()).ToList();

            parentShape.Children.Clear();
            parentShape.Children.AddRange(sortedChildren);

            this.RealignShapes(parentShape.Children.ToArray(), parentShape.Children.Count);
            parentShape.LineLength = parentShape.Children.Count;
        }

        private void RealignShapes(DiagramShape[] sortedChildren, int lineLength)
        {
            for (var i = 0; i < sortedChildren.Length; i++)
            {
                var child = sortedChildren[i];

                // line number & position
                var lineNumber = (int)Math.Truncate((double)(i / lineLength));
                var linePosition = i % lineLength;

                // find below
                var positionBelow = (lineNumber + 1) * lineLength + linePosition;
                if (sortedChildren.Length > positionBelow)
                {
                    var belowChild = sortedChildren[positionBelow];
                    child.ShapeBelow = belowChild;
                }

                // find to left
                if (linePosition + 1 <= lineLength - 1)
                {
                    var positionToLeft = lineNumber * lineLength + linePosition + 1;
                    if (sortedChildren.Length > positionToLeft)
                    {
                        var leftChild = sortedChildren[positionToLeft];
                        child.ShapeToRight = leftChild;
                    }
                }
            }
        }

        private void RealignShapes(DiagramShape diagramShape, double maxLineSize)
        {
            if (!diagramShape.HasChildren())
            {
                return;
            }

            // reset all shapes.
            foreach (var shape in diagramShape.Children)
            {
                shape.ShapeAbove = null;
                shape.ShapeBelow = null;
                shape.ShapeToLeft = null;
                shape.ShapeToRight = null;
                shape.IncreasedHeight = false;
            }

            List<List<DiagramShape>> lines = new List<List<DiagramShape>>();
            var lineWidth = maxLineSize - diagramShape.Corners.LeftSide;
            double lineLength = 0;
            var currentLine = 0;
            var currentPosition = 0;
            lines.Insert(currentLine, new List<DiagramShape>());

            foreach (var child in diagramShape.Children)
            {
                var spacer = child.HasChildren() ? this.settings.VisioHorizontalSpacer : this.settings.VisioVerticalSpacer;

                if (child.Corners.Width() > lineWidth)
                {
                    lineWidth = child.Corners.Width() + spacer + 1;
                }

                if (!(lineLength + child.Corners.Width() + spacer < lineWidth))
                {
                    currentPosition = 0;
                    currentLine++;
                    lines.Insert(currentLine, new List<DiagramShape>());
                    lineLength = 0;
                }

                lines[currentLine].Insert(currentPosition, child);
                lineLength += child.Corners.Width() + spacer;
                if (currentLine >= 1 && lines.Count >= currentLine - 1)
                {
                    child.ShapeAbove = lines[currentLine - 1][currentPosition];
                }

                if (currentPosition >= 1 && lines[currentLine].Count >= currentPosition - 1)
                {
                    child.ShapeToLeft = lines[currentLine][currentPosition - 1];
                }

                currentPosition++;
            }

            diagramShape.LineLength = lines[0].Count;
        }

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
    }
}