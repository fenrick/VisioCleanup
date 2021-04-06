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

    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models;
    using VisioCleanup.Core.Models.Config;

    /// <inheritdoc />
    public class SqlServerDatabaseApplication : ISqlServerDatabaseApplication
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
            this.databaseConnection.Dispose();
        }

        /// <inheritdoc />
        public void Open()
        {
            SqlConnectionStringBuilder builder = new()
                                                     {
                                                         DataSource = this.appConfig.DatabaseServer,
                                                         Authentication = SqlAuthenticationMethod.ActiveDirectoryIntegrated,
                                                         InitialCatalog = this.appConfig.DatabaseCatalog,
                                                     };

            this.databaseConnection = new SqlConnection(builder.ConnectionString);
            this.databaseConnection.Open();
        }

        /// <inheritdoc />
        public IEnumerable<DiagramShape> RetrieveRecords(string sqlCommand)
        {
            using SqlCommand command = new(sqlCommand, this.databaseConnection);
            using SqlDataReader reader = command.ExecuteReader();
            var lineCount = 0;
            while (reader.Read())
            {
                lineCount++;
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    this.logger.LogDebug("{Line} - {Field} {Value}", lineCount, reader.GetName(i), reader.GetString(i));
                }
            }

            return new List<DiagramShape>();
        }
    }
}