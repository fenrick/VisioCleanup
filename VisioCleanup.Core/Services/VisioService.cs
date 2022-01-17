// -----------------------------------------------------------------------
// <copyright file="VisioService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services;

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
    public void LoadVisioObjectModel()
    {
        try
        {
            this.VisioApplication.Open();

            List<DiagramShape> shapes = new();

            this.Logger.LogInformation("Create a fake parent shape");
            this.MasterShape = new DiagramShape(0) { ShapeText = "FAKE MASTER", ShapeType = ShapeType.FakeShape };
            shapes.Add(this.MasterShape);

            this.Logger.LogInformation("Retrieving selected shapes");
            shapes.AddRange(this.VisioApplication.RetrieveShapes());

            if (shapes.Count == 1)
            {
                return;
            }

            this.AllShapes.Clear();
            foreach (var diagramShape in shapes)
            {
                this.AllShapes.Add(diagramShape);
            }

            // turn overlaps into parents
            this.Logger.LogInformation("Finding parent shapes");
            foreach (var diagramShape in this.AllShapes)
            {
                var parentShape = this.FindClosestOverlap(diagramShape);
                parentShape?.AddChildShape(diagramShape);
            }

            // add children to master shape.
            this.Logger.LogInformation("Assigning fake parent");
            foreach (var shape in this.AllShapes.Where(shape => !shape.HasParent() && (shape.ShapeType != ShapeType.FakeShape)))
            {
                this.MasterShape!.AddChildShape(shape);
            }

            // set master shape size.
            this.MasterShape!.PositionX = this.MasterShape.Children.Values.Select(shape => shape.PositionX).Min() - DiagramShape.ConvertMeasurement(this.AppConfig.Left);
            this.MasterShape!.PositionY = this.MasterShape.Children.Values.Select(shape => shape.PositionY).Max() + DiagramShape.ConvertMeasurement(this.AppConfig.Top);

            this.MasterShape.ResizeShape();

            this.Logger.LogInformation("Finding shape neighours");
            foreach (var shape in this.AllShapes)
            {
                shape.FindNeighbours();
            }
        }
        finally
        {
            this.Logger.LogInformation("Closing connection to visio");
            this.VisioApplication.Close();
        }
    }

    private DiagramShape? FindClosestOverlap(DiagramShape diagramShape)
    {
        var diagramShapeTopSide = diagramShape.PositionY;
        var diagramShapeLeftSide = diagramShape.PositionX;
        var diagramShapeRightSide = diagramShape.RightSide;
        var diagramShapeBaseSide = diagramShape.PositionY - diagramShape.Height;

        bool AllSidesOverlap(DiagramShape shape) =>
            (shape.PositionX < diagramShapeLeftSide) && (shape.PositionY > diagramShapeTopSide) && (shape.RightSide > diagramShapeRightSide)
            && ((shape.PositionY - shape.Height) < diagramShapeBaseSide);

        var allOverlaps = this.AllShapes.Where(AllSidesOverlap);

        var minShapeArea = long.MaxValue;
        DiagramShape? minShape = null;
        foreach (var shape in allOverlaps.ToList())
        {
            var shapeArea = Math.BigMul(shape.Width(), shape.Height);
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
