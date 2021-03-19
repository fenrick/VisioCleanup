// -----------------------------------------------------------------------
// <copyright file="VisioService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services
{
    using System;
    using System.Collections.Generic;
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
        /// <summary>Initialises a new instance of the <see cref="VisioService" /> class.</summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="visioApplication">Visio application handler.</param>
        /// <param name="options">Application configuration being passed in.</param>
        public VisioService(ILogger<VisioService> logger, IVisioApplication visioApplication, IOptions<AppConfig> options)
            : base(logger, options, visioApplication)
        {
            // all in base.
        }

        /// <inheritdoc />
        public async Task LoadVisioObjectModel()
        {
            await Task.Run(
                () =>
                    {
                        try
                        {
                            this.VisioApplication.Open();

                            List<DiagramShape> shapes = new();

                            this.Logger.LogInformation("Create a fake parent shape.");
                            this.MasterShape = new DiagramShape(0) { ShapeText = "FAKE PARENT", ShapeType = ShapeType.FakeShape };
                            shapes.Add(this.MasterShape);

                            this.Logger.LogInformation("Retrieving selected shapes.");
                            shapes.AddRange(this.VisioApplication.RetrieveShapes());

                            if (shapes.Count == 1)
                            {
                                return;
                            }

                            this.AllShapes = new Collection<DiagramShape>(shapes);

                            // turn overlaps into parents
                            this.Logger.LogInformation("Finding parent shapes.");
                            foreach (var diagramShape in this.AllShapes)
                            {
                                var parentShape = this.FindClosestOverlap(diagramShape);
                                parentShape?.AddChildShape(diagramShape);
                            }

                            // add children to master shape.
                            this.Logger.LogInformation("Assigning fake parent.");
                            foreach (var shape in this.AllShapes.Where(shape => !shape.HasParent() && (shape.ShapeType != ShapeType.FakeShape)))
                            {
                                this.MasterShape!.AddChildShape(shape);
                            }

                            // set master shape size.
                            this.MasterShape!.LeftSide = this.MasterShape.Children.Select(shape => shape.LeftSide).Min() - DiagramShape.ConvertMeasurement(this.AppConfig.Left);
                            this.MasterShape!.TopSide = this.MasterShape.Children.Select(shape => shape.TopSide).Max() + DiagramShape.ConvertMeasurement(this.AppConfig.Top);

                            this.MasterShape.ResizeShape();

                            this.Logger.LogInformation("Finding shape neighours.");
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

        private DiagramShape? FindClosestOverlap(DiagramShape diagramShape)
        {
            var allOverlaps = this.AllShapes.Where(
                shape => (shape.LeftSide < diagramShape.LeftSide) && (shape.TopSide > diagramShape.TopSide) && (shape.RightSide > diagramShape.RightSide)
                         && (shape.BaseSide < diagramShape.BaseSide));

            var minShapeArea = long.MaxValue;
            DiagramShape? minShape = null;
            foreach (var shape in allOverlaps.ToList())
            {
                var shapeArea = Math.BigMul(shape.Width(), shape.Height());
                if (minShapeArea <= shapeArea)
                {
                    continue;
                }

                minShape = shape;
                minShapeArea = shapeArea;
            }

            return minShape;
        }
    }
}