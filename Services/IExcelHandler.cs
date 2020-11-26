// -----------------------------------------------------------------------
// <copyright file="IExcelHandler.cs" company="Jolyon Suthers">
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

    using Range = Microsoft.Office.Interop.Excel.Range;

    /// <summary>
    ///     Interface for excel handler.
    /// </summary>
    internal interface IExcelHandler
    {
        /// <summary>
        ///     Close connection to excel.
        /// </summary>
        void Close();

        /// <summary>
        ///     Open connection to excel.
        /// </summary>
        void Open();

        MyTree<string> RetrieveRecords();
    }

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
        public void Close()
        {
            Marshal.ReleaseObject(this.excelApplication);
        }

        /// <inheritdoc />
        public void Open()
        {
            this.logger.LogDebug("Opening connection to excel.");
            this.excelApplication = Marshal.GetActiveObject("Excel.Application") as Application ?? throw new InvalidOperationException();
        }

        public MyTree<string> RetrieveRecords()
        {
            if (this.excelApplication == null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            SortedList<int, int> columnMapping = new SortedList<int, int>();
            MyTree<string> root = new MyTree<string> { Value = "ROOT" };
            Worksheet? worksheet = null;
            ListObject? dataTable = null;
            try
            {
                worksheet = this.excelApplication.ActiveSheet;
                dataTable = worksheet.ListObjects[1];

                // find headers
                Range? headerRowRange = null;
                try
                {
                    headerRowRange = dataTable.HeaderRowRange;
                    foreach (Range cell in headerRowRange)
                    {
                        switch (cell.Value)
                        {
                            case "L0":
                                columnMapping.Add(1, cell.Column);
                                break;
                            case "L1":
                                columnMapping.Add(2, cell.Column);
                                break;
                            case "L2":
                                columnMapping.Add(3, cell.Column);
                                break;
                        }
                    }
                }
                finally
                {
                    Marshal.ReleaseObject(headerRowRange);
                }

                // process rows
                Range? rows = null;
                try
                {
                    rows = dataTable.DataBodyRange.Rows;
                    for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
                    {
                        Range? cells = null;
                        Dictionary<int, string> rowResults = new Dictionary<int, string>();
                        try
                        {
                            cells = rows[rowIndex];

                            // process cells
                            for (var cellIndex = 1; cellIndex <= columnMapping.Count; cellIndex++)
                            {
                                var column = columnMapping[cellIndex];

                                foreach (Range columnCells in cells.Columns)
                                {
                                    if (columnCells.Column == column)
                                    {
                                        rowResults.Add(cellIndex, columnCells.Value);
                                    }
                                }
                            }
                        }
                        finally
                        {
                            Marshal.ReleaseObject(cells);
                        }

                        this.PopulateTree(root, rowResults, 1);
                    }
                }
                finally
                {
                    Marshal.ReleaseObject(rows);
                }
            }
            finally
            {
                Marshal.ReleaseObject(worksheet);
                Marshal.ReleaseObject(dataTable);
                System.GC.Collect();
            }

            return root;
        }

        private void PopulateTree(MyTree<string> parent, Dictionary<int, string> rowResults, int level)
        {
            var results = parent.Where(tree => tree.Value.Equals(rowResults[level]));
            MyTree<string> node;
            if (!results.Any())
            {
                // no matching results
                MyTree<string> subTree = new MyTree<string> { Value = rowResults[level] };
                parent.Add(subTree);
                node = subTree;
            }
            else
            {
                node = results.First();
            }

            var newLevel = level + 1;
            if (newLevel <= rowResults.Count)
            {
                this.PopulateTree(node, rowResults, level + 1);
            }
        }
    }

    public class MyTree<V> : HashSet<MyTree<V>>
    {
        public V Value { get; set; }
    }
}