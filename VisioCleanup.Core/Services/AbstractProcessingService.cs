// -----------------------------------------------------------------------
// <copyright file="AbstractProcessingService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services;

using System.Collections.ObjectModel;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using VisioCleanup.Core.Contracts;
using VisioCleanup.Core.Models;
using VisioCleanup.Core.Models.Config;

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
    public void LayoutDataSet()
    {
        for (var counter = 1; counter <= MaxCorrectRuns; counter++)
        {
            this.Logger.LogInformation("Correcting diagram: pass {Count}", counter);

            if (!this.MasterShape!.CorrectDiagram())
            {
                return;
            }
        }
    }

    /// <inheritdoc />
    public void UpdateVisio()
    {
        try
        {
            this.VisioApplication.Open();
            this.VisioApplication.VisualChanges(state: true);
            this.Logger.LogInformation("Modelling changes to visio");

            if (this.MasterShape is not null)
            {
                this.DrawShape(this.MasterShape);
            }
        }
        finally
        {
            this.Logger.LogInformation("Recalculating diagrams");
            this.VisioApplication.VisualChanges(state: true);
            this.VisioApplication.Close();
            this.Logger.LogInformation("Visio closed");
        }
    }

    /// <inheritdoc />
    public void DrawBitmapStructure() => this.MasterShape!.Bitmap();

    /// <summary>private process data sets.</summary>
    /// <param name="dataSource">Data source to process.</param>
    /// <param name="parameters">Parameters for process.</param>
    protected void ProcessDataSetInternal(IDataSource dataSource, string parameters)
    {
        if (dataSource is null)
        {
            throw new ArgumentNullException(nameof(dataSource));
        }

        try
        {
            // open connection to excel
            dataSource.Open();
            this.VisioApplication.Open();

            // master shape
            this.Logger.LogInformation("Create a fake parent shape");
            this.MasterShape = new DiagramShape(0)
            {
                ShapeText = "FAKE MASTER",
                SortValue = "FAKE MASTER",
                ShapeType = ShapeType.FakeShape,
                LeftSide = this.VisioApplication.PageLeftSide,
                TopSide = this.VisioApplication.PageTopSide - DiagramShape.ConvertMeasurement(this.AppConfig.HeaderHeight),
                RightSide = this.VisioApplication.PageRightSide - DiagramShape.ConvertMeasurement(this.AppConfig.SidePanelWidth),
            };

            // retrieve records
            this.Logger.LogInformation("Loading {dataSource} data", dataSource.Name);

            dataSource.RetrieveRecords(parameters, this.MasterShape);
            if (this.MasterShape.Children.Count == 0)
            {
                return;
            }

            // need to set children relationships.
            this.Logger.LogInformation("Creating all shapes");
            this.AllShapes.Clear();
            this.PopulateAllShapes(this.MasterShape);

            // sort
            this.SortChildren(this.MasterShape, this.MasterShape.RightSide);
        }
        finally
        {
            this.Logger.LogInformation("Closing connection to data source");
            dataSource.Close();

            this.Logger.LogInformation("Closing connection to visio");
            this.VisioApplication.Close();
        }
    }

    private static void ClearExistingRelationships(IEnumerable<DiagramShape> children)
    {
        // clear existing relationships.
        foreach (var child in children)
        {
            child.DiagramShapeRight = null;
            child.DiagramShapeBelow = null;
        }
    }

    private static void SortChildrenByLines(DiagramShape diagramShape, int drawLines)
    {
        var children = diagramShape.Children.Values;

        ClearExistingRelationships(children);

        var lineCount = 0;
        var lines = 1;
        var maxLine = Math.Round(children.Count / (double)drawLines, MidpointRounding.AwayFromZero);

        for (var i = 0; i < children.Count; i++)
        {
            // shape being placed.
            var childShape = children[i];

            // are we first shape?
            if (i == 0)
            {
                lineCount++;

                diagramShape.CorrectDiagram();

                // skip over
                continue;
            }

            // are we below line count?
            if (lineCount < maxLine)
            {
                children[i - 1].DiagramShapeRight = childShape;
                lineCount++;

                diagramShape.CorrectDiagram();

                continue;
            }

            // find start of line.
            var shape = children[i - 1];
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

            shape.DiagramShapeBelow = childShape;
            lineCount = 1;
            lines++;

            diagramShape.CorrectDiagram();
        }

        diagramShape.ChildrenDepth = lines;

        diagramShape.FindNeighbours();

        diagramShape.CorrectDiagram();
    }

    private static DiagramShape MostLeftShape(DiagramShape firstOption)
    {
        var shape = firstOption;
        while (shape.Left is not null)
        {
            shape = shape.Left;
        }

        return shape;
    }

    private void PopulateAllShapes(DiagramShape diagramShape)
    {
        this.AllShapes.Add(diagramShape);

        foreach (var child in diagramShape.Children.Values)
        {
            this.PopulateAllShapes(child);
        }
    }

    private void DrawShape(DiagramShape diagramShape)
    {
        // draw shapw
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

        // draw children
        foreach (var child in diagramShape.Children.Values)
        {
            this.DrawShape(child);
        }
    }

    /// <summary>Sort the children of the diagram shape.</summary>
    /// <param name="diagramShape">Shape that's children are to be sorted.</param>
    /// <param name="maxRight">Maximum right side.</param>
    private void SortChildren(DiagramShape diagramShape, int maxRight)
    {
        var internalMaxRight = maxRight - DiagramShape.ConvertMeasurement(this.AppConfig.Right);

        var children = diagramShape.Children.Values;

        foreach (var child in children.Where(child => child.Children.Count > 0))
        {
            this.SortChildren(child, internalMaxRight);
        }

        var maxLine = this.CalculateMaxLine(diagramShape);

        var lineCount = 0;
        var lines = 1;
        var currentMaxDepth = 1;

        ClearExistingRelationships(children);

        for (var i = 0; i < children.Count; i++)
        {
            // shape being placed.
            var childShape = children[i];

            // are we first shape?
            if (i == 0)
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
            var leftByOne = i - 1;
            if ((lineCount < maxLine) && ((children[leftByOne].RightSide + childShape.Width() + DiagramShape.ConvertMeasurement(this.AppConfig.HorizontalSpacing))
                                          < internalMaxRight))
            {
                // can we place on right
                children[leftByOne].DiagramShapeRight = childShape;
                lineCount++;

                diagramShape.CorrectDiagram();

                if ((childShape.Children.Count > 0) && (childShape.ChildrenDepth < currentMaxDepth))
                {
                    SortChildrenByLines(childShape, currentMaxDepth);
                }

                continue;
            }

            // find start of line.
            var shape = MostLeftShape(children[leftByOne]);

            // are we relating to our self?
            if (shape == childShape)
            {
                lineCount++;

                diagramShape.CorrectDiagram();
                continue;
            }

            shape.DiagramShapeBelow = childShape;
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

    private double CalculateMaxLine(DiagramShape diagramShape )
    {
        double maxLine;
        var childrenCount = diagramShape.Children.Count;
        if (childrenCount == (diagramShape.TotalChildrenCount() - 1))
        {
            maxLine = Math.Round(Math.Sqrt(childrenCount), MidpointRounding.AwayFromZero);
            var drawLines = childrenCount / maxLine;
            var appConfigMaxBoxLines = this.AppConfig.MaxBoxLines ?? 5d;
            if (drawLines > appConfigMaxBoxLines)
            {
                maxLine = Math.Round(childrenCount / appConfigMaxBoxLines, MidpointRounding.AwayFromZero);
            }
        }
        else
        {
            maxLine = int.MaxValue;
        }

        return maxLine;
    }
}
