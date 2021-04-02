﻿// -----------------------------------------------------------------------
// <copyright file="DatabaseService.cs" company="Jolyon Suthers">
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

    /// <summary>The database service.</summary>
    public class DatabaseService : AbstractProcessingService, IDatabaseService
    {
        private IIServerDatabaseApplication iserverDatabaseApplication;

        /// <summary>Initialises a new instance of the <see cref="DatabaseService" /> class.</summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="visioApplication">Visio application handler.</param>
        /// <param name="iserverDatabaseApplication">iServer Database application handler.</param>
        /// <param name="options">Application configuration being passed in.</param>
        public DatabaseService(
            ILogger<DatabaseService> logger,
            IVisioApplication visioApplication,
            IIServerDatabaseApplication iserverDatabaseApplication,
            IOptions<AppConfig> options)
            : base(logger, options, visioApplication) =>
            this.iserverDatabaseApplication = iserverDatabaseApplication ?? throw new ArgumentNullException(nameof(iserverDatabaseApplication));

        /// <inheritdoc />
        public Task ProcessDataSet(string sqlCommand)
        {
            return Task.Run(
                () =>
                    {
                        try
                        {
                            // open connection to excel
                            this.iserverDatabaseApplication.Open();

                            // open connection to visio
                            this.VisioApplication.Open();
                            List<DiagramShape> shapes = new();

                            // master shape
                            this.Logger.LogInformation("Create a fake parent shape.");
                            this.MasterShape = new DiagramShape(0)
                                                   {
                                                       ShapeText = "FAKE MASTER",
                                                       ShapeType = ShapeType.FakeShape,
                                                       LeftSide = this.VisioApplication.GetPageLeftSide(),
                                                       TopSide = this.VisioApplication.GetPageTopSide() - DiagramShape.ConvertMeasurement(this.AppConfig.HeaderHeight),
                                                   };
                            shapes.Add(this.MasterShape);

                            // retrieve records
                            this.Logger.LogInformation("Loading excel data.");
                            shapes.AddRange(this.iserverDatabaseApplication.RetrieveRecords(sqlCommand));

                            if (shapes.Count == 1)
                            {
                                return;
                            }

                            this.AllShapes = new Collection<DiagramShape>(shapes);

                            this.Logger.LogInformation("Assigning fake parent.");
                            foreach (var shape in this.AllShapes.Where(shape => !shape.HasParent() && (shape.ShapeType != ShapeType.FakeShape)))
                            {
                                this.MasterShape.AddChildShape(shape);
                            }

                            // need to set children relationships.
                            this.Logger.LogInformation("Sorting shapes into lines.");
                            this.SortChildren(this.MasterShape);
                        }
                        finally
                        {
                            this.Logger.LogInformation("Closing connection to visio.");
                            this.VisioApplication.Close();

                            this.Logger.LogInformation("Closing connection to database.");
                            this.iserverDatabaseApplication.Close();
                        }
                    });
        }
    }
}