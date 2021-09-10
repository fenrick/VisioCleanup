// -----------------------------------------------------------------------
// <copyright file="AbstractProcessingService.cs" company="Jolyon Suthers">
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
    using VisioCleanup.Core.Resources;

    /// <inheritdoc />
    /// <summary>Abstract implementation of common code for processing services.</summary>
    public abstract class AbstractProcessingService : IProcessingService
    {
        private const int MaxCorrectRuns = 10;

        /// <summary>Store for converted app config right measure.</summary>
        protected int convertedAppConfigRight;

        /// <summary>Initialises a new instance of the <see cref="AbstractProcessingService" /> class.</summary>
        /// <param name="logger">Logger.</param>
        /// <param name="options">Application configuration being passed in.</param>
        /// <param name="visioApplication">Visio Application engine.</param>
        protected AbstractProcessingService(ILogger logger, IOptions<AppConfig> options, IVisioApplication visioApplication)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.VisioApplication = visioApplication ?? throw new ArgumentNullException(nameof(visioApplication));

            this.AppConfig = options.Value;

            // setup DiagramShape
            DiagramShape.AppConfig = this.AppConfig;
        }

        /// <inheritdoc />
        public Collection<DiagramShape> AllShapes { get; protected set; } = new();

        /// <inheritdoc />
        public DiagramShape? MasterShape { get; protected set; }

        /// <summary>Gets application configuration.</summary>
        /// <value>Configuration.</value>
        protected AppConfig AppConfig { get; }

        /// <summary>Gets logging environment.</summary>
        /// <value>Logger.</value>
        protected ILogger Logger { get; }

        /// <summary>Gets visio processing engine.</summary>
        /// <value>Visio execution environment.</value>
        protected IVisioApplication VisioApplication { get; }

        /// <inheritdoc />
        public Task LayoutDataSet()
        {
            return Task.Run(
                () =>
                    {
                        for (var counter = 1; counter <= MaxCorrectRuns; counter++)
                        {
                            this.Logger.LogInformation(en_AU.AbstractProcessingService_LayoutDataSet_Correcting_diagram__pass__Count_, counter);

                            if (this.MasterShape!.CorrectDiagram())
                            {
                                return;
                            }
                        }
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

        /// <summary>Internal process data sets.</summary>
        /// <param name="dataSource">Data source to process.</param>
        /// <param name="parameters">Parameters for process.</param>
        /// <returns>Task tracking progress.</returns>
        protected Task ProcessDataSetInternal(IDataSource dataSource, string parameters)
        {
            return Task.Run(
                () =>
                    {
                        try
                        {
                            // open connection to excel
                            dataSource.Open();

                            // open connection to visio
                            this.VisioApplication.Open();
                            List<DiagramShape> shapes = new();

                            // master shape
                            this.Logger.LogInformation("Create a fake parent shape");
                            this.MasterShape = new DiagramShape(0)
                                                   {
                                                       ShapeText = "FAKE MASTER",
                                                       ShapeType = ShapeType.FakeShape,
                                                       LeftSide = this.VisioApplication.PageLeftSide,
                                                       TopSide = this.VisioApplication.PageTopSide - DiagramShape.ConvertMeasurement(this.AppConfig.HeaderHeight),
                                                   };
                            shapes.Add(this.MasterShape);

                            var maxRight = this.VisioApplication.PageRightSide - DiagramShape.ConvertMeasurement(this.AppConfig.SidePanelWidth);
                            this.convertedAppConfigRight = DiagramShape.ConvertMeasurement(this.AppConfig.Right);

                            // retrieve records
                            this.Logger.LogInformation("Loading {dataSource} data", dataSource.Name);
                            shapes.AddRange(dataSource.RetrieveRecords(parameters));

                            if (shapes.Count == 1)
                            {
                                return;
                            }

                            this.AllShapes = new Collection<DiagramShape>(shapes);

                            this.Logger.LogInformation("Assigning fake parent");
                            foreach (var shape in this.AllShapes.Where(shape => !shape.HasParent() && (shape.ShapeType != ShapeType.FakeShape)))
                            {
                                this.MasterShape.AddChildShape(shape);
                            }

                            // need to set children relationships.
                            this.Logger.LogInformation("Sorting shapes into lines");
                            this.SortChildren(this.MasterShape, maxRight);
                        }
                        finally
                        {
                            this.Logger.LogInformation("Closing connection to visio");
                            this.VisioApplication.Close();

                            this.Logger.LogInformation("Closing connection to excel");
                            dataSource.Close();
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
                var leftByTwo = i - 1 - 1;
                if (lineCount < maxLine)
                {
                    // width if placed on right.
                    var rightWidth = children[leftByTwo].RightSide + childShape.Width() + DiagramShape.ConvertMeasurement(this.AppConfig.HorizontalSpacing);

                    // can we place on right
                    if (rightWidth < internalMaxRight)
                    {
                        children[leftByTwo].Right = childShape;
                        lineCount++;

                        diagramShape.CorrectDiagram();

                        if ((childShape.Children.Count > 0) && (childShape.ChildrenDepth < currentMaxDepth))
                        {
                            SortChildrenByLines(childShape, currentMaxDepth);
                        }

                        continue;
                    }
                }

                // find start of line.
                var shape = children[leftByTwo];
                while (shape.Left is not null)
                {
                    shape = shape.Left;
                }

                // are we relating to our self?
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
                    children[i - 1 - 1].Right = childShape;
                    lineCount++;

                    diagramShape.CorrectDiagram();

                    continue;
                }

                // find start of line.
                var shape = children[i - 1 - 1];
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
