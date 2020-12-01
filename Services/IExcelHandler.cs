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

        /// <summary>
        ///     Retrieve records from excel.
        /// </summary>
        /// <returns>Tree of results.</returns>
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
        /// <exception cref="T:System.InvalidOperationException">Excel must be running.</exception>
        public void Open()
        {
            this.logger.LogDebug("Opening connection to excel.");
            this.excelApplication = Marshal.GetActiveObject("Excel.Application") as Application ?? throw new InvalidOperationException("Excel must be running.");
        }

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">System not initialised.</exception>
        public MyTree<string> RetrieveRecords()
        {
            if (this.excelApplication == null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            var dataTable = this.excelApplication.ActiveSheet.ListObjects[1];
            var root = new MyTree<string> { Value = "ROOT" };

            // find headers
            SortedList<int, int> columnMapping = this.FindHeaders(dataTable);

            // process rows
            Range rows = dataTable.DataBodyRange.Rows;
            for (var rowIndex = 1; rowIndex <= rows.Count; rowIndex++)
            {
                var rowResults = GenerateRow(rows, rowIndex, columnMapping);

                this.PopulateTree(root, rowResults, 1);
            }

            return root;
        }

        private static Dictionary<int, string> GenerateRow(Range rows, int rowIndex, IReadOnlyDictionary<int, int> columnMapping)
        {
            Range? cells = null;
            try
            {
                var rowResults = new Dictionary<int, string>();
                cells = rows[rowIndex];
                for (var cellIndex = 1; cellIndex <= columnMapping.Count; cellIndex++)
                {
                    foreach (var columnCells in cells.Columns.Cast<Range>().Where(columnCells => columnCells.Column == columnMapping[cellIndex]))
                    {
                        rowResults.Add(cellIndex, columnCells.Value);
                    }
                }

                return rowResults;
            }
            finally
            {
                Marshal.ReleaseObject(cells);
            }
        }

        private SortedList<int, int> FindHeaders(ListObject dataTable)
        {
            var columnMapping = new SortedList<int, int>();
            var level = 0;
            do
            {
                var fieldName = string.Format(this.settings.ExcelHeaderFormat ?? "{0}", level);

                level++;
                foreach (var cell in dataTable.HeaderRowRange.Cast<Range>().Where(cell => cell.Value.Equals(fieldName)))
                {
                    columnMapping.Add(level, cell.Column);
                }
            }
            while (columnMapping.ContainsKey(level));

            return columnMapping;
        }

        private void PopulateTree(MyTree<string> parent, IReadOnlyDictionary<int, string> rowResults, int level)
        {
            var results = parent.Where(tree => tree.Value.Equals(rowResults[level])).ToList();
            MyTree<string> node;
            if (results.Count > 0)
            {
                node = results[0];
            }
            else
            {
                // no matching results
                var subTree = new MyTree<string> { Value = rowResults[level] };
                parent.Add(subTree);
                node = subTree;
            }

            var newLevel = level + 1;
            if (newLevel <= rowResults.Count)
            {
                this.PopulateTree(node, rowResults, level + 1);
            }
        }
    }
}