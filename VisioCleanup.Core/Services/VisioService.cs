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

        public VisioService(ILogger<VisioService> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task LayoutDiagram()
        {
            this.logger.LogError("Not implemented yet.");
            await Task.Delay(5000);
            this.logger.LogError("Still not implemented yet.");
        }
    }
}