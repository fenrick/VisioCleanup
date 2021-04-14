// -----------------------------------------------------------------------
// <copyright file="ExcelService.cs" company="Jolyon Suthers">
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
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models;
    using VisioCleanup.Core.Models.Config;

    /// <inheritdoc />
    /// <summary>The excel service.</summary>
    public class ExcelService : AbstractProcessingService, IExcelService
    {
        private readonly IExcelApplication excelApplication;

        /// <inheritdoc />
        /// <summary>Initialises a new instance of the <see cref="T:VisioCleanup.Core.Services.ExcelService" /> class.</summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="visioApplication">Visio application handler.</param>
        /// <param name="excelApplication">Excel application handler.</param>
        /// <param name="options">Application configuration being passed in.</param>
        public ExcelService(ILogger<ExcelService> logger, IVisioApplication visioApplication, IExcelApplication excelApplication, IOptions<AppConfig> options)
            : base(logger, options, visioApplication) =>
            this.excelApplication = excelApplication ?? throw new ArgumentNullException(nameof(excelApplication));

        /// <inheritdoc />
        public Task ProcessDataSet()
        {
            return Task.Run(
                () =>
                    {
                        try
                        {
                            // open connection to excel
                            this.excelApplication.Open();

                            // open connection to visio
                            this.VisioApplication.Open();
                            List<DiagramShape> shapes = new();

                            // master shape
                            this.Logger.LogInformation("Create a fake parent shape");
                            this.MasterShape = new DiagramShape(0)
                                                   {
                                                       ShapeText = "FAKE MASTER",
                                                       ShapeType = ShapeType.FakeShape,
                                                       LeftSide = this.VisioApplication.GetPageLeftSide(),
                                                       TopSide = this.VisioApplication.GetPageTopSide() - DiagramShape.ConvertMeasurement(this.AppConfig.HeaderHeight),
                                                   };
                            shapes.Add(this.MasterShape);

                            var maxRight = this.VisioApplication.GetPageRightSide() - DiagramShape.ConvertMeasurement(this.AppConfig.SidePanelWidth);
                            this.convertedAppConfigRight = DiagramShape.ConvertMeasurement(this.AppConfig.Right);

                            // retrieve records
                            this.Logger.LogInformation("Loading excel data");
                            shapes.AddRange(this.excelApplication.RetrieveRecords());

                            if (shapes.Count == 1)
                            {
                                return;
                            }

                            this.AllShapes = new Collection<DiagramShape>(shapes);

                            this.Logger.LogInformation("Assigning fake parent");
                            foreach (var shape in this.AllShapes.Where(shape => !shape.HasParent() && (shape.ShapeType != ShapeType.FakeShape)))
                            {
                                this.MasterShape.AddChildShape(shape);
                            }

                            // need to set children relationships.
                            this.Logger.LogInformation("Sorting shapes into lines");
                            this.SortChildren(this.MasterShape, maxRight);
                        }
                        finally
                        {
                            this.Logger.LogInformation("Closing connection to visio");
                            this.VisioApplication.Close();

                            this.Logger.LogInformation("Closing connection to excel");
                            this.excelApplication.Close();
                        }
                    });
        }
    }
}