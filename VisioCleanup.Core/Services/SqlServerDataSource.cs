// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlServerDataSource.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
    /// <summary>The database connection.</summary>
    private SqlConnection? databaseConnection;

    /// <summary>Initialises a new instance of the <see cref="SqlServerDataSource"/> class. Initialises a new instance of the<see cref="SqlServerDataSource"/> class.</summary>
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
    public void Dispose() => this.Dispose(native: true);

    /// <inheritdoc />
    public void Open()
    {
        SqlConnectionStringBuilder builder = new ()
        {
            DataSource = this.AppConfig.DatabaseServer,
            Authentication = SqlAuthenticationMethod.ActiveDirectoryIntegrated,
            InitialCatalog = this.AppConfig.DatabaseCatalog,
            ApplicationIntent = ApplicationIntent.ReadOnly,
            TrustServerCertificate = true,
        };

        this.databaseConnection = new SqlConnection(builder.ConnectionString);
        this.databaseConnection.Open();
    }

    /// <inheritdoc />
    public void RetrieveRecords(string parameter, DiagramShape masterShape)
    {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
        using SqlCommand command = new (parameter, this.databaseConnection);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
        using var reader = command.ExecuteReader();

        Dictionary<string, DiagramShape> allShapes = new (StringComparer.Ordinal);

        // map columns
        var columnMapping = this.MapColumns(reader.GetColumnSchema());

        while (reader.Read())
        {
            var rowResults = ConvertRowToValues(columnMapping, reader);

            DiagramShape? result = null;
            for (var i = 0; i < columnMapping.Length; i++)
            {
                result = this.CreateShape(rowResults[i], allShapes, result);

                if (result is not null && result.ParentShape is null)
                {
                    masterShape.AddChildShape(result);
                }
            }
        }
    }

    /// <summary>The convert row to values.</summary>
    /// <param name="columnMapping">The column mapping.</param>
    /// <param name="reader">The reader.</param>
    /// <returns>The <see><cref>Dictionary</cref></see> .</returns>
    private static Dictionary<FieldType, string>[] ConvertRowToValues(Dictionary<FieldType, int>[] columnMapping, SqlDataReader reader)
    {
        var rowResults = new Dictionary<FieldType, string>[columnMapping.Length];
        for (var cellIndex = 0; cellIndex < columnMapping.Length; cellIndex++)
        {
            var columnMap = columnMapping[cellIndex];
            Dictionary<FieldType, string> values = new ();

            foreach (var (key, value) in columnMap)
            {
                values[key] = !reader.IsDBNull(value) ? reader.GetFieldValue<string>(value) : string.Empty;
            }

            rowResults[cellIndex] = values;
        }

        return rowResults;
    }

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

    /// <summary>The map columns.</summary>
    /// <param name="columnSchema">The column schema.</param>
    /// <returns>The <see><cref>Dictionary</cref></see> .</returns>
    private Dictionary<FieldType, int>[] MapColumns(IReadOnlyCollection<DbColumn> columnSchema)
    {
        var columnMapping = Array.Empty<Dictionary<FieldType, int>>();
        var level = -1;

        do
        {
            level++;
            Dictionary<FieldType, int> mappings = new ();

            var fieldName = string.Format(CultureInfo.InvariantCulture, this.AppConfig.FieldLabelFormat!, level);
            var sortFieldName = string.Format(CultureInfo.InvariantCulture, this.AppConfig.SortFieldLabelFormat!, level);
            var shapeFieldName = string.Format(CultureInfo.InvariantCulture, this.AppConfig.ShapeTypeLabelFormat!, level);

            Array.Resize(ref columnMapping, level + 1);
            foreach (var column in columnSchema)
            {
                Debug.Assert(column.ColumnOrdinal is not null, "column.ColumnOrdinal is not  null");

                var columnOrdinal = (int)column.ColumnOrdinal;
                var columnName = column.ColumnName;

                if (columnName!.Equals(fieldName, StringComparison.Ordinal))
                {
                    mappings[FieldType.ShapeText] = columnOrdinal;
                    columnMapping[level] = mappings;
                }
                else if (columnName.Equals(sortFieldName, StringComparison.Ordinal))
                {
                    mappings[FieldType.SortValue] = columnOrdinal;
                }
                else if (columnName.Equals(shapeFieldName, StringComparison.Ordinal))
                {
                    mappings[FieldType.ShapeType] = columnOrdinal;
                }
                else
                {
                    // ignoring value
                }
            }
        }
        while (columnMapping.ElementAtOrDefault(level) is not null);

        Array.Resize(ref columnMapping, level);

        return columnMapping;
    }
}
