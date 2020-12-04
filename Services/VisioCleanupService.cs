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
                                var page = this.visioHandler.GetPageSize(this.settings.HeaderHeight, this.settings.SidePanelWidth);

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
        private bool AdjustDiagram(DiagramShape diagramShape, double maxRightSide)
        {
            var changed = false;

            // loop through children.
            foreach (var _ in diagramShape.Children.Where(_ => this.AdjustDiagram(_, maxRightSide)))
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

            // place children within parentShape
            foreach (var child in diagramShape.Children)
            {
                if (child.ShapeAbove is null)
                {
                    // nothing above
                    changed = changed || child.ShapeToLeft is null
                                  ? child.PlaceInCorner(this.settings.TopPadding, this.settings.LeftPadding)
                                  : child.AlignToLeftShape(child.HasChildren() ? this.settings.VisioHorizontalSpacer : this.settings.VisioVerticalSpacer);
                }
                else
                {
                    // shape above
                    changed = changed || child.ShapeToLeft is null
                                  ? child.AlignToShapeAbove(child.HasChildren() ? this.settings.VisioHorizontalSpacer : this.settings.VisioVerticalSpacer)
                                  : child.AlignToLeftAndAbove(child.HasChildren() ? this.settings.VisioHorizontalSpacer : this.settings.VisioVerticalSpacer);
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

                    changed = changed || shape.ManualChangeHeight(heightToLeft);
                }

                if (shapeToRight is null)
                {
                    continue;
                }

                var heightToRight = Math.Round(shapeToRight.Corners.TopSide - shapeToRight.Corners.BottomSide, 3, MidpointRounding.AwayFromZero);

                changed = changed || shape.ManualChangeHeight(heightToRight);
            }

            return changed || diagramShape.ShapesTooLong(
                       maxRightSide,
                       this.settings.UltimateShapeWidth,
                       this.settings.LeftPadding + this.settings.RightPadding,
                       shape => shape.HasChildren() ? this.settings.VisioHorizontalSpacer : this.settings.VisioVerticalSpacer);
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
            if (!diagramShape.HasChildren())
            {
                return;
            }

            diagramShape.SortChildrenBySize();
            diagramShape.RealignShapes();
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
            this.logger.LogDebug("New size for shape: {Corners}", newCorners.ToString());
            diagramShape.Corners = newCorners;
            return true;
        }
    }
}