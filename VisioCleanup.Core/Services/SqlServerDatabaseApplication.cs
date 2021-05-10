// -----------------------------------------------------------------------
// <copyright file="SqlServerDatabaseApplication.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models;
    using VisioCleanup.Core.Models.Config;

    /// <inheritdoc />
    public class SqlServerDatabaseApplication : ISqlServerDatabaseApplication, IDisposable
    {
        private readonly AppConfig appConfig;

        private readonly ILogger<SqlServerDatabaseApplication> logger;

        private SqlConnection? databaseConnection;

        /// <summary>Initialises a new instance of the <see cref="SqlServerDatabaseApplication" /> class.</summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="options">Configuration options.</param>
        public SqlServerDatabaseApplication(ILogger<SqlServerDatabaseApplication> logger, IOptions<AppConfig> options)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.appConfig = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        public void Close()
        {
            if (this.databaseConnection is null)
            {
                throw new InvalidOperationException("Open database first.");
            }

            this.databaseConnection.Close();
            this.Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Open()
        {
            SqlConnectionStringBuilder builder = new()
                                                     {
                                                         DataSource = this.appConfig.DatabaseServer,
                                                         Authentication = SqlAuthenticationMethod.ActiveDirectoryIntegrated,
                                                         InitialCatalog = this.appConfig.DatabaseCatalog,
                                                         ApplicationIntent = ApplicationIntent.ReadOnly,
                                                     };

            this.databaseConnection = new SqlConnection(builder.ConnectionString);
            this.databaseConnection.Open();
        }

        /// <inheritdoc />
        public IEnumerable<DiagramShape> RetrieveRecords(string sqlCommand)
        {
            using SqlCommand command = new(sqlCommand, this.databaseConnection);
            using var reader = command.ExecuteReader();

            Dictionary<string, DiagramShape> allShapes = new();

            // map columns
            var columnMapping = this.MapColumns(reader.GetColumnSchema());

            while (reader.Read())
            {
                Dictionary<int, Dictionary<FieldType, string>> rowResults = new();
                foreach (var cellIndex in Enumerable.Range(1, columnMapping.Count))
                {
                    Dictionary<FieldType, string> values = new();

                    foreach (var (key, value) in columnMapping[cellIndex])
                    {
                        values[key] = !reader.IsDBNull(value) ? reader.GetFieldValue<string>(value) : string.Empty;
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

        /// <summary>Native/Managed Dispose</summary>
        /// <param name="native">Is this a native dispose.</param>
        protected virtual void Dispose(bool native)
        {
            if (!native)
            {
                return;
            }

            if (this.databaseConnection is null)
            {
                return;
            }

            this.databaseConnection.Close();
            this.databaseConnection.Dispose();
            this.databaseConnection = null;
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

        private SortedList<int, Dictionary<FieldType, int>> MapColumns(IReadOnlyCollection<DbColumn> columnSchema)
        {
            SortedList<int, Dictionary<FieldType, int>> columnMapping = new();
            var level = 0;
            do
            {
                Dictionary<FieldType, int> mappings = new();
                var fieldName = string.Format(this.appConfig.FieldLabelFormat ?? "{0}", level);
                var sortFieldName = string.Format(this.appConfig.SortFieldLabelFormat ?? "{0} SortValue", level);
                var shapeFieldName = string.Format(this.appConfig.ShapeTypeLabelFormat ?? "{0} Shape", level);

                level++;

                foreach (var column in columnSchema)
                {
                    Debug.Assert(column.ColumnOrdinal is not null, "column.ColumnOrdinal is not  null");
                    var columnColumnOrdinal = (int)column.ColumnOrdinal;
                    var columnColumnName = column.ColumnName;
                    FieldType? fieldMapping = null;
                    if (columnColumnName.Equals(fieldName, StringComparison.Ordinal))
                    {
                        fieldMapping = FieldType.ShapeText;
                    }

                    if (columnColumnName.Equals(sortFieldName, StringComparison.Ordinal))
                    {
                        fieldMapping = FieldType.SortValue;
                    }

                    if (columnColumnName.Equals(shapeFieldName, StringComparison.Ordinal))
                    {
                        fieldMapping = FieldType.ShapeType;
                    }

                    if (fieldMapping is null)
                    {
                        continue;
                    }

                    mappings.Add((FieldType)fieldMapping, columnColumnOrdinal);
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
