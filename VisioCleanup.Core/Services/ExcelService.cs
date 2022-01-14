// -----------------------------------------------------------------------
// <copyright file="ExcelService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using VisioCleanup.Core.Contracts;
using VisioCleanup.Core.Models.Config;

/// <summary>The excel service.</summary>
public class ExcelService : AbstractProcessingService, IExcelService
{
    private readonly IExcelDataSource dataSource;

    /// <summary>Initialises a new instance of the <see cref="ExcelService" /> class.</summary>
    /// <param name="logger">Logging instance.</param>
    /// <param name="visioApplication">Visio application handler.</param>
    /// <param name="dataSource">Excel application handler.</param>
    /// <param name="options">Application configuration being passed in.</param>
    public ExcelService(ILogger<ExcelService> logger, IVisioApplication visioApplication, IExcelDataSource dataSource, IOptions<AppConfig> options)
        : base(logger, options, visioApplication) =>
        this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));

    /// <inheritdoc />
    public void ProcessDataSet() => this.ProcessDataSetInternal(this.dataSource, string.Empty);
}
