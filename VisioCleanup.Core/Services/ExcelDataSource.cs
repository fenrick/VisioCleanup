// -----------------------------------------------------------------------
// <copyright file="ExcelDataSource.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services;

using System.Globalization;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Office.Interop.Excel;

using VisioCleanup.Core.Contracts;
using VisioCleanup.Core.Models;
using VisioCleanup.Core.Models.Config;

using Marshal = VisioCleanup.Core.Marshal;

/// <inheritdoc cref="IExcelDataSource" />
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
    public void RetrieveRecords(string parameter, DiagramShape masterShape)
    {
        if (masterShape is null)
        {
            throw new ArgumentNullException(nameof(masterShape));
        }

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
        Dictionary<string, DiagramShape> allShapes = new(StringComparer.OrdinalIgnoreCase);

        // find headers
        var columnMapping = this.FindHeaders(dataTable);

        // process rows
        var rows = dataTable.DataBodyRange.Rows;
        this.Logger.LogDebug("getting values");

        if (rows.Value is not object[,] data)
        {
            return;
        }

        foreach (var rowNumber in Enumerable.Range(1, data.GetLength(0)))
        {
            var rowResults = ConvertRowToValues(data, columnMapping, rowNumber);

            DiagramShape? result = null;
            for (var i = 0; i < columnMapping.Length; i++)
            {
                result = this.CreateShape(rowResults[i], allShapes, result);

                if (result is not null && result.ParentShape is null)
                {
                    masterShape.AddChildShape(result);
                }
            }
        }
    }

    private static Dictionary<FieldType, string>[] ConvertRowToValues(object[,] data, Dictionary<FieldType, int>[] columnMapping, int rowNumber)
    {
        var rowResults = new Dictionary<FieldType, string>[columnMapping.Length];
        for (var cellIndex = 0; cellIndex < columnMapping.Length; cellIndex++)
        {
            var columnMap = columnMapping[cellIndex];
            Dictionary<FieldType, string> values = new();

            foreach (var (key, value) in columnMap)
            {
                values[key] = data.GetValue(rowNumber, value)?.ToString() ?? string.Empty;
            }

            rowResults[cellIndex] = values;
        }

        return rowResults;
    }

    private Dictionary<FieldType, int>[] FindHeaders(ListObject dataTable)
    {
        var level = -1;
        var header = dataTable.HeaderRowRange.Value as object[,];
        var columnMapping = Array.Empty<Dictionary<FieldType, int>>();

        if (this.AppConfig.FieldLabelFormat is null || this.AppConfig.SortFieldLabelFormat is null || this.AppConfig.ShapeTypeLabelFormat is null)
        {
            throw new InvalidOperationException("Excel parameters not set in configuration.");
        }

        do
        {
            level++;

            var mappings = new Dictionary<FieldType, int>();
            var fieldName = string.Format(CultureInfo.CurrentCulture, this.AppConfig.FieldLabelFormat, level);
            var sortFieldName = string.Format(CultureInfo.CurrentCulture, this.AppConfig.SortFieldLabelFormat, level);
            var shapeFieldName = string.Format(CultureInfo.CurrentCulture, this.AppConfig.ShapeTypeLabelFormat, level);

            Array.Resize(ref columnMapping, level + 1);
            for (var i = 1; i <= header?.GetLength(1); i++)
            {
                var value = header.GetValue(1, i);

                if (value!.Equals(fieldName))
                {
                    mappings[FieldType.ShapeText] = i;
                    columnMapping[level] = mappings;
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
        while (columnMapping.ElementAtOrDefault(level) is not null);

        Array.Resize(ref columnMapping, level);

        return columnMapping;
    }
}
