// -----------------------------------------------------------------------
// <copyright file="VisioCleanupService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Services
{
    using System;
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

                            var masterShapeId = this.visioHandler.SelectionPrimaryItem();

                            // assemble processing structure
                            this.logger.LogDebug("Processing parent shape.");
                            var parentShape = new DiagramShape(
                                masterShapeId,
                                this.visioHandler.GetShapeText(masterShapeId),
                                this.visioHandler.CalculateCorners(masterShapeId));

                            this.logger.LogDebug("Processing children.");
                            var childShapeIds = this.visioHandler.GetChildren(masterShapeId);
                            foreach (var childId in childShapeIds)
                            {
                                // add to parent
                                var childShape = new DiagramShape(
                                    childId,
                                    this.visioHandler.GetShapeText(childId),
                                    this.visioHandler.CalculateCorners(childId));
                                parentShape.AddChildShape(childShape);

                                // add children of child
                                var secondaryChildShapesIds = this.visioHandler.GetChildren(childId);
                                foreach (var secondaryChildId in secondaryChildShapesIds)
                                {
                                    var secondaryChildShape = new DiagramShape(
                                        secondaryChildId,
                                        this.visioHandler.GetShapeText(secondaryChildId),
                                        this.visioHandler.CalculateCorners(secondaryChildId));
                                    childShape.AddChildShape(secondaryChildShape);
                                }
                            }

                            this.logger.LogDebug("Adjusting spacing.");

                            // adjust spacing
                            parentShape.AdjustDiagram(
                                this.settings.VisioVerticalSpacer,
                                this.settings.VisioHorizontalSpacer,
                                45,
                                7.5);

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
    }
}