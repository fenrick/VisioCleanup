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

    using VisioCleanup.Core.Contracts;

    /// <summary>The visio service.</summary>
    public class VisioService : IVisioService
    {
        private readonly ILogger<VisioService> logger;

        private readonly IVisioApplication visioApplication;

        /// <summary>
        /// Initialises a new instance of the <see cref="VisioService"/> class.
        /// </summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="visioApplication">Visio application handler.</param>
        public VisioService(ILogger<VisioService> logger, IVisioApplication visioApplication)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                            this.visioApplication.Open();
                        }
                        finally
                        {
                            this.visioApplication.Close();
                        }
                    });
        }
    }
}