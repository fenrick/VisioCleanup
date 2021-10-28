// -----------------------------------------------------------------------
// <copyright file="ExcelDataSource.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Office.Interop.Excel;

using VisioCleanup.Core.Contracts;
using VisioCleanup.Core.Models;
using VisioCleanup.Core.Models.Config;

using Marshal = VisioCleanup.Core.Marshal;

/// <inheritdoc />
public class ExcelDataSource : AbstractDataSource, IExcelDataSource
{
    private Application? excelApplication;

    /// <summary>Initialises a new instance of the <see cref="ExcelDataSource" /> class.</summary>
    /// <param name="logger">Logging instance.</param>
    /// <param name="options">Application configuration settings.</param>
    public ExcelDataSource(ILogger<ExcelDataSource> logger, IOptions<AppConfig> options)
        : base(logger, options)
    {
    }

    /// <inheritdoc />
    public string Name => "Excel";

    /// <inheritdoc />
    public void Close()
    {
        this.Logger.LogDebug("Releasing excel application");
        this.excelApplication = null;
    }

    /// <inheritdoc />
    public void Open()
    {
        this.Logger.LogDebug("Opening connection to excel");
        try
        {
            this.excelApplication = Marshal.GetActiveObject("Excel.Application") as Application ?? throw new InvalidOperationException("Excel must be running.");
        }
        catch (COMException)
        {
            throw new InvalidOperationException("Excel must be running.");
        }
    }

    /// <inheritdoc />
    public IEnumerable<DiagramShape> RetrieveRecords(string parameter)
    {
        if (this.excelApplication?.ActiveSheet is null)
        {
            throw new InvalidOperationException("Excel must be running.");
        }

        var excelApplicationActiveSheet = this.excelApplication!.ActiveSheet as Worksheet;
        if (excelApplicationActiveSheet!.ListObjects.Count == 0)
        {
            throw new InvalidOperationException("Excel not setup correctly.");
        }

        var dataTable = excelApplicationActiveSheet.ListObjects[1];
        Dictionary<string, DiagramShape> allShapes = new();

        // find headers
        var columnMapping = this.FindHeaders(dataTable);

        // process rows
        var rows = dataTable.DataBodyRange.Rows;
        this.Logger.LogDebug("getting values");
        var data = rows.Value as object[,];
        foreach (var rowNumber in Enumerable.Range(1, data.GetLength(0)))
        {
            Dictionary<int, Dictionary<FieldType, string>> rowResults = new();
            foreach (var cellIndex in Enumerable.Range(1, columnMapping.Count))
            {
                Dictionary<FieldType, string> values = new();

                foreach (var (key, value) in columnMapping[cellIndex])
                {
                    values[key] = data.GetValue(rowNumber, value)?.ToString() ?? string.Empty;
                }

                rowResults.Add(cellIndex, values);
            }

            var result = Enumerable.Range(1, columnMapping.Count).Aggregate<int, DiagramShape?>(seed: null, (current, i) => this.CreateShape(rowResults[i], allShapes, current));
        }

        Collection<DiagramShape> shapes = new();
        foreach (var value in allShapes.Values)
        {
            shapes.Add(value);
        }

        return shapes;
    }

    private SortedList<int, Dictionary<FieldType, int>> FindHeaders(ListObject dataTable)
    {
        var columnMapping = new SortedList<int, Dictionary<FieldType, int>>();
        var level = 0;
        var header = dataTable.HeaderRowRange.Value as object[,];

        if (this.AppConfig.FieldLabelFormat is null || this.AppConfig.SortFieldLabelFormat is null || this.AppConfig.ShapeTypeLabelFormat is null)
        {
            throw new InvalidOperationException("Excel parameters not set in configuration.");
        }

        do
        {
            var mappings = new Dictionary<FieldType, int>();
            var fieldName = string.Format(CultureInfo.CurrentCulture, this.AppConfig.FieldLabelFormat, level);
            var sortFieldName = string.Format(CultureInfo.CurrentCulture, this.AppConfig.SortFieldLabelFormat, level);
            var shapeFieldName = string.Format(CultureInfo.CurrentCulture, this.AppConfig.ShapeTypeLabelFormat, level);

            level++;
            for (var i = 1; i <= header?.GetLength(1); i++)
            {
                var value = header.GetValue(1, i);

                if (value!.Equals(fieldName))
                {
                    mappings[FieldType.ShapeText] = i;
                    columnMapping.Add(level, mappings);
                }
                else if (value.Equals(sortFieldName))
                {
                    mappings[FieldType.SortValue] = i;
                }
                else if (value.Equals(shapeFieldName))
                {
                    mappings[FieldType.ShapeType] = i;
                }
                else
                {
                    // ignoring value
                }
            }
        }
        while (columnMapping.ContainsKey(level));

        return columnMapping;
    }
}
