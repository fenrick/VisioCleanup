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
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(
                async () =>
                    {
                        try
                        {
                            this.logger.LogDebug("Starting Visio Cleanup Service");

                            // open
                            this.visioHandler.Open();

                            var shapeId = this.visioHandler.SelectionPrimaryItem();

                            var children = this.visioHandler.GetChildren(shapeId);

                            // assemble processing structure
                            this.logger.LogDebug("Processing parent shape.");
                            var parentShape = new DiagramShape(
                                shapeId,
                                this.visioHandler.CalculateCorners(shapeId));

                            this.logger.LogDebug("Final all children.");
                            foreach (var childId in children)
                            {
                                parentShape.AddChildShape(
                                    new DiagramShape(
                                        childId,
                                        this.visioHandler.CalculateCorners(childId)));
                            }

                            this.logger.LogDebug($"Children found: {parentShape.Children.Count}");

                            this.logger.LogDebug("Shrink child shapes.");

                            // shrink child shapes.
                            foreach (var child in parentShape.Children)
                            {
                                var childCorners = child.Corners;
                                childCorners.BottomSide += 7.5;
                                child.Corners = childCorners;
                            }

                            this.logger.LogDebug("Redraw shapes.");

                            await this.visioHandler.ReDrawShapesAsync(parentShape).ConfigureAwait(false);

                            this.logger.LogDebug("Completed processing Visio Cleanup.");
                        }
                        catch (Exception e)
                        {
                            this.logger.LogError(e, "Error processing");
                        }
                    },
                stoppingToken);

            this.appLifetime.StopApplication();
        }
    }
}