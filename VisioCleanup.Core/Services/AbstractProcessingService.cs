// -----------------------------------------------------------------------
// <copyright file="AbstractProcessingService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models;

    /// <summary>
    /// Abstract implementation of common code for processing services.
    /// </summary>
    public abstract class AbstractProcessingService : IProcessingService
    {
        /// <inheritdoc />
        public Collection<DiagramShape> AllShapes { get; protected set; } = new();

        /// <inheritdoc />
        public DiagramShape? MasterShape { get; protected set; }

        /// <inheritdoc />
        public Task LayoutDataSet()
        {
            throw new NotImplementedException();
        }
    }
}