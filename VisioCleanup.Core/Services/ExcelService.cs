// -----------------------------------------------------------------------
// <copyright file="ExcelService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models;
    using VisioCleanup.Core.Models.Config;

    /// <summary>The excel service.</summary>
    public class ExcelService : AbstractProcessingService, IExcelService
    {
        private readonly IExcelApplication excelApplication;

        /// <summary>Initialises a new instance of the <see cref="ExcelService" /> class.</summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="visioApplication">Visio application handler.</param>
        /// <param name="excelApplication">Excel application handler.</param>
        /// <param name="options">Application configuration being passed in.</param>
        public ExcelService(ILogger<ExcelService> logger, IVisioApplication visioApplication, IExcelApplication excelApplication, IOptions<AppConfig> options)
            : base(logger, options, visioApplication) =>
            this.excelApplication = excelApplication ?? throw new ArgumentNullException(nameof(excelApplication));

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

                            // open connection to visio
                            this.VisioApplication.Open();

                            // retrieve records
                            this.AllShapes = this.excelApplication.RetrieveRecords();

                            if (this.AllShapes.Count == 0)
                            {
                                return;
                            }

                            this.MasterShape = new DiagramShape(0) { ShapeText = "FAKE MASTER", ShapeType = ShapeType.FakeShape };

                            this.MasterShape.LeftSide = this.VisioApplication.GetPageLeftSide();
                            this.MasterShape.TopSide = this.VisioApplication.GetPageTopSide() - DiagramShape.ConvertMeasurement(this.AppConfig.HeaderHeight);

                            foreach (var shape in this.AllShapes.Where(shape => !shape.HasParent()))
                            {
                                this.MasterShape.AddChildShape(shape);
                            }

                            // need to set children relationships.
                        }
                        finally
                        {
                            this.VisioApplication.Close();

                            this.excelApplication.Close();
                        }
                    });
        }
    }
}