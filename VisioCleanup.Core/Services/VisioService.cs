// -----------------------------------------------------------------------
// <copyright file="VisioService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models;
    using VisioCleanup.Core.Models.Config;

    /// <summary>The visio service.</summary>
    public class VisioService : IVisioService
    {
        private readonly AppConfig appConfig;

        private readonly ILogger<VisioService> logger;

        private readonly IVisioApplication visioApplication;

        /// <summary>
        /// Initialises a new instance of the <see cref="VisioService"/> class.
        /// </summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="visioApplication">Visio application handler.</param>
        /// <param name="options">Application configuration being passed in.</param>
        public VisioService(ILogger<VisioService> logger, IVisioApplication visioApplication, IOptions<AppConfig> options)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.appConfig = options.Value ?? throw new ArgumentNullException(nameof(options));
            this.visioApplication = visioApplication ?? throw new ArgumentNullException(nameof(visioApplication));
        }

        /// <inheritdoc />
        public async Task LayoutDiagram()
        {
            await Task.Run(
                () =>
                    {
                        try
                        {
                            // setup DiagramShape
                            DiagramShape.AppConfig = this.appConfig;

                            this.visioApplication.Open();

                            var selection = this.visioApplication.Selection();
                        }
                        finally
                        {
                            this.visioApplication.Close();
                        }
                    });
        }
    }
}