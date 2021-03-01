﻿// -----------------------------------------------------------------------
// <copyright file="VisioService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models;
    using VisioCleanup.Core.Models.Config;

    /// <summary>The visio service.</summary>
    public class VisioService : AbstractProcessingService, IVisioService
    {
        private readonly AppConfig appConfig;

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
        public async Task LoadVisioObjectModel()
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
                            this.MasterShape = null;
                            this.AllShapes = new Collection<DiagramShape>();

                            // confirm one or more items selected.
                            if (selection.Length == 0)
                            {
                                this.logger.LogDebug("No items selected, all loaded!");
                                return;
                            }

                            this.logger.LogDebug("Create a fake parent shape.");
                            this.MasterShape = new DiagramShape(0) { ShapeText = "FAKE PARENT", ShapeType = ShapeType.FakeShape };
                            this.AllShapes.Add(this.MasterShape);

                            this.logger.LogDebug("Adding children to parent.");
                            foreach (var visioId in selection)
                            {
                                this.ProcessChildren(this.MasterShape, visioId);
                            }

                            this.MasterShape.ResizeShape();
                        }
                        finally
                        {
                            this.visioApplication.Close();
                        }
                    });
        }

        private void ProcessChildren(DiagramShape parentShape, int visioId)
        {
            // process shape
            DiagramShape childShape = new(visioId)
                                          {
                                              ShapeText = this.visioApplication.GetShapeText(visioId),
                                              LeftSide = this.visioApplication.CalculateLeftSide(visioId),
                                              RightSide = this.visioApplication.CalculateRightSide(visioId),
                                              TopSide = this.visioApplication.CalculateTopSide(visioId),
                                              BaseSide = this.visioApplication.CalculateBaseSide(visioId),
                                              ShapeType = ShapeType.Existing,
                                          };
            this.AllShapes.Add(childShape);
            parentShape.AddChildShape(childShape);

            // find children
            var childrenIds = this.visioApplication.GetChildren(visioId).ToList();
            foreach (var childId in childrenIds)
            {
                this.ProcessChildren(childShape, childId);
            }

            // find neighbours
            parentShape.FindNeighbours();
        }
    }
}