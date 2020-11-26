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
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("Starting Visio Cleanup Service");

            stoppingToken.Register(() => { this.logger.LogInformation("Stopping Visio Cleanup Service."); });

            await Task.Run(
                async () =>
                    {
                        try
                        {
                            // open
                            this.visioHandler.Open();

                            var selection = this.visioHandler.Selection();

                            DiagramShape parentShape;
                            if (selection.Length == 1)
                            {
                                // use real master.
                                var masterShapeId = this.visioHandler.SelectionPrimaryItem();

                                // assemble processing structure
                                this.logger.LogInformation("Processing parent shape.");
                                parentShape = new DiagramShape(masterShapeId)
                                                  {
                                                      ShapeText = this.visioHandler.GetShapeText(masterShapeId),
                                                      Corners = this.visioHandler.CalculateCorners(masterShapeId),
                                                      ShapeType = ShapeType.Existing,
                                                  };

                                this.logger.LogInformation("Processing children.");

                                // process children until no more
                                await this.ProcessChildrenAsync(parentShape).ConfigureAwait(false);

                                parentShape.FindNeighbours();
                            }
                            else if (selection.Length > 1)
                            {
                                // create a fake master.
                                this.logger.LogInformation("Creating a fake master for selection.");
                                parentShape = new DiagramShape(0) { ShapeText = "FAKE", Corners = default, ShapeType = ShapeType.FakeShape };

                                this.logger.LogInformation("Adding selection as children");

                                foreach (var childId in selection)
                                {
                                    // add to parent
                                    var childShape = new DiagramShape(childId)
                                                         {
                                                             ShapeText = this.visioHandler.GetShapeText(childId),
                                                             Corners = this.visioHandler.CalculateCorners(childId),
                                                             ShapeType = ShapeType.Existing,
                                                         };
                                    parentShape.AddChildShape(childShape);

                                    await this.ProcessChildrenAsync(childShape).ConfigureAwait(false);

                                    childShape.FindNeighbours();
                                }

                                parentShape.FindNeighbours();
                            }
                            else
                            {
                                // create diagram
                                this.visioHandler.CreateDocument();
                                var page = this.visioHandler.GetPageSize();

                                // open excel handler
                                this.excelHandler.Open();

                                var results = this.excelHandler.RetrieveRecords();
                                var shapeCounter = 0;

                                // do we need a fake parent?
                                if (results.Count > 1)
                                {
                                    // fake parent
                                    parentShape = new DiagramShape(shapeCounter++) { ShapeText = "FAKE", Corners = default, ShapeType = ShapeType.FakeShape };
                                }
                                else
                                {
                                    results = results.First();

                                    // single record
                                    parentShape = new DiagramShape(shapeCounter++) { ShapeText = results.Value, ShapeType = ShapeType.NewShape };
                                }

                                ProcessTreeChildren(results, ref shapeCounter, parentShape);

                                // close excel
                                this.excelHandler.Close();
                            }

                            this.logger.LogInformation("Moving and adjusting.");

                            // adjust spacing
                            this.AdjustDiagram(parentShape);

                            this.logger.LogInformation("Redraw shapes.");

                            await this.visioHandler.ReDrawShapesAsync(parentShape).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            this.logger.LogError(e, "Error processing");
                        }
                    }, stoppingToken).ConfigureAwait(false);

            this.logger.LogInformation("Completed processing Visio Cleanup.");
            this.visioHandler.Close();

            this.appLifetime.StopApplication();
        }

        private static void ProcessTreeChildren(MyTree<string> results, ref int shapeCounter, DiagramShape parentShape)
        {
            foreach (var result in results)
            {
                var childShape = new DiagramShape(shapeCounter++) { ShapeText = result.Value, ShapeType = ShapeType.NewShape };

                parentShape.AddChildShape(childShape);

                if (result.Count > 0)
                {
                    ProcessTreeChildren(result, ref shapeCounter, childShape);
                }
            }
        }

        /// <summary>
        ///     Loop through child shapes and move them until no overlaps.
        /// </summary>
        /// <param name="diagramShape">Shape being adjusted.</param>
        private void AdjustDiagram(DiagramShape diagramShape)
        {
            if (!diagramShape.HasChildren())
            {
                var newCorners = diagramShape.Corners;
                newCorners.BottomSide = newCorners.TopSide - this.settings.UltimateShapeHeight;
                newCorners.RightSide = newCorners.LeftSide + this.settings.UltimateShapeWidth;
                if (!diagramShape.Corners.Equals(newCorners))
                {
                    this.logger.LogDebug("Adjusting shape {Shape}", diagramShape);
                    this.logger.LogDebug("New size for shape: {Corners}", newCorners);
                }

                diagramShape.Corners = newCorners;

                return;
            }

            var changed = false;

            foreach (var shape in diagramShape.Children)
            {
                this.AdjustDiagram(shape);

                // space below
                var nextShape = shape;
                while (nextShape != null && nextShape.ShapeBelow != null)
                {
                    var desiredSpace = nextShape.HasChildren() ? this.settings.VisioHorizontalSpacer : this.settings.VisioVerticalSpacer;

                    var movement = desiredSpace - (nextShape.Corners.BottomSide - nextShape.ShapeBelow.Corners.TopSide);

                    if (movement != 0)
                    {
                        this.logger.LogDebug("Adjusting shape {Shape}", nextShape.ShapeBelow);

                        this.MoveForSpacer(movement, nextShape.ShapeBelow.MoveDown);

                        changed = true;
                    }

                    nextShape = nextShape.ShapeBelow;
                }

                // space to right
                nextShape = shape;
                while (nextShape != null && nextShape.ShapeToRight != null)
                {
                    var desiredSpace = nextShape.HasChildren() ? this.settings.VisioHorizontalSpacer : this.settings.VisioVerticalSpacer;

                    var movement = desiredSpace - (nextShape.ShapeToRight.Corners.LeftSide - nextShape.Corners.RightSide);

                    if (movement != 0)
                    {
                        this.logger.LogDebug("Adjusting shape {Shape}", nextShape.ShapeToRight);

                        this.MoveForSpacer(movement, nextShape.ShapeToRight.MoveRight);

                        changed = true;
                    }

                    nextShape = nextShape.ShapeToRight;
                }
            }

            // if we moved the shapes at the end, let's just go over again to confirm all good.
            changed = this.ShrinkToChildren(diagramShape) || changed;
            changed = this.AlignTop(diagramShape) || changed;

            if (changed)
            {
                // Task.Run(() => { this.visioHandler.ReDrawShapesAsync(diagramShape); });
                this.logger.LogDebug("Processing again {Shape}", diagramShape);

                this.AdjustDiagram(diagramShape);
            }
        }

        private bool AlignTop(DiagramShape diagramShape)
        {
            if (!diagramShape.HasChildren())
            {
                return false;
            }

            var changed = false;
            var children = diagramShape.Children.OrderBy(shape => shape.Corners.LeftSide);

            foreach (var child in children)
            {
                DiagramShape? nextShape;

                // align down
                nextShape = child.ShapeBelow;
                while (nextShape != null)
                {
                    var offset = child.Corners.LeftSide - nextShape.Corners.LeftSide;
                    if (offset != 0)
                    {
                        nextShape.MoveRight(offset);
                        changed = true;
                        this.logger.LogDebug("Adjusting shape {Shape}", nextShape);
                        this.logger.LogDebug("Moving right by {Offset}", offset);
                    }

                    nextShape = nextShape.ShapeBelow;
                }

                // align right
                nextShape = child.ShapeToRight;
                while (nextShape != null)
                {
                    var offset = child.Corners.TopSide - nextShape.Corners.TopSide;
                    if (offset != 0)
                    {
                        nextShape.MoveUp(offset);
                        changed = true;
                        this.logger.LogDebug("Adjusting shape {Shape}", nextShape);
                        this.logger.LogDebug("Moving right up {Offset}", offset);
                    }

                    nextShape = nextShape.ShapeToRight;
                }
            }

            return changed;
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

        private async Task ProcessChildrenAsync(DiagramShape parentShape)
        {
            // find children
            var childShapeIds = await this.visioHandler.GetChildrenAsync(parentShape.VisioId).ConfigureAwait(false);
            foreach (var childId in childShapeIds)
            {
                await Task.Run(
                    async () =>
                        {
                            // add to parent
                            var childShape = new DiagramShape(childId)
                                                 {
                                                     ShapeText = this.visioHandler.GetShapeText(childId),
                                                     Corners = this.visioHandler.CalculateCorners(childId),
                                                     ShapeType = ShapeType.Existing,
                                                 };
                            parentShape.AddChildShape(childShape);

                            await this.ProcessChildrenAsync(childShape).ConfigureAwait(false);

                            childShape.FindNeighbours();
                        }).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Shrink shape to size of internal shapes.
        /// </summary>
        /// <param name="diagramShape">Shape to be resized.</param>
        /// <returns>Was the diagram changed.</returns>
        private bool ShrinkToChildren(DiagramShape diagramShape)
        {
            if (!diagramShape.HasChildren())
            {
                return false;
            }

            var newCorners = diagramShape.Corners;
            var children = diagramShape.Children;

            // left side
            var leftSide = children.Select(shape => shape.Corners.LeftSide).Min();
            newCorners.LeftSide = leftSide - this.settings.LeftPadding;

            // right side
            var rightSide = children.Select(shape => shape.Corners.RightSide).Max();
            newCorners.RightSide = rightSide + this.settings.RightPadding;

            // bottom side
            var bottomSide = children.Select(shape => shape.Corners.BottomSide).Min();
            newCorners.BottomSide = bottomSide - this.settings.BottomPadding;

            // top side
            var topSide = children.Select(shape => shape.Corners.TopSide).Max();
            newCorners.TopSide = topSide + this.settings.TopPadding;

            var result = !diagramShape.Corners.Equals(newCorners);
            diagramShape.Corners = newCorners;

            if (result)
            {
                this.logger.LogDebug("Adjusting shape {Shape}", diagramShape);
                this.logger.LogDebug("New size for shape: {Corners}", newCorners);
            }

            return result;
        }
    }
}