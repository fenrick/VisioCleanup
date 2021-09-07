// -----------------------------------------------------------------------
// <copyright file="SQLServerDataSource.cs" company="Jolyon Suthers">
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
    using System.Globalization;
    using System.Linq;

    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models;
    using VisioCleanup.Core.Models.Config;
    using VisioCleanup.Core.Resources;

    /// <inheritdoc />
    public class SQLServerDataSource : AbstractDataSource, ISQLServerDataSource, IDisposable
    {
        private SqlConnection? databaseConnection;

        /// <summary>Initialises a new instance of the <see cref="SQLServerDataSource" /> class.</summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="options">Configuration options.</param>
        public SQLServerDataSource(ILogger<SQLServerDataSource> logger, IOptions<AppConfig> options)
            : base(logger, options)
        {
        }

        /// <inheritdoc />
        public string Name => "SQL Server";

        /// <inheritdoc />
        public void Close()
        {
            if (this.databaseConnection is null)
            {
                throw new InvalidOperationException(en_AU.SqlServerDatabaseApplication_Close_Open_database_first_);
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
                                                         DataSource = this.AppConfig.DatabaseServer,
                                                         Authentication = SqlAuthenticationMethod.ActiveDirectoryIntegrated,
                                                         InitialCatalog = this.AppConfig.DatabaseCatalog,
                                                         ApplicationIntent = ApplicationIntent.ReadOnly,
                                                     };

            this.databaseConnection = new SqlConnection(builder.ConnectionString);
            this.databaseConnection.Open();
        }

        /// <inheritdoc />
        public IEnumerable<DiagramShape> RetrieveRecords(string parameter)
        {
            using SqlCommand command = new(parameter, this.databaseConnection);
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

        /// <summary>Native/Managed Dispose.</summary>
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

        private SortedList<int, Dictionary<FieldType, int>> MapColumns(IReadOnlyCollection<DbColumn> columnSchema)
        {
            SortedList<int, Dictionary<FieldType, int>> columnMapping = new();
            var level = 0;
            do
            {
                Dictionary<FieldType, int> mappings = new();
                var fieldName = string.Format(CultureInfo.CurrentCulture, this.AppConfig.FieldLabelFormat ?? en_AU.SqlServerDatabaseApplication_MapColumns__0_, level);
                var sortFieldName = string.Format(CultureInfo.CurrentCulture, this.AppConfig.SortFieldLabelFormat ?? en_AU.ExcelApplication_FindHeaders__0__SortValue, level);
                var shapeFieldName = string.Format(CultureInfo.CurrentCulture, this.AppConfig.ShapeTypeLabelFormat ?? en_AU.ExcelApplication_FindHeaders__0__Shape, level);

                level++;

                foreach (var column in columnSchema)
                {
                    Debug.Assert(column.ColumnOrdinal is not null, en_AU.SqlServerDatabaseApplication_MapColumns_column_ColumnOrdinal_is_not__null);
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
