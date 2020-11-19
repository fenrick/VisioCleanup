﻿// -----------------------------------------------------------------------
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

        private readonly ILogger<VisioCleanupService> logger;

        private readonly VisioCleanupSettings settings;

        private readonly IVisioHandler visioHandler;

        /// <summary>
        ///     Initialises a new instance of the <see cref="VisioCleanupService" /> class.
        /// </summary>
        /// <param name="settings">External settings.</param>
        /// <param name="logger">Logging.</param>
        /// <param name="visioHandler">Visio Handler.</param>
        /// <param name="appLifetime">Host application lifetime.</param>
        public VisioCleanupService(
            IOptions<VisioCleanupSettings> settings,
            ILogger<VisioCleanupService> logger,
            IVisioHandler visioHandler,
            IHostApplicationLifetime appLifetime)
        {
            this.settings = settings.Value;
            this.logger = logger;
            this.visioHandler = visioHandler;
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
            this.logger.LogDebug("Starting Visio Cleanup Service");

            stoppingToken.Register(() => { this.logger.LogDebug("Stopping Visio Cleanup Service."); });

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
                                this.logger.LogDebug("Processing parent shape.");
                                parentShape = new DiagramShape(masterShapeId)
                                                  {
                                                      ShapeText = this.visioHandler.GetShapeText(masterShapeId),
                                                      Corners = this.visioHandler.CalculateCorners(masterShapeId),
                                                      ShapeType = ShapeType.Existing,
                                                  };

                                this.logger.LogDebug("Processing children.");
                                var childShapeIds = await this.visioHandler.GetChildrenAsync(masterShapeId)
                                                        .ConfigureAwait(false);
                                foreach (var childId in childShapeIds)
                                {
                                    // add to parent
                                    var childShape = new DiagramShape(childId)
                                                         {
                                                             ShapeText = this.visioHandler.GetShapeText(childId),
                                                             Corners = this.visioHandler.CalculateCorners(childId),
                                                             ShapeType = ShapeType.Existing,
                                                         };
                                    parentShape.AddChildShape(childShape);

                                    // add children of child
                                    var secondaryChildShapesIds =
                                        await this.visioHandler.GetChildrenAsync(childId).ConfigureAwait(false);
                                    foreach (var secondaryChildId in secondaryChildShapesIds)
                                    {
                                        var secondaryChildShape = new DiagramShape(secondaryChildId)
                                                                      {
                                                                          ShapeText =
                                                                              this.visioHandler.GetShapeText(
                                                                                  secondaryChildId),
                                                                          Corners = this.visioHandler.CalculateCorners(
                                                                              secondaryChildId),
                                                                          ShapeType = ShapeType.Existing,
                                                                      };
                                        childShape.AddChildShape(secondaryChildShape);
                                    }
                                }
                            }
                            else
                            {
                                // create a fake master.
                                this.logger.LogDebug("Creating a fake master for selection.");
                                parentShape = new DiagramShape(0)
                                                  {
                                                      ShapeText = "FAKE",
                                                      Corners = default,
                                                      ShapeType = ShapeType.FakeShape,
                                                  };

                                this.logger.LogDebug("Adding selection as children");

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

                                    // add children of child
                                    var secondaryChildShapesIds =
                                        await this.visioHandler.GetChildrenAsync(childId).ConfigureAwait(false);
                                    foreach (var secondaryChildId in secondaryChildShapesIds)
                                    {
                                        var secondaryChildShape = new DiagramShape(secondaryChildId)
                                                                      {
                                                                          ShapeText = this.visioHandler.GetShapeText(
                                                                              secondaryChildId),
                                                                          Corners = this.visioHandler.CalculateCorners(
                                                                              secondaryChildId),
                                                                          ShapeType = ShapeType.Existing,
                                                                      };
                                        childShape.AddChildShape(secondaryChildShape);
                                    }
                                }
                            }

                            this.logger.LogDebug("Adjusting spacing.");

                            // adjust spacing
                            this.AdjustDiagram(parentShape);

                            this.logger.LogDebug("Redraw shapes.");

                            await this.visioHandler.ReDrawShapesAsync(parentShape).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            this.logger.LogError(
                                e,
                                "Error processing");
                        }
                    },
                stoppingToken).ConfigureAwait(false);

            this.logger.LogDebug("Completed processing Visio Cleanup.");

            this.appLifetime.StopApplication();
        }

        /// <summary>
        ///     Loop through child shapes and move them until no overlaps.
        /// </summary>
        /// <param name="diagramShape">Shape being adjusted.</param>
        private void AdjustDiagram(DiagramShape diagramShape)
        {
            this.logger.LogDebug(
                "Adjusting shape {ShapeID}: {ShapeText}",
                diagramShape.VisioId,
                diagramShape.ShapeText);

            if (!diagramShape.HasChildren())
            {
                this.logger.LogDebug("No children.");
                var newCorners = diagramShape.Corners;
                newCorners.BottomSide = newCorners.TopSide - this.settings.UltimateShapeHeight;
                newCorners.RightSide = newCorners.LeftSide + this.settings.UltimateShapeWidth;
                if (!diagramShape.Corners.Equals(newCorners))
                {
                    this.logger.LogDebug(
                        "New size for shape: {Corners}",
                        newCorners);
                }

                diagramShape.Corners = newCorners;

                return;
            }

            this.logger.LogDebug("Adjusting children.");
            foreach (var shape in diagramShape.Children)
            {
                this.AdjustDiagram(shape);

                // space above
                if (shape.ShapeAbove != null)
                {
                    var desiredSpace = shape.HasChildren()
                                           ? this.settings.VisioHorizontalSpacer
                                           : this.settings.VisioVerticalSpacer;

                    var movement = desiredSpace - (shape.ShapeAbove.Corners.BottomSide - shape.Corners.TopSide);

                    if (movement != 0)
                    {
                        this.logger.LogDebug(
                            "Shape above {ShapeID}: {ShapeText}",
                            shape.ShapeAbove.VisioId,
                            shape.ShapeAbove.ShapeText);

                        this.MoveForSpacer(
                            movement,
                            shape.ShapeAbove.MoveUp);
                    }
                }

                // space below
                if (shape.ShapeBelow != null)
                {
                    var desiredSpace = shape.HasChildren()
                                           ? this.settings.VisioHorizontalSpacer
                                           : this.settings.VisioVerticalSpacer;

                    var movement = desiredSpace - (shape.Corners.BottomSide - shape.ShapeBelow.Corners.TopSide);
                    if (movement != 0)
                    {
                        this.logger.LogDebug(
                            "Shape below {ShapeID}: {ShapeText}",
                            shape.ShapeBelow.VisioId,
                            shape.ShapeBelow.ShapeText);

                        this.MoveForSpacer(
                            movement,
                            shape.ShapeBelow.MoveDown);
                    }
                }

                // space left
                if (shape.ShapeToLeft != null)
                {
                    var movement = this.settings.VisioHorizontalSpacer
                                   - (shape.Corners.LeftSide - shape.ShapeToLeft.Corners.RightSide);

                    if (movement != 0)
                    {
                        this.logger.LogDebug(
                            "Shape to left {ShapeID}: {ShapeText}",
                            shape.ShapeToLeft.VisioId,
                            shape.ShapeToLeft.ShapeText);

                        this.MoveForSpacer(
                            movement,
                            shape.ShapeToLeft.MoveLeft);
                    }
                }

                // space right
                if (shape.ShapeToRight != null)
                {
                    var movement = this.settings.VisioHorizontalSpacer
                                   - (shape.ShapeToRight.Corners.LeftSide - shape.Corners.RightSide);

                    if (movement != 0)
                    {
                        this.logger.LogDebug(
                            "Shape to right {ShapeID}: {ShapeText}",
                            shape.ShapeToRight.VisioId,
                            shape.ShapeToRight.ShapeText);

                        this.MoveForSpacer(
                            movement,
                            shape.ShapeToRight.MoveRight);
                    }
                }
            }

            // if we moved the shapes at the end, let's just go over again to confirm all good.
            var changed = this.ShrinkToChildren(diagramShape);
            changed = this.AlignTop(diagramShape) || changed;

            if (changed)
            {
                this.logger.LogDebug(
                    "Processing again {ShapeID}: {ShapeText}",
                    diagramShape.VisioId,
                    diagramShape.ShapeText);
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

            // find far left side (assume non-ragged side).
            var children = diagramShape.Children;
            var leftSide = children.Select(shape => shape.Corners.LeftSide).Min();
            var bottomOrdered = children.Where(shape => shape.Corners.LeftSide.Equals(leftSide));

            foreach (var shape in bottomOrdered)
            {
                var nextShape = shape.ShapeToRight;
                while (nextShape != null)
                {
                    var offset = shape.Corners.TopSide - nextShape.Corners.TopSide;
                    if (offset != 0)
                    {
                        nextShape.MoveUp(offset);
                        changed = true;
                        this.logger.LogDebug(
                            "Aligning shape {ShapeID}: {ShapeText} by {Offset}",
                            nextShape.VisioId,
                            nextShape.ShapeText,
                            offset);
                    }

                    nextShape = nextShape.ShapeToRight;
                }
            }

            return changed;
        }

        /// <summary>
        ///     Move a shape for a spacer.
        /// </summary>
        /// <param name="currentSpace">Current space.</param>
        /// <param name="desiredSpace">Desired space.</param>
        /// <param name="movementAction">Action for movement.</param>
        private void MoveForSpacer(double movement, Action<double> movementAction)
        {
            this.logger.LogDebug(
                "{Action} by {Movement}",
                movementAction.Method,
                movement);
            if (movement != 0)
            {
                movementAction(movement);
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
                this.logger.LogDebug(
                    "Adjusting shape {ShapeID}: {ShapeText}",
                    diagramShape.VisioId,
                    diagramShape.ShapeText);
                this.logger.LogDebug(
                    "New size for shape: {Corners}",
                    newCorners);
            }

            return result;
        }
    }
}