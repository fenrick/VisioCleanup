// -----------------------------------------------------------------------
// <copyright file="VisioCleanupService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Services
{
    using System;
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
                            // setup DiagramShape
                            DiagramShape.HorizontalSpacer = this.settings.VisioHorizontalSpacer;
                            DiagramShape.VerticalSpacer = this.settings.VisioVerticalSpacer;
                            DiagramShape.TopPadding = this.settings.TopPadding;
                            DiagramShape.LeftPadding = this.settings.LeftPadding;
                            DiagramShape.RightPadding = this.settings.RightPadding;
                            DiagramShape.BottomPadding = this.settings.BottomPadding;
                            DiagramShape.UltimateShapeWidth = this.settings.UltimateShapeWidth;
                            DiagramShape.UltimateShapeHeight = this.settings.UltimateShapeHeight;

                            // open
                            this.visioHandler.Open();

                            var selection = this.visioHandler.Selection();

                            DiagramShape parentShape;
                            if (selection.Length >= 1)
                            {
                                // create a fake master.
                                var page = this.visioHandler.GetPageSize(15, 0);
                                this.logger.LogInformation("Creating a fake master for selection.");
                                parentShape = new DiagramShape(0) { ShapeText = "FAKE", Corners = default, ShapeType = ShapeType.FakeShape };

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

                                this.SizeFakeShape(parentShape);
                            }
                            else
                            {
                                // create diagram
                                // this.visioHandler.CreateDocument();
                                var page = this.visioHandler.GetPageSize(this.settings.HeaderHeight, this.settings.SidePanelWidth);

                                // open excel handler
                                this.excelHandler.Open();

                                var results = this.excelHandler.RetrieveRecords();

                                this.logger.LogInformation("Data loaded from Excel.");

                                if (!results.Any())
                                {
                                    return Task.CompletedTask;
                                }

                                // close excel
                                this.excelHandler.Close();

                                // excel leaves 0 unassigned.

                                // fake diagramShape
                                parentShape = new DiagramShape(0) { ShapeText = "FAKE", Corners = page, ShapeType = ShapeType.FakeShape, LineLength = 1 };
                                foreach (var shape in results.Where(kvp => !kvp.Value.HasParent()).Select(kvp => kvp.Value))
                                {
                                    parentShape.AddChildShape(shape);
                                }

                                this.logger.LogInformation("Initial shape mapping.");
                                this.ProcessExcelChildren(parentShape);
                                parentShape.Corners = page;
                                parentShape.LineLength = 1;
                            }

                            this.logger.LogInformation("Moving and adjusting.");
                            var maxRightSide = parentShape.Corners.RightSide;

                            // adjust spacing
                            var counter = 1;
                            //while (this.AdjustDiagram(parentShape, maxRightSide))
                            //{
                            //    this.logger.LogDebug("Moving and adjusting: {Counter}", counter++);
                            //}

                            this.logger.LogInformation("Redraw shapes.");

                            try
                            {
                                this.visioHandler.VisualChanges(false);
                                this.visioHandler.UpdateVisio(parentShape).Wait();
                            }
                            finally
                            {
                                this.visioHandler.VisualChanges(true);
                            }
                        }

                        // ReSharper disable once CatchAllClause
                        catch (Exception e)
                        {
                            this.logger.LogError(e, "Error processing");
                        }

                        return Task.CompletedTask;
                    },
                stoppingToken).ConfigureAwait(false);

            this.logger.LogInformation("Completed processing Visio Cleanup.");
            this.visioHandler.Close();

            this.appLifetime.StopApplication();
        }

        /// <summary>
        ///     Iterates changes until there are no more.
        /// </summary>
        /// <param name="diagramShape">Shape being adjusted.</param>
        /// <param name="maxRightSide">Max right side.</param>
        /// TODO: Needs refactoring
        private bool AdjustDiagram(DiagramShape diagramShape, int maxRightSide)
        {
            var changed = false;

            // depth first resizing - bulk action to simplify processing.
            foreach (var child in diagramShape.Children.Where(child => this.ResizeShape(diagramShape)))
            {
                changed = true;
            }

            if (changed)
            {
                return true;
            }

            // loop through children.
            foreach (var child in diagramShape.Children.Where(child => this.AdjustDiagram(child, maxRightSide)))
            {
                changed = true;
            }

            DiagramShape.Align(diagramShape);

            if (changed)
            {
                return true;
            }

            // resize to taller of neighbour
            var shapeToRight = diagramShape.ShapeToRight;
            var shapeToLeft = diagramShape.ShapeToLeft;
            if (shapeToLeft is not null)
            {
                var heightToLeft = shapeToLeft.Corners.TopSide - shapeToLeft.Corners.BottomSide;

                changed = diagramShape.ManualChangeHeight(heightToLeft);
            }

            if (changed)
            {
                return true;
            }

            if (shapeToRight is not null)
            {
                var heightToRight = shapeToRight.Corners.TopSide - shapeToRight.Corners.BottomSide;

                changed = diagramShape.ManualChangeHeight(heightToRight);
            }

            if (changed)
            {
                return true;
            }

            changed = diagramShape.ShapesTooLong(maxRightSide);

            if (changed)
            {
                return true;
            }

            return changed;
        }

        // TODO: Needs refactoring
        private void ProcessChildren(DiagramShape parentShape)
        {
            // find children
            var childShapeIds = this.visioHandler.GetChildren(parentShape.VisioId);
            Parallel.ForEach(
                childShapeIds,
                childId =>
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

                        parentShape.FindNeighbours();
                    });

            if (parentShape.HasChildren())
            {
                parentShape.SortChildrenByNeighbour();
            }
        }

        // TODO: Needs refactoring
        private void ProcessExcelChildren(DiagramShape diagramShape)
        {
            // sort children
            if (!diagramShape.HasChildren())
            {
                return;
            }

            diagramShape.SortChildrenBySize(
                shape =>
                    {
                        if (shape.SortValue is null)
                        {
                            return 0 - shape.TotalChildrenCount();
                        }

                        return shape.SortValue;
                    });

            diagramShape.LineLength = (int)Math.Round(diagramShape.Children.Count / 2D, 0, MidpointRounding.ToPositiveInfinity);
            if (diagramShape.ParentShape is not null)
            {
                // size to parent's internal width with spacing.
                var lineWidth = diagramShape.ParentShape.Corners.Width() - (this.settings.LeftPadding + this.settings.RightPadding);
                diagramShape.RealignShapes(lineWidth);
            }
            else
            {
                // size to internal width (spacing is already taken into account)
                var lineWidth = diagramShape.Corners.Width();
                diagramShape.RealignShapes();
                diagramShape.RealignShapes(lineWidth);
            }

            // loop children
            foreach (var child in diagramShape.Children)
            {
                this.ProcessExcelChildren(child);
            }
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

                newCorners.BottomSide = newCorners.TopSide - newHeight;
                newCorners.RightSide = newCorners.LeftSide + newWidth;
            }
            else
            {
                newCorners = diagramShape.Corners;
                newCorners.BottomSide = newCorners.TopSide - this.settings.UltimateShapeHeight;
                newCorners.RightSide = newCorners.LeftSide + this.settings.UltimateShapeWidth;
            }

            if (diagramShape.Corners.Equals(newCorners))
            {
                return false;
            }

            this.logger.LogDebug("Resizing: {Shape}", diagramShape);
            this.logger.LogDebug("New size for shape: {Corners}", newCorners.ToString());
            diagramShape.Corners = newCorners;
            return true;
        }

        private void SizeFakeShape(DiagramShape diagramShape)
        {
            if (!diagramShape.HasChildren())
            {
                return;
            }

            var newCorners = diagramShape.Corners;
            var children = diagramShape.Children;

            var leftSide = children.Select(shape => shape.Corners.LeftSide).Min() - this.settings.LeftPadding;
            var rightSide = children.Select(shape => shape.Corners.RightSide).Max() + this.settings.RightPadding;

            var bottomSide = children.Select(shape => shape.Corners.BottomSide).Min() - this.settings.BottomPadding;
            var topSide = children.Select(shape => shape.Corners.TopSide).Max() + this.settings.TopPadding;

            newCorners.BottomSide = bottomSide;
            newCorners.TopSide = topSide;
            newCorners.RightSide = rightSide;
            newCorners.LeftSide = leftSide;

            if (diagramShape.Corners.Equals(newCorners))
            {
                return;
            }

            this.logger.LogDebug("Resizing: {Shape}", diagramShape);
            this.logger.LogDebug("New size for shape: {Corners}", newCorners.ToString());
            diagramShape.Corners = newCorners;
        }
    }
}