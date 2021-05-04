// -----------------------------------------------------------------------
// <copyright file="ExcelApplication.cs" company="Jolyon Suthers">
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
    using System.Runtime.InteropServices;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Office.Interop.Excel;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models;
    using VisioCleanup.Core.Models.Config;

    using Marshal = VisioCleanup.Core.Marshal;

    /// <inheritdoc />
    internal class ExcelApplication : IExcelApplication
    {
        private readonly AppConfig appConfig;

        private readonly ILogger<ExcelApplication> logger;

        private Application? excelApplication;

        /// <summary>Initialises a new instance of the <see cref="ExcelApplication" /> class.</summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="options">Application configuration settings.</param>
        public ExcelApplication(ILogger<ExcelApplication> logger, IOptions<AppConfig> options)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.appConfig = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        public void Close()
        {
            this.logger.LogDebug("Releasing excel application");
            this.excelApplication = null;
        }

        /// <inheritdoc />
        public void Open()
        {
            this.logger.LogDebug("Opening connection to excel");
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
        public IEnumerable<DiagramShape> RetrieveRecords()
        {
            if (this.excelApplication?.ActiveSheet is null)
            {
                throw new InvalidOperationException("Excel must be running.");
            }

            Worksheet excelApplicationActiveSheet = this.excelApplication.ActiveSheet as Worksheet;
            if (excelApplicationActiveSheet.ListObjects.Count == 0)
            {
                throw new InvalidOperationException("Excel not setup correctly.");
            }

            var dataTable = excelApplicationActiveSheet.ListObjects[1];
            Dictionary<string, DiagramShape> allShapes = new();

            // find headers
            var columnMapping = this.FindHeaders(dataTable);

            // process rows
            var rows = dataTable.DataBodyRange.Rows;
            this.logger.LogDebug("getting values");
            object[,] data = rows.Value;
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

                DiagramShape? result = null;
                foreach (var i in Enumerable.Range(1, columnMapping.Count))
                {
                    result = this.CreateShape(rowResults[i], allShapes, result);
                }
            }

            Collection<DiagramShape> shapes = new();
            foreach (var (_, value) in allShapes)
            {
                shapes.Add(value);
            }

            return shapes;
        }

        private DiagramShape? CreateShape(IReadOnlyDictionary<FieldType, string> rowResult, IDictionary<string, DiagramShape> allShapes, DiagramShape? previousShape)
        {
            var shapeType = rowResult.ContainsKey(FieldType.ShapeType) ? rowResult[FieldType.ShapeType] : string.Empty;
            var sortValue = rowResult.ContainsKey(FieldType.SortValue) ? rowResult[FieldType.SortValue] : null;
            var shapeText = rowResult.ContainsKey(FieldType.ShapeText) ? rowResult[FieldType.ShapeText] : string.Empty;

            if (string.IsNullOrEmpty(shapeText))
            {
                return previousShape;
            }

            var shapeIdentifier = $"{previousShape?.ShapeIdentifier} {shapeText}:{shapeType}".Trim();

            if (!allShapes.ContainsKey(shapeIdentifier))
            {
                this.logger.LogDebug("Creating shape for: {ShapeText}", shapeText);
                allShapes.Add(
                    shapeIdentifier,
                    new DiagramShape(0)
                        {
                            ShapeText = shapeText,
                            ShapeType = ShapeType.NewShape,
                            SortValue = sortValue,
                            Master = shapeType,
                            ShapeIdentifier = shapeIdentifier,
                        });
            }

            var shape = allShapes[shapeIdentifier];

            previousShape?.AddChildShape(shape);
            return shape;
        }

        private SortedList<int, Dictionary<FieldType, int>> FindHeaders(ListObject dataTable)
        {
            var columnMapping = new SortedList<int, Dictionary<FieldType, int>>();
            var level = 0;
            object[,] header = dataTable.HeaderRowRange.Value;
            do
            {
                var mappings = new Dictionary<FieldType, int>();
                var fieldName = string.Format(this.appConfig.FieldLabelFormat ?? "{0}", level);
                var sortFieldName = string.Format(this.appConfig.SortFieldLabelFormat ?? "{0} SortValue", level);
                var shapeFieldName = string.Format(this.appConfig.ShapeTypeLabelFormat ?? "{0} Shape", level);

                level++;
                for (var i = 1; i <= header.GetLength(1); i++)
                {
                    var value = header.GetValue(1, i);

                    if (value?.Equals(fieldName) == true)
                    {
                        mappings[FieldType.ShapeText] = i;
                    }

                    if (value?.Equals(sortFieldName) == true)
                    {
                        mappings[FieldType.SortValue] = i;
                    }

                    if (value?.Equals(shapeFieldName) == true)
                    {
                        mappings[FieldType.ShapeType] = i;
                    }
                }

                if (mappings.ContainsKey(FieldType.ShapeText))
                {
                    columnMapping.Add(level, mappings);
                }
            }
            while (columnMapping.ContainsKey(level));

            return columnMapping;
        }
    }
}
