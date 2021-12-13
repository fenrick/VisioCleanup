// -----------------------------------------------------------------------
// <copyright file="DatabaseService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services;

using System;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using VisioCleanup.Core.Contracts;
using VisioCleanup.Core.Models.Config;

/// <summary>The database service.</summary>
public class DatabaseService : AbstractProcessingService, IDatabaseService
{
    private readonly ISqlServerDataSource iserverDatabaseApplication;

    /// <summary>Initialises a new instance of the <see cref="DatabaseService" /> class.</summary>
    /// <param name="logger">Logging instance.</param>
    /// <param name="visioApplication">Visio application handler.</param>
    /// <param name="iserverDatabaseApplication">iServer Database application handler.</param>
    /// <param name="options">Application configuration being passed in.</param>
    public DatabaseService(ILogger<DatabaseService> logger, IVisioApplication visioApplication, ISqlServerDataSource iserverDatabaseApplication, IOptions<AppConfig> options)
        : base(logger, options, visioApplication) =>
        this.iserverDatabaseApplication = iserverDatabaseApplication ?? throw new ArgumentNullException(nameof(iserverDatabaseApplication));

    /// <inheritdoc />
    public void ProcessDataSet(string sqlCommand) => this.ProcessDataSetInternal(this.iserverDatabaseApplication, sqlCommand);
}
