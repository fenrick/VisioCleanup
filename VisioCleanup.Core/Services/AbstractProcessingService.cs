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
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models;
    using VisioCleanup.Core.Models.Config;

    /// <inheritdoc />
    /// <summary>Abstract implementation of common code for processing services.</summary>
    public abstract class AbstractProcessingService : IProcessingService
    {
        /// <summary>Store for converted app config right measure.</summary>
        protected int convertedAppConfigRight;

        /// <summary>Initialises a new instance of the <see cref="AbstractProcessingService" /> class.</summary>
        /// <param name="logger">Logger.</param>
        /// <param name="options">Application configuration being passed in.</param>
        /// <param name="visioApplication">Visio Application engine.</param>
        protected AbstractProcessingService(ILogger logger, IOptions<AppConfig> options, IVisioApplication visioApplication)
        {
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.VisioApplication = visioApplication ?? throw new ArgumentNullException(nameof(visioApplication));
            this.AppConfig = options.Value ?? throw new ArgumentNullException(nameof(options));

            // setup DiagramShape
            DiagramShape.AppConfig = this.AppConfig;
        }

        /// <inheritdoc />
        public Collection<DiagramShape> AllShapes { get; protected set; } = new();

        /// <inheritdoc />
        public DiagramShape? MasterShape { get; protected set; }

        /// <summary>Gets application configuration.</summary>
        protected AppConfig AppConfig { get; }

        /// <summary>Gets logging environment.</summary>
        protected ILogger Logger { get; }

        /// <summary>Gets visio processing engine.</summary>
        protected IVisioApplication VisioApplication { get; }

        /// <inheritdoc />
        public Task LayoutDataSet()
        {
            return Task.Run(
                () =>
                    {
                        var counter = 1;

                        do
                        {
                            this.Logger.LogInformation("Correcting diagram: pass {Count}", counter++);

                            if (counter > 10)
                            {
                                break;
                            }
                        }
                        while (this.MasterShape!.CorrectDiagram());
                    });
        }

        /// <inheritdoc />
        public Task UpdateVisio()
        {
            return Task.Run(
                () =>
                    {
                        try
                        {
                            this.VisioApplication.Open();
                            this.VisioApplication.VisualChanges(false);
                            this.Logger.LogInformation("Modelling changes to visio");

                            // update each shape
                            foreach (var diagramShape in this.AllShapes)
                            {
                                switch (diagramShape.ShapeType)
                                {
                                    case ShapeType.NewShape:
                                        this.Logger.LogDebug("Dropping new shape: {Shape}", diagramShape);
                                        this.VisioApplication.CreateShape(diagramShape);
                                        break;
                                    case ShapeType.Existing:
                                        this.Logger.LogDebug("Updating shape: {Shape}", diagramShape);
                                        this.VisioApplication.UpdateShape(diagramShape);
                                        break;
                                    case ShapeType.FakeShape:
                                        // we don't draw this!
                                        this.Logger.LogDebug("Skipping fake shape: {Shape}", diagramShape);
                                        break;
                                    default:
                                        throw new InvalidOperationException("ShapeType not matched");
                                }
                            }
                        }
                        finally
                        {
                            this.Logger.LogInformation("Recalculating diagrams");
                            this.VisioApplication.VisualChanges(true);
                            this.VisioApplication.Close();
                            this.Logger.LogInformation("Visio closed");
                        }
                    });
        }

        /// <summary>Sort the children of the diagram shape.</summary>
        /// <param name="diagramShape">Shape that's children are to be sorted.</param>
        /// <param name="maxRight">Maximum right side.</param>
        protected void SortChildren(DiagramShape diagramShape, int maxRight)
        {
            var internalMaxRight = maxRight - this.convertedAppConfigRight;

            var orderedChildren = diagramShape.Children.OrderBy<DiagramShape, object>(
                shape =>
                    {
                        if (shape.SortValue is null)
                        {
                            return 0 - shape.TotalChildrenCount();
                        }

                        return shape.SortValue;
                    }).ThenBy(shape => shape.ShapeText);

            var children = orderedChildren.ToList();

            foreach (var child in children.Where(child => child.Children.Count > 0))
            {
                this.SortChildren(child, internalMaxRight);
            }

            double maxLine;
            if (children.Count == (diagramShape.TotalChildrenCount() - 1))
            {
                maxLine = Math.Round(Math.Sqrt(children.Count), MidpointRounding.ToPositiveInfinity);
                var drawLines = children.Count / maxLine;
                var appConfigMaxBoxLines = this.AppConfig.MaxBoxLines ?? 5d;
                if (drawLines > appConfigMaxBoxLines)
                {
                    maxLine = Math.Round(children.Count / appConfigMaxBoxLines, MidpointRounding.ToPositiveInfinity);
                }
            }
            else
            {
                maxLine = int.MaxValue;
            }

            var lineCount = 0;
            var lines = 1;
            var currentMaxDepth = 1;

            // clear existing relationships.
            foreach (var child in children)
            {
                child.Right = null;
                child.Below = null;
            }

            for (var i = 1; i <= children.Count; i++)
            {
                // shape being placed.
                var childShape = children[i - 1];

                // are we first shape?
                if (i == 1)
                {
                    lineCount++;

                    diagramShape.CorrectDiagram();

                    if (childShape.Children.Count > 0)
                    {
                        currentMaxDepth = childShape.ChildrenDepth;
                    }

                    // skip over
                    continue;
                }

                // are we below line count?
                if (lineCount < maxLine)
                {
                    // width if placed on right.
                    var rightWidth = children[i - 2].RightSide + childShape.Width() + DiagramShape.ConvertMeasurement(this.AppConfig.HorizontalSpacing);

                    // can we place on right
                    if (rightWidth < internalMaxRight)
                    {
                        children[i - 2].Right = childShape;
                        lineCount++;

                        diagramShape.CorrectDiagram();

                        if (childShape.Children.Count > 0 && childShape.ChildrenDepth < currentMaxDepth)
                        {
                            SortChildrenByLines(childShape, currentMaxDepth);
                        }

                        continue;
                    }
                }

                // find start of line.
                var shape = children[i - 2];
                while (shape.Left is not null)
                {
                    shape = shape.Left;
                }

                // are we relating to ourself?
                if (shape == childShape)
                {
                    lineCount++;

                    diagramShape.CorrectDiagram();
                    continue;
                }

                shape.Below = childShape;
                lineCount = 1;
                lines++;

                if (childShape.Children.Count > 0)
                {
                    currentMaxDepth = childShape.ChildrenDepth;
                }

                diagramShape.CorrectDiagram();
            }

            diagramShape.ChildrenDepth = lines;

            diagramShape.FindNeighbours();

            diagramShape.CorrectDiagram();
        }

        private static void SortChildrenByLines(DiagramShape diagramShape, int drawLines)
        {
            var orderedChildren = diagramShape.Children.OrderBy<DiagramShape, object>(
                shape =>
                    {
                        if (shape.SortValue is null)
                        {
                            return 0 - shape.TotalChildrenCount();
                        }

                        return shape.SortValue;
                    }).ThenBy(shape => shape.ShapeText);
            var children = orderedChildren.ToList();

            // clear existing relationships.
            foreach (var child in children)
            {
                child.Right = null;
                child.Below = null;
            }

            var lineCount = 0;
            var lines = 1;
            var maxLine = Math.Round(children.Count / (double)drawLines, MidpointRounding.ToPositiveInfinity);

            for (var i = 1; i <= children.Count; i++)
            {
                // shape being placed.
                var childShape = children[i - 1];

                // are we first shape?
                if (i == 1)
                {
                    lineCount++;

                    diagramShape.CorrectDiagram();

                    // skip over
                    continue;
                }

                // are we below line count?
                if (lineCount < maxLine)
                {
                    children[i - 2].Right = childShape;
                    lineCount++;

                    diagramShape.CorrectDiagram();

                    continue;
                }

                // find start of line.
                var shape = children[i - 2];
                while (shape.Left is not null)
                {
                    shape = shape.Left;
                }

                // are we relating to ourself?
                if (shape == childShape)
                {
                    lineCount++;

                    diagramShape.CorrectDiagram();
                    continue;
                }

                shape.Below = childShape;
                lineCount = 1;
                lines++;

                diagramShape.CorrectDiagram();
            }

            diagramShape.ChildrenDepth = lines;

            diagramShape.FindNeighbours();

            diagramShape.CorrectDiagram();
        }
    }
}
