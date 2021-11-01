// -----------------------------------------------------------------------
// <copyright file="AbstractProcessingService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services;

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

using AbstractProcessingService_Res = AbstractProcessingService_Resources;

/// <inheritdoc />
/// <summary>Abstract implementation of common code for processing services.</summary>
public class AbstractProcessingService : IProcessingService
{
    private const int MaxCorrectRuns = 10;

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
    public Collection<DiagramShape> AllShapes { get; } = new();

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
    public Task LayoutDataSetAsync()
    {
        return Task.Run(
            () =>
                {
                    for (var counter = 1; counter <= MaxCorrectRuns; counter++)
                    {
                        this.Logger.LogInformation("Correcting diagram: pass {Count}", counter);

                        if (this.MasterShape!.CorrectDiagram())
                        {
                            return;
                        }
                    }
                });
    }

    /// <inheritdoc />
    public Task UpdateVisioAsync()
    {
        return Task.Run(
            () =>
                {
                    try
                    {
                        this.VisioApplication.Open();
                        this.VisioApplication.VisualChanges(state: false);
                        this.Logger.LogInformation(AbstractProcessingService_Res.Modelling_changes_to_visio);

                        // update each shape
                        foreach (var diagramShape in this.AllShapes)
                        {
                            switch (diagramShape.ShapeType)
                            {
                                case ShapeType.NewShape:
                                    this.Logger.LogDebug(AbstractProcessingService_Res.Dropping_new_shape_Shape, diagramShape);
                                    this.VisioApplication.CreateShape(diagramShape);
                                    break;
                                case ShapeType.Existing:
                                    this.Logger.LogDebug(AbstractProcessingService_Res.Updating_shape_Shape, diagramShape);
                                    this.VisioApplication.UpdateShape(diagramShape);
                                    break;
                                case ShapeType.FakeShape:
                                    // we don't draw this!
                                    this.Logger.LogDebug(AbstractProcessingService_Res.Skipping_fake_shape_Shape, diagramShape);
                                    break;
                                default:
                                    throw new InvalidOperationException(AbstractProcessingService_Res.ShapeType_not_matched);
                            }
                        }
                    }
                    finally
                    {
                        this.Logger.LogInformation(AbstractProcessingService_Res.Recalculating_diagrams);
                        this.VisioApplication.VisualChanges(state: true);
                        this.VisioApplication.Close();
                        this.Logger.LogInformation(AbstractProcessingService_Res.Visio_closed);
                    }
                });
    }

    /// <summary>Internal process data sets.</summary>
    /// <param name="dataSource">Data source to process.</param>
    /// <param name="parameters">Parameters for process.</param>
    /// <returns>Task tracking progress.</returns>
    protected Task ProcessDataSetInternalAsync(IDataSource dataSource, string parameters)
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
                        this.Logger.LogInformation(AbstractProcessingService_Res.Create_a_fake_parent_shape);
                        this.MasterShape = new DiagramShape(0)
                        {
                            ShapeText = AbstractProcessingService_Res.FAKE_MASTER,
                            ShapeType = ShapeType.FakeShape,
                            LeftSide = this.VisioApplication.PageLeftSide,
                            TopSide = this.VisioApplication.PageTopSide - DiagramShape.ConvertMeasurement(this.AppConfig.HeaderHeight),
                        };
                        shapes.Add(this.MasterShape);

                        var maxRight = this.VisioApplication.PageRightSide - DiagramShape.ConvertMeasurement(this.AppConfig.SidePanelWidth);

                        // retrieve records
                        this.Logger.LogInformation(AbstractProcessingService_Res.Loading_dataSource_data, dataSource.Name);
                        shapes.AddRange(dataSource.RetrieveRecords(parameters));

                        if (shapes.Count == 1)
                        {
                            return;
                        }

                        this.AllShapes.Clear();
                        foreach (var diagramShape in shapes)
                        {
                            this.AllShapes.Add(diagramShape);
                        }

                        this.Logger.LogInformation(AbstractProcessingService_Res.Assigning_fake_parent);
                        foreach (var shape in this.AllShapes.Where(shape => !shape.HasParent() && (shape.ShapeType != ShapeType.FakeShape)))
                        {
                            this.MasterShape.AddChildShape(shape);
                        }

                        // need to set children relationships.
                        this.Logger.LogInformation(AbstractProcessingService_Res.Sorting_shapes_into_lines);
                        this.SortChildren(this.MasterShape, maxRight);
                    }
                    finally
                    {
                        this.Logger.LogInformation(AbstractProcessingService_Res.Closing_connection_to_visio);
                        this.VisioApplication.Close();

                        this.Logger.LogInformation(AbstractProcessingService_Res.Closing_connection_to_excel);
                        dataSource.Close();
                    }
                });
    }

    /// <summary>Sort the children of the diagram shape.</summary>
    /// <param name="diagramShape">Shape that's children are to be sorted.</param>
    /// <param name="maxRight">Maximum right side.</param>
    protected void SortChildren(DiagramShape diagramShape, int maxRight)
    {
        var internalMaxRight = maxRight - DiagramShape.ConvertMeasurement(this.AppConfig.Right);

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
            maxLine = Math.Round(Math.Sqrt(children.Count), MidpointRounding.AwayFromZero);
            var drawLines = children.Count / maxLine;
            var appConfigMaxBoxLines = this.AppConfig.MaxBoxLines ?? 5d;
            if (drawLines > appConfigMaxBoxLines)
            {
                maxLine = Math.Round(children.Count / appConfigMaxBoxLines, MidpointRounding.AwayFromZero);
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
            if ((lineCount < maxLine) && ((children[leftByTwo].RightSide + childShape.Width() + DiagramShape.ConvertMeasurement(this.AppConfig.HorizontalSpacing))
                                          < internalMaxRight))
            {
                // can we place on right
                children[leftByTwo].Right = childShape;
                lineCount++;

                diagramShape.CorrectDiagram();

                if ((childShape.Children.Count > 0) && (childShape.ChildrenDepth < currentMaxDepth))
                {
                    SortChildrenByLines(childShape, currentMaxDepth);
                }

                continue;
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
        var maxLine = Math.Round(children.Count / (double)drawLines, MidpointRounding.AwayFromZero);

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
