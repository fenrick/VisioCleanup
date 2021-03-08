// -----------------------------------------------------------------------
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

        /// <summary>Initialises a new instance of the <see cref="VisioService" /> class.</summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="visioApplication">Visio application handler.</param>
        /// <param name="options">Application configuration being passed in.</param>
        public VisioService(ILogger<VisioService> logger, IVisioApplication visioApplication, IOptions<AppConfig> options)
            : base(logger, visioApplication) =>
            this.appConfig = options.Value ?? throw new ArgumentNullException(nameof(options));

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

                            this.VisioApplication.Open();

                            var selection = this.VisioApplication.Selection();
                            this.MasterShape = null;
                            this.AllShapes = new Collection<DiagramShape>();

                            // confirm one or more items selected.
                            if (selection.Length == 0)
                            {
                                this.Logger.LogDebug("No items selected, all loaded!");
                                return;
                            }

                            this.Logger.LogDebug("Create a fake parent shape.");
                            this.MasterShape = new DiagramShape(0) { ShapeText = "FAKE PARENT", ShapeType = ShapeType.FakeShape };
                            this.AllShapes.Add(this.MasterShape);

                            this.Logger.LogDebug("Adding children to parent.");
                            Parallel.ForEach(selection, childId => { this.ProcessChildren(this.MasterShape, childId); });

                            // set left and top for master shape. Resize will handle reset.
                            this.MasterShape.LeftSide = this.MasterShape.Children.Select(shape => shape.LeftSide).Min() - DiagramShape.ConvertMeasurement(this.appConfig.Left);
                            this.MasterShape.TopSide = this.MasterShape.Children.Select(shape => shape.TopSide).Max() + DiagramShape.ConvertMeasurement(this.appConfig.Top);

                            this.MasterShape.ResizeShape();

                            foreach (var shape in this.AllShapes)
                            {
                                shape.FindNeighbours();
                            }
                        }
                        finally
                        {
                            this.VisioApplication.Close();
                        }
                    });
        }

        private void ProcessChildren(DiagramShape parentShape, int visioId)
        {
            // process shape
            DiagramShape childShape = new(visioId)
                                          {
                                              ShapeText = this.VisioApplication.GetShapeText(visioId),
                                              LeftSide = this.VisioApplication.CalculateLeftSide(visioId),
                                              RightSide = this.VisioApplication.CalculateRightSide(visioId),
                                              TopSide = this.VisioApplication.CalculateTopSide(visioId),
                                              BaseSide = this.VisioApplication.CalculateBaseSide(visioId),
                                              ShapeType = ShapeType.Existing,
                                          };
            this.AllShapes.Add(childShape);
            parentShape.AddChildShape(childShape);

            // find children
            var childrenIds = this.VisioApplication.GetChildren(visioId).ToList();
            Parallel.ForEach(childrenIds, childId => { this.ProcessChildren(childShape, childId); });
        }
    }
}