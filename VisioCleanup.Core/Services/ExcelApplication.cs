// -----------------------------------------------------------------------
// <copyright file="ExcelApplication.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services
{
    using System;

    using Microsoft.Extensions.Logging;
    using Microsoft.Office.Interop.Excel;

    using VisioCleanup.Core.Contracts;

    /// <inheritdoc />
    internal class ExcelApplication : IExcelApplication
    {
        private readonly ILogger<ExcelApplication> logger;

        private Application? excelApplication;

        /// <summary>Initialises a new instance of the <see cref="ExcelApplication" /> class.</summary>
        /// <param name="logger">Logging instance.</param>
        public ExcelApplication(ILogger<ExcelApplication> logger) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <inheritdoc />
        public void Close()
        {
            this.logger.LogDebug("Releasing excel application.");
            this.excelApplication = null;

            this.logger.LogDebug("Cleaning up.");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <inheritdoc />
        public void Open()
        {
            this.logger.LogDebug("Opening connection to excel.");
            this.excelApplication = Marshal.GetActiveObject("Excel.Application") as Application ?? throw new InvalidOperationException("Excel must be running.");
        }
    }
}