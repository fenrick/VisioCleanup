// -----------------------------------------------------------------------
// <copyright file="ExcelHandlerService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Office.Interop.Excel;

    using VisioCleanup.Objects;

    using Range = Microsoft.Office.Interop.Excel.Range;

    /// <summary>
    ///     Implementation of Excel handler.
    /// </summary>
    internal class ExcelHandlerService : IExcelHandler
    {
        private readonly ILogger<ExcelHandlerService> logger;

        private readonly VisioCleanupSettings settings;

        private Application? excelApplication;

        /// <summary>
        ///     Initialises a new instance of the <see cref="ExcelHandlerService" /> class.
        /// </summary>
        /// <param name="settings">Settings object.</param>
        /// <param name="logger">Logging object.</param>
        public ExcelHandlerService(IOptions<VisioCleanupSettings> settings, ILogger<ExcelHandlerService> logger)
        {
            this.logger = logger;
            this.settings = settings.Value;
        }

        /// <inheritdoc />
        /// <exception cref="T:System.NullReferenceException">excel application is <see langword="null" />.</exception>
        public void Close()
        {
            Marshal.ReleaseObject(this.excelApplication);
        }

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">Excel must be running.</exception>
        public void Open()
        {
            this.logger.LogDebug("Opening connection to excel.");
            this.excelApplication = Marshal.GetActiveObject("Excel.Application") as Application ?? throw new InvalidOperationException("Excel must be running.");
        }

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">System not initialised.</exception>
        public Dictionary<string, DiagramShape> RetrieveRecords()
        {
            if (this.excelApplication == null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            dynamic? dataTable = null;
            try
            {
                dataTable = this.excelApplication.ActiveSheet.ListObjects[1];
                var allShapes = new Dictionary<string, DiagramShape>();
                var shapeCounter = 1;

                // find headers
                SortedList<int, Dictionary<FieldType, int>> columnMapping = this.FindHeaders(dataTable);

                // process rows
                Range rows = dataTable.DataBodyRange.Rows;
                this.logger.LogDebug("getting values");
                object[,] data = rows.Value;
                foreach (var rowNumber in Enumerable.Range(1, data.GetLength(0)))
                {
                    var rowResults = new Dictionary<int, Dictionary<FieldType, string>>();
                    foreach (var cellIndex in Enumerable.Range(1, columnMapping.Count))
                    {
                        var values = new Dictionary<FieldType, string>();

                        foreach (var mapping in columnMapping[cellIndex])
                        {
                            var dataValue = data.GetValue(rowNumber, mapping.Value);
                            values[mapping.Key] = dataValue as string;
                        }

                        rowResults.Add(cellIndex, values);
                    }

                    DiagramShape? previousShape = null;
                    foreach (var cellIndex in Enumerable.Range(1, columnMapping.Count))
                    {
                        var rowResult = rowResults[cellIndex];

                        var shapeType = rowResult.ContainsKey(FieldType.Shape) ? rowResult[FieldType.Shape] : null;
                        var sortValue = rowResult.ContainsKey(FieldType.Sort) ? rowResult[FieldType.Sort] : null;
                        var shapeText = rowResult.ContainsKey(FieldType.Primary) ? rowResult[FieldType.Primary] : null;

                        if (!allShapes.ContainsKey(shapeText!))
                        {
                            this.logger.LogDebug("Creating shape for: {ShapeText}", shapeText);
                            allShapes.Add(
                                shapeText,
                                new DiagramShape(shapeCounter++)
                                    {
                                        ShapeText = shapeText, ShapeType = ShapeType.NewShape, SortValue = sortValue, Stencil = shapeType,
                                    });
                        }

                        var shape = allShapes[shapeText];

                        previousShape?.AddChildShape(shape);

                        previousShape = shape;
                    }
                }

                return allShapes;
            }
            finally
            {
                Marshal.ReleaseObject(dataTable);
                GC.Collect();
            }
        }

        private SortedList<int, Dictionary<FieldType, int>> FindHeaders(ListObject dataTable)
        {
            var columnMapping = new SortedList<int, Dictionary<FieldType, int>>();
            var level = 0;
            object[,] header = dataTable.HeaderRowRange.Value;
            do
            {
                var mappings = new Dictionary<FieldType, int>();
                var fieldName = string.Format(this.settings.ExcelFieldLabelFormat ?? "{0}", level);
                var sortFieldName = string.Format(this.settings.ExcelSortFieldLabelFormat ?? "{0} Sort", level);
                var shapeFieldName = string.Format(this.settings.ExcelShapeTypeLabelFormat ?? "{0} Shape", level);

                level++;
                for (var i = 1; i <= header.GetLength(1); i++)
                {
                    var value = header.GetValue(1, i);

                    if ((value != null) && value.Equals(fieldName))
                    {
                        mappings[FieldType.Primary] = i;
                    }

                    if ((value != null) && value.Equals(sortFieldName))
                    {
                        mappings[FieldType.Sort] = i;
                    }

                    if ((value != null) && value.Equals(shapeFieldName))
                    {
                        mappings[FieldType.Shape] = i;
                    }
                }

                if (mappings.ContainsKey(FieldType.Primary))
                {
                    columnMapping.Add(level, mappings);
                }
            }
            while (columnMapping.ContainsKey(level));

            return columnMapping;
        }
    }
}