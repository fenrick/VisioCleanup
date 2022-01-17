// -----------------------------------------------------------------------
// <copyright file="SqlServerDataSource.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services;

using System.Data.Common;
using System.Diagnostics;
using System.Globalization;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using VisioCleanup.Core.Contracts;
using VisioCleanup.Core.Models;
using VisioCleanup.Core.Models.Config;

/// <inheritdoc cref="ISqlServerDataSource" />
public sealed class SqlServerDataSource : AbstractDataSource, ISqlServerDataSource, IDisposable
{
    private SqlConnection? databaseConnection;

    /// <summary>Initialises a new instance of the <see cref="SqlServerDataSource" /> class.</summary>
    /// <param name="logger">Logging instance.</param>
    /// <param name="options">Configuration options.</param>
    public SqlServerDataSource(ILogger<SqlServerDataSource> logger, IOptions<AppConfig> options)
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
            throw new InvalidOperationException("Open database first.");
        }

        this.databaseConnection.Close();
        this.Dispose();
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
    public void RetrieveRecords(string parameter, DiagramShape masterShape)
    {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
        using SqlCommand command = new(parameter, this.databaseConnection);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
        using var reader = command.ExecuteReader();

        Dictionary<string, DiagramShape> allShapes = new(StringComparer.Ordinal);

        // map columns
        var columnMapping = this.MapColumns(reader.GetColumnSchema());

        while (reader.Read())
        {
            Dictionary<int, Dictionary<FieldType, string>> rowResults = new();
            foreach (var cellIndex in Enumerable.Range(1, columnMapping.Count))
            {
                Dictionary<FieldType, string> values = new();
                foreach (var pair in columnMapping[cellIndex])
                {
                    var key = pair.Key;
                    var value = pair.Value;
                    values[key] = !reader.IsDBNull(value) ? reader.GetFieldValue<string>(value) : string.Empty;
                }

                rowResults.Add(cellIndex, values);
            }

            DiagramShape? result = null;
            foreach (var i in Enumerable.Range(1, columnMapping.Count))
            {
                result = this.CreateShape(rowResults[i], allShapes, result);

                if (result is not null && result.ParentShape is null)
                {
                    masterShape.AddChildShape(result);
                }
            }
        }
    }

    /// <inheritdoc />
    public void Dispose() => this.Dispose(native: true);

    /// <summary>Native/Managed Dispose.</summary>
    /// <param name="native">Is this a native dispose.</param>
    private void Dispose(bool native)
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
            var fieldName = string.Format(CultureInfo.CurrentCulture, this.AppConfig.FieldLabelFormat ?? "{0}", level);
            var sortFieldName = string.Format(CultureInfo.CurrentCulture, this.AppConfig.SortFieldLabelFormat ?? "{0} SortValue", level);
            var shapeFieldName = string.Format(CultureInfo.CurrentCulture, this.AppConfig.ShapeTypeLabelFormat ?? "{0} Shape", level);

            level++;

            foreach (var column in columnSchema)
            {
                Debug.Assert(column.ColumnOrdinal is not null, "column.ColumnOrdinal is not  null");
                var columnColumnOrdinal = (int)column.ColumnOrdinal;
                var columnColumnName = column.ColumnName;
                FieldType fieldMapping;

                if (columnColumnName.Equals(fieldName, StringComparison.Ordinal))
                {
                    fieldMapping = FieldType.ShapeText;
                }
                else if (columnColumnName.Equals(sortFieldName, StringComparison.Ordinal))
                {
                    fieldMapping = FieldType.SortValue;
                }
                else if (columnColumnName.Equals(shapeFieldName, StringComparison.Ordinal))
                {
                    fieldMapping = FieldType.ShapeType;
                }
                else
                {
                    continue;
                }

                mappings.Add(fieldMapping, columnColumnOrdinal);
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
