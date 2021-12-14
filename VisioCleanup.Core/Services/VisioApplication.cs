// -----------------------------------------------------------------------
// <copyright file="VisioApplication.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Visio;

using VisioCleanup.Core.Contracts;
using VisioCleanup.Core.Models;

using Marshal = VisioCleanup.Core.Marshal;

/// <inheritdoc />
public class VisioApplication : IVisioApplication
{
    private const string SystemNotInitialised = "System not initialised.";

    private readonly ILogger<VisioApplication> logger;

    private readonly ConcurrentDictionary<int, IVShape> shapeCache = new();

    private readonly ConcurrentDictionary<string, IVMaster?> stencilCache = new(StringComparer.Ordinal);

    private IVPage? activePage;

    private Application? visioApplication;

    /// <summary>Initialises a new instance of the <see cref="VisioApplication" /> class.</summary>
    /// <param name="logger">Logging instance.</param>
    public VisioApplication(ILogger<VisioApplication> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.activePage = null;
        this.visioApplication = null;
    }

    private enum VisioFields
    {
        SheetId,

        Cell,

        Result,
    }

    /// <inheritdoc />
    public int PageLeftSide
    {
        get
        {
            if (this.visioApplication is null || this.activePage is null)
            {
                throw new InvalidOperationException(SystemNotInitialised);
            }

            var pageSheet = this.activePage.PageSheet;

            return DiagramShape.ConvertMeasurement(
                GetCellValue(pageSheet, VisSectionIndices.visSectionObject, VisRowIndices.visRowPrintProperties, VisCellIndices.visPrintPropertiesLeftMargin));
        }
    }

    /// <inheritdoc />
    public int PageRightSide
    {
        get
        {
            if (this.visioApplication is null || this.activePage is null)
            {
                throw new InvalidOperationException(SystemNotInitialised);
            }

            var pageSheet = this.activePage.PageSheet;

            var rightMargin = GetCellValue(pageSheet, VisSectionIndices.visSectionObject, VisRowIndices.visRowPrintProperties, VisCellIndices.visPrintPropertiesRightMargin);
            var width = GetCellValue(pageSheet, VisSectionIndices.visSectionObject, VisRowIndices.visRowPage, VisCellIndices.visPageWidth);

            return DiagramShape.ConvertMeasurement(width - rightMargin);
        }
    }

    /// <inheritdoc />
    public int PageTopSide
    {
        get
        {
            if (this.visioApplication is null || this.activePage is null)
            {
                throw new InvalidOperationException(SystemNotInitialised);
            }

            var pageSheet = this.activePage.PageSheet;

            var height = GetCellValue(pageSheet, VisSectionIndices.visSectionObject, VisRowIndices.visRowPage, VisCellIndices.visPageHeight);
            var topMargin = GetCellValue(pageSheet, VisSectionIndices.visSectionObject, VisRowIndices.visRowPage, VisCellIndices.visPrintPropertiesTopMargin);

            return DiagramShape.ConvertMeasurement(height - topMargin);
        }
    }

    /// <inheritdoc />
    public void Close()
    {
        this.logger.LogDebug("Clearning shape cache");
        this.shapeCache.Clear();

        this.logger.LogDebug("Clearing stencil cache");
        this.stencilCache.Clear();

        this.logger.LogDebug("Releasing active page");
        this.activePage = null;

        this.logger.LogDebug("Releasing visio application");
        this.visioApplication = null;
    }

    /// <inheritdoc />
    public void CreateShape(DiagramShape diagramShape)
    {
        if (diagramShape is null)
        {
            throw new ArgumentNullException(nameof(diagramShape));
        }

        var newLocPinX = DiagramShape.ConvertMeasurement(diagramShape.Width() / 2);
        var newLocPinY = DiagramShape.ConvertMeasurement(diagramShape.Height() / 2);
        var newPinX = DiagramShape.ConvertMeasurement(diagramShape.LeftSide) + newLocPinX;
        var newPinY = DiagramShape.ConvertMeasurement(diagramShape.BaseSide) + newLocPinY;

        var shapeMaster = "Rectangle";

        if (!string.IsNullOrEmpty(diagramShape.Master))
        {
            shapeMaster = diagramShape.Master;
        }

        var master = this.stencilCache.GetOrAdd(shapeMaster, this.StencilValueFactory);

        if (master is null)
        {
            this.logger.LogError("Unable to find matching stencil: {StencilName}", shapeMaster);
            throw new InvalidOperationException("Unable to find matching stencil.");
        }

        if (this.visioApplication is null)
        {
            throw new InvalidOperationException(SystemNotInitialised);
        }

        var shape = this.visioApplication.ActivePage.Drop(master, newPinX, newPinY);

        diagramShape.VisioId = shape.ID;
        diagramShape.ShapeType = ShapeType.Existing;

        var visioShape = this.GetShape(diagramShape.VisioId);
        visioShape.Text = diagramShape.ShapeText;

        this.UpdateShape(diagramShape);
    }

    /// <inheritdoc />
    public void Open()
    {
        try
        {
            this.logger.LogDebug("Opening connection to visio");
            this.visioApplication = Marshal.GetActiveObject("Visio.Application") as Application ?? throw new InvalidOperationException("Visio must be running.");
            this.activePage = this.visioApplication.ActivePage;

            this.stencilCache.Clear();
            this.shapeCache.Clear();
        }
        catch (COMException)
        {
            throw new InvalidOperationException();
        }
    }

    /// <inheritdoc />
    public IEnumerable<DiagramShape> RetrieveShapes()
    {
        if (this.visioApplication?.ActiveWindow is null)
        {
            throw new InvalidOperationException(SystemNotInitialised);
        }

        Dictionary<int, DiagramShape> allShapes = new();

        // get selection.
        var selection = this.visioApplication.ActiveWindow.Selection;
        this.LoadShapeCache();

        this.logger.LogDebug("Found {Count} selected shapes", selection.Count);
        foreach (var selected in selection.Cast<Shape>().Where(selected => selected is not null))
        {
            this.logger.LogDebug("Processing shape: {ShapeID} - {ShapeText}", selected.ID, selected.Text);

            var sheetId = selected.ID;
            if (allShapes.ContainsKey(sheetId))
            {
                this.logger.LogDebug("Shape already processed");
                continue;
            }

            // create new shape.
            DiagramShape diagramShape = new(sheetId)
            {
                ShapeText = selected.Text,
                ShapeType = ShapeType.Existing,
                LeftSide = this.CalculateLeftSide(sheetId),
                RightSide = this.CalculateRightSide(sheetId),
                TopSide = this.CalculateTopSide(sheetId),
                BaseSide = this.CalculateBaseSide(sheetId),
            };
            this.logger.LogDebug("Adding shape to collection");
            allShapes.Add(sheetId, diagramShape);
        }

        this.logger.LogDebug("Processed a total of {Count} shapes", allShapes.Count);

        // generate final collection
        Collection<DiagramShape> shapes = new();

        foreach (var value in allShapes.Values)
        {
            shapes.Add(value);
        }

        return shapes;
    }

    /// <inheritdoc />
    public void UpdateShape(DiagramShape diagramShape)
    {
        if (this.visioApplication is null)
        {
            throw new InvalidOperationException(SystemNotInitialised);
        }

        if (diagramShape is null)
        {
            throw new ArgumentNullException(nameof(diagramShape));
        }

        var newLocPinX = DiagramShape.ConvertMeasurement(diagramShape.Width() / 2);
        var newLocPinY = DiagramShape.ConvertMeasurement(diagramShape.Height() / 2);
        var newPinX = DiagramShape.ConvertMeasurement(diagramShape.LeftSide) + newLocPinX;
        var newPinY = DiagramShape.ConvertMeasurement(diagramShape.BaseSide) + newLocPinY;

        var width = DiagramShape.ConvertMeasurement(diagramShape.Width());
        var height = DiagramShape.ConvertMeasurement(diagramShape.Height());

        var updates = new[]
        {
            CreateUpdateObject((short)VisCellIndices.visXFormWidth, width),
            CreateUpdateObject((short)VisCellIndices.visXFormHeight, height),
            CreateUpdateObject((short)VisCellIndices.visXFormPinX, newPinX),
            CreateUpdateObject((short)VisCellIndices.visXFormPinY, newPinY),
        };

        this.ExecuteVisioShapeUpdate(diagramShape, updates);
    }

    /// <exception cref="System.InvalidOperationException">System not initialised.</exception>
    /// <inheritdoc />
    public void VisualChanges(bool state)
    {
        if (this.visioApplication is null)
        {
            throw new InvalidOperationException(SystemNotInitialised);
        }

        this.visioApplication.ShowChanges = state;
        this.visioApplication.UndoEnabled = state;
        this.visioApplication.ScreenUpdating = state ? (short)1 : (short)0;
        this.visioApplication.DeferRecalc = state ? (short)1 : (short)0;
    }

    private static object[] CreateUpdateObject(short cell, double newValue)
    {
        var result = new object[2];
        result[(int)VisioFields.Cell] = cell;
        result[(int)VisioFields.Result] = newValue;
        return result;
    }

    private static double GetCellValue(IVShape shape, VisSectionIndices sectionIndex, VisRowIndices rowIndex, VisCellIndices cellIndex)
    {
        var shapeCell = shape.CellsSRC[(short)sectionIndex, (short)rowIndex, (short)cellIndex];
        return shapeCell.Result[VisUnitCodes.visMillimeters];
    }

    private void ExecuteVisioShapeUpdate(DiagramShape diagramShape, object[][] updates)
    {
        // MAP THE REQUEST TO THE STRUCTURES VISIO EXPECTS
        const int srcStreamFields = 3;
        var srcStream = new short[updates.Length * srcStreamFields];
        var unitsArray = new object[updates.Length];
        var resultsArray = new object[updates.Length];
        for (var i = 0; i < updates.Length; i++)
        {
            var item = updates[i];
            var srcStreamTracker = 0;

            srcStream[(i * srcStreamFields) + srcStreamTracker] = Convert.ToInt16((short)VisSectionIndices.visSectionObject, CultureInfo.InvariantCulture);
            srcStreamTracker++;
            srcStream[(i * srcStreamFields) + srcStreamTracker] = Convert.ToInt16((short)VisRowIndices.visRowXFormOut, CultureInfo.InvariantCulture);
            srcStreamTracker++;
            srcStream[(i * srcStreamFields) + srcStreamTracker] = Convert.ToInt16(item[(int)VisioFields.Cell], CultureInfo.InvariantCulture);
            resultsArray[i] = item[(int)VisioFields.Result];
            unitsArray[i] = VisUnitCodes.visMillimeters;
        }

        // EXECUTE THE REQUEST
        const short flags = 0;
        try
        {
            this.GetShape(diagramShape.VisioId).SetResults(srcStream, unitsArray, resultsArray, flags);
        }
        catch (COMException e)
        {
            this.logger.LogError(e, "Error occured during updating {Shape}", diagramShape);
        }
    }

    private int CalculateBaseSide(int visioId)
    {
        var shape = this.GetShape(visioId);

        var pinY = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormPinY);
        var locPinY = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormLocPinY);

        return DiagramShape.ConvertMeasurement(pinY - locPinY);
    }

    private int CalculateLeftSide(int visioId)
    {
        var shape = this.GetShape(visioId);

        var pinX = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormPinX);
        var locPinX = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormLocPinX);

        return DiagramShape.ConvertMeasurement(pinX - locPinX);
    }

    private int CalculateRightSide(int visioId)
    {
        var shape = this.GetShape(visioId);

        var pinX = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormPinX);
        var locPinX = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormLocPinX);
        var width = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormWidth);

        return DiagramShape.ConvertMeasurement((pinX - locPinX) + width);
    }

    private int CalculateTopSide(int visioId)
    {
        var shape = this.GetShape(visioId);

        var pinY = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormPinY);
        var locPinY = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormLocPinY);
        var height = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormHeight);

        return DiagramShape.ConvertMeasurement((pinY - locPinY) + height);
    }

    private IVShape GetShape(int visioId)
    {
        if (this.visioApplication is null || this.activePage is null)
        {
            throw new InvalidOperationException(SystemNotInitialised);
        }

        return this.shapeCache.GetOrAdd(visioId, this.activePage.Shapes.ItemFromID[visioId]);
    }

    private void LoadShapeCache()
    {
        if (this.visioApplication is null || this.activePage is null)
        {
            throw new InvalidOperationException("Initialise system first");
        }

        // clear cache
        this.shapeCache.Clear();

        var shapes = this.activePage.Shapes;
        foreach (var element in shapes)
        {
            if (element is not Shape shape)
            {
                continue;
            }

            this.shapeCache.TryAdd(shape.ID, shape);
        }
    }

    private IVMaster? StencilValueFactory(string key)
    {
        if (this.visioApplication is null)
        {
            throw new InvalidOperationException("Initialise system first.");
        }

        var documentStencil = this.visioApplication.ActiveDocument.Masters;
        if (documentStencil.Count > 0)
        {
            documentStencil.GetNames(out var masterNames);
            if (masterNames is string[] strings)
            {
                var result = strings.Contains(key, StringComparer.Ordinal);
                if (result)
                {
                    return documentStencil[key];
                }
            }
        }

        this.visioApplication.ActiveWindow.DockedStencils(out var stencilNames);
        if (stencilNames is null)
        {
            return null;
        }

        foreach (var masters in stencilNames.Cast<string>().Where(stencilName => !string.IsNullOrEmpty(stencilName))
                     .Select(stencilName => this.visioApplication.Documents[stencilName].Masters))
        {
            masters.GetNames(out var names);
            if (names is not string[] nameArray)
            {
                continue;
            }

            var result = nameArray.Contains(key, StringComparer.Ordinal);
            if (result)
            {
                return masters[key];
            }
        }

        return null;
    }
}
