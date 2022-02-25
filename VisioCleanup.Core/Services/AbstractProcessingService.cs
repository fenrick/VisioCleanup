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
    public ICollection<DiagramShape> AllShapes { get; } = new Collection<DiagramShape>();

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
            this.VisioApplication.VisualChanges(state: false);
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
                PositionX = this.VisioApplication.PageLeftSide,
                PositionY = this.VisioApplication.PageTopSide - DiagramShape.ConvertMeasurement(this.AppConfig.HeaderHeight),
                Width = this.VisioApplication.PageRightSide - this.VisioApplication.PageLeftSide - DiagramShape.ConvertMeasurement(this.AppConfig.SidePanelWidth),
                HasCalculatedSortValue = false,
            };
            this.MasterShape.MaxRight = this.MasterShape.Width + this.MasterShape.PositionX;

            // retrieve records
            this.Logger.LogInformation("Loading {DataSource} data", dataSource.Name);

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
            this.MasterShape.SortChildren();
        }
        finally
        {
            this.Logger.LogInformation("Closing connection to data source");
            dataSource.Close();

            this.Logger.LogInformation("Closing connection to visio");
            this.VisioApplication.Close();
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

    private void PopulateAllShapes(DiagramShape diagramShape)
    {
        this.AllShapes.Add(diagramShape);

        foreach (var child in diagramShape.Children.Values)
        {
            this.PopulateAllShapes(child);
        }
    }
}
