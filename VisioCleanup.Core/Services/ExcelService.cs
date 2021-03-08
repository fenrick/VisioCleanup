// -----------------------------------------------------------------------
// <copyright file="ExcelService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using VisioCleanup.Core.Contracts;

    /// <summary>The excel service.</summary>
    public class ExcelService : AbstractProcessingService, IExcelService
    {
        private readonly IExcelApplication excelApplication;

        /// <summary>Initialises a new instance of the <see cref="ExcelService" /> class.</summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="visioApplication">Visio application handler.</param>
        /// <param name="excelApplication">Excel application handler.</param>
        public ExcelService(ILogger<ExcelService> logger, IVisioApplication visioApplication, IExcelApplication excelApplication)
            : base(logger, visioApplication)
        {
            this.excelApplication = excelApplication ?? throw new ArgumentNullException(nameof(excelApplication));
        }

        /// <inheritdoc />
        public async Task ProcessDataSet()
        {
            await Task.Run(
                () =>
                    {
                        try
                        {
                            // open connection to excel
                            this.excelApplication.Open();
                        }
                        finally
                        {
                            this.excelApplication.Close();
                        }
                    });
        }
    }
}