// -----------------------------------------------------------------------
// <copyright file="VisioCleanupService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Services
{
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    ///     Background service that runs engine.
    /// </summary>
    internal class VisioCleanupService : BackgroundService
    {
        private readonly ILogger<VisioCleanupService> logger;

        private readonly VisioCleanupSettings settings;

        private readonly IVisioHandler visioHandler;

        /// <summary>
        ///     Initialises a new instance of the <see cref="VisioCleanupService" /> class.
        /// </summary>
        /// <param name="settings">External settings.</param>
        /// <param name="logger">Logging.</param>
        /// <param name="visioHandler">Visio Handler.</param>
        public VisioCleanupService(
            IOptions<VisioCleanupSettings> settings,
            ILogger<VisioCleanupService> logger,
            IVisioHandler visioHandler)
        {
            this.settings = settings.Value;
            this.logger = logger;
            this.visioHandler = visioHandler;
        }

        /// <inheritdoc />
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            // clean-up tasks!
            this.logger.LogDebug(@"Cleaning up visio handler");
            this.visioHandler?.Close();

            return base.StopAsync(stoppingToken);
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogDebug("Starting Visio Cleanup Service");

            await Task.Run(
                async () =>
                    {
                        this.logger.LogDebug("Opening connection to visio.");
                        this.visioHandler.Open();
                    },
                stoppingToken).ConfigureAwait(false);

            this.logger.LogDebug("Completed processing Visio Cleanup.");
        }
    }
}